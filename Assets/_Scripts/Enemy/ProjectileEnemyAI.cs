using UnityEngine;

public class ProjectileEnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Movement Within Formation")]
    public float formationSlotAttainSpeed = 15f; // Скорость, с которой враг стремится к своей точке в строю
    public float rotationSpeed = 8f;             // Скорость поворота к точке в строю или к игроку
    public float lifetime = 20f;
    private float spawnTime;

    // !!! --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ ОГРАНИЧЕНИЯ ВЫСОТЫ ---
    [Header("Y-Axis Constraints")]
    [Tooltip("Включить ограничение высоты полета?")]
    public bool constrainYPosition = false;
    [Tooltip("Минимальная высота (координата Y), на которой может находиться враг.")]
    public float minYPosition = 1.0f;
    [Tooltip("Максимальная высота (координата Y), на которой может находиться враг. Убедитесь, что Max > Min.")]
    public float maxYPosition = 10.0f;
    // !!! --- КОНЕЦ НОВЫХ ПЕРЕМЕННЫХ ---

    private SwarmController swarmController;    // Ссылка на контроллер роя
    private Vector3 localFormationOffset;       // Локальное смещение относительно контроллера роя

    private bool initialized = false;
    private bool isDead = false;

    private Rigidbody rb;
    private float lastPlayerContactTime;
    private float playerContactCooldown = 0.5f; // Кулдаун касания игрока

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) { Debug.LogError("ProjectileEnemyAI: Rigidbody не найден!", gameObject); enabled = false; return; }
        /*rb.useGravity = false;*/
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Initialize(SwarmController controller, Vector3 formationOffset)
    {
        currentHealth = maxHealth;
        isDead = false;
        swarmController = controller;
        localFormationOffset = formationOffset;
        spawnTime = Time.time;
        lastPlayerContactTime = -playerContactCooldown;

        if (swarmController == null)
        {
            Debug.LogError("ProjectileEnemyAI: SwarmController не был передан!", gameObject);
            initialized = false;
            Die("Нет SwarmController при инициализации");
            return;
        }

        swarmController.RegisterMember(this);

        float initialTinySpreadRadius = 0.1f;
        Vector3 tinyRandomOffset = Random.insideUnitSphere * initialTinySpreadRadius;
        tinyRandomOffset.y = 0;

        transform.position = swarmController.transform.position + tinyRandomOffset;
        transform.rotation = swarmController.transform.rotation;

        initialized = true;
    }

    void FixedUpdate()
    {
        if (!initialized || isDead || rb == null || swarmController == null) return;

        // 1. Рассчитываем целевую мировую позицию слота в формации
        Vector3 worldTargetSlotPosition = swarmController.transform.TransformPoint(localFormationOffset);

        // !!! --- НОВЫЙ БЛОК КОДА ДЛЯ ОГРАНИЧЕНИЯ ВЫСОТЫ ---
        // Если опция включена, применяем ограничение по оси Y к ЦЕЛЕВОЙ позиции
        if (constrainYPosition)
        {
            worldTargetSlotPosition.y = Mathf.Clamp(worldTargetSlotPosition.y, minYPosition, maxYPosition);
        }
        // !!! --- КОНЕЦ НОВОГО БЛОКА КОДА ---

        // 2. Рассчитываем направление к этой точке
        Vector3 directionToSlot = (worldTargetSlotPosition - rb.position);
        float distanceToSlot = directionToSlot.magnitude;

        // 3. Движение к слоту
        Vector3 movement = Vector3.zero;
        if (distanceToSlot > 0.01f)
        {
            movement = directionToSlot.normalized * Mathf.Min(distanceToSlot / Time.fixedDeltaTime, formationSlotAttainSpeed) * Time.fixedDeltaTime;
        }
        rb.MovePosition(rb.position + movement);

        // 4. Поворот
        Vector3 lookDirection;
        if (distanceToSlot > 0.2f && directionToSlot.normalized != Vector3.zero)
        {
            lookDirection = directionToSlot.normalized;
        }
        else
        {
            lookDirection = swarmController.transform.forward;
        }

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    void Update()
    {
        if (!initialized || isDead) return;
        if (Time.time >= spawnTime + lifetime)
        {
            Die("Время жизни истекло");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!initialized || isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastPlayerContactTime + playerContactCooldown)
            {
                Debug.Log(gameObject.name + " коснулся игрока (физически)!");
                lastPlayerContactTime = Time.time;
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // Die("Столкнулся со стеной");
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || !initialized) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die("Получен смертельный урон от игрока");
        }
    }

    private void Die(string reason = "Неизвестная причина")
    {
        if (isDead) return;
        isDead = true;
        // Проверяем, существует ли Instance, чтобы избежать ошибок при выходе из Play Mode
        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.RegisterKill();
        }

        if (swarmController != null)
        {
            swarmController.UnregisterMember(this);
        }
        Destroy(gameObject);
    }
}