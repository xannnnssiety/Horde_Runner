using UnityEngine;
using System.Collections.Generic; // Потребуется, если будете добавлять сложные списки или словари

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Targeting")]
    private Transform playerTransform; // Для ориентации и как фоллбэк
    private PlayerMovement playerMovementScript; // Для получения скорости игрока

    [Header("Speed Settings")]
    public float playerSpeedMultiplier = 0.8f; // Процент от скорости игрока
    public float minEnemySpeed = 2f;           // Минимальная скорость врага

    [Header("Movement Behavior")]
    // stoppingDistance теперь определяет, насколько близко враг подходит к своему ЦЕЛЕВОМУ СЛОТУ (или к игроку в режиме фоллбэка)
    public float stoppingDistance = 0.5f;
    public float rotationSpeed = 10f;       // Скорость поворота врага

    [Header("Formation")]
    private EnemyFormationManager.FormationSlot currentFormationSlot;
    private Vector3 targetSlotWorldPosition; // Позиция слота в мире, к которой стремится враг

    // Компоненты
    private Rigidbody rb;
    private SphereCollider sphereCollider;

    [Header("Collision Settings")]
    public LayerMask environmentLayerMask; // Слой для стен и препятствий
    public float collisionOffset = 0.05f; // Небольшой отступ от стены после столкновения

    [Header("Vertical Constraints")]
    public bool constrainYPosition = true; // Включить/выключить ограничение по Y
    public float minYPosition = 0.0f;     // Минимальная высота Y для врага
    public float maxYPosition = 0.5f;     // Максимальная высота Y для врага


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) { Debug.LogError("EnemyAI: Rigidbody не найден!", gameObject); enabled = false; return; }

        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null) { Debug.LogWarning("EnemyAI: SphereCollider не найден, будет использован дефолтный радиус для SphereCast.", gameObject); }
    }

    public void Initialize(Transform targetPlayer, PlayerMovement pMovementScript)
    {
        playerTransform = targetPlayer;
        playerMovementScript = pMovementScript;
        currentHealth = maxHealth;

        if (playerMovementScript == null) { Debug.LogError("EnemyAI: PlayerMovement script не передан для " + name, gameObject); }
        // Начальный поворот не так важен, так как он будет поворачиваться к слоту/игроку
    }

    void Start()
    {
        RequestFormationSlot();
    }

    void OnEnable()
    {
        // Если враг был деактивирован и снова активирован (например, из пула объектов)
        // Убедимся, что он запрашивает слот, если у него его еще нет или он был освобожден
        if (currentFormationSlot == null || !currentFormationSlot.isOccupied || currentFormationSlot.assignedEnemy != this)
        {
            RequestFormationSlot();
        }
    }

    void OnDisable()
    {
        ReleaseFormationSlot();
    }

    void OnDestroy()
    {
        ReleaseFormationSlot();
    }

    void RequestFormationSlot()
    {
        if (currentFormationSlot != null && currentFormationSlot.assignedEnemy == this) return; // Уже есть валидный слот

        if (EnemyFormationManager.Instance != null)
        {
            currentFormationSlot = EnemyFormationManager.Instance.RequestSlot(this);
            if (currentFormationSlot == null)
            {
                // Debug.LogWarning($"{name}: Не удалось получить слот в формации. Активирован режим фоллбэка.");
            }
        }
        else
        {
            // Debug.LogWarning($"{name}: EnemyFormationManager.Instance не найден. Активирован режим фоллбэка.");
        }
    }

    void ReleaseFormationSlot()
    {
        if (currentFormationSlot != null && EnemyFormationManager.Instance != null)
        {
            // Проверяем, действительно ли этот враг занимает слот, прежде чем освобождать
            if (currentFormationSlot.assignedEnemy == this)
            {
                EnemyFormationManager.Instance.ReleaseSlot(this);
            }
            currentFormationSlot = null;
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 targetPositionForMovement = rb.position; 
        bool hasValidTarget = false;
        

        if (currentFormationSlot != null && EnemyFormationManager.Instance != null)
        {
            // Если есть слот, он является приоритетной целью
            targetPositionForMovement = EnemyFormationManager.Instance.GetWorldPositionForSlot(currentFormationSlot);
            hasValidTarget = true;
        }
        else if (playerTransform != null)
        {
            // Фоллбэк: если нет слота, но есть игрок - стремиться к позиции с отступом от игрока
            // Отступ сзади игрока
            Vector3 offsetDirection = -playerTransform.forward;
            // Учитываем и боковое смещение, чтобы не все толпились строго сзади
            // Можно добавить небольшое случайное боковое смещение или использовать ID врага для детерминированного смещения
            float fallbackOffsetDistance = stoppingDistance + 2.0f; // Расстояние для фоллбэка
            targetPositionForMovement = playerTransform.position + (offsetDirection * fallbackOffsetDistance);
            targetPositionForMovement.y = Mathf.Clamp(rb.position.y, minYPosition, maxYPosition); // Двигаться на текущей высоте Y врага
            hasValidTarget = true;
        }

        if (hasValidTarget)
        {
            HandleMovementToTarget(targetPositionForMovement);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);  // Останавливаем горизонтальное движение, если нет цели
        }

        if (constrainYPosition)
        {
            ConstrainYPosition();
        }
    }

    void HandleMovementToTarget(Vector3 targetPoint)
    {
        // 1. Определяем скорость врага
        float baseSpeed = (playerMovementScript != null) ? playerMovementScript.currentMoveSpeed : minEnemySpeed;
        float enemyTargetSpeed = baseSpeed * playerSpeedMultiplier;
        float actualEnemySpeed = Mathf.Max(enemyTargetSpeed, minEnemySpeed);

        // Если это фоллбэк на игрока и мы уже близко, замедляемся/останавливаемся
        if (currentFormationSlot == null && playerTransform != null)
        {
            if (Vector3.Distance(Vector3.ProjectOnPlane(rb.position, Vector3.up), Vector3.ProjectOnPlane(playerTransform.position, Vector3.up)) < stoppingDistance + 1.0f)
            {
                actualEnemySpeed *= 0.5f; // Замедление
            }
            if (Vector3.Distance(Vector3.ProjectOnPlane(rb.position, Vector3.up), Vector3.ProjectOnPlane(playerTransform.position, Vector3.up)) < stoppingDistance)
            {
                actualEnemySpeed = 0; // Остановка
            }
        }


        // 2. Определяем направление и расстояние до целевой точки
        Vector3 directionToTarget = targetPoint - rb.position;
        Vector3 directionToTargetHorizontal = Vector3.ProjectOnPlane(directionToTarget, Vector3.up);
        float distanceToTargetHorizontal = directionToTargetHorizontal.magnitude;
        Vector3 normalizedDirectionToTargetHorizontal = Vector3.zero;

        if (distanceToTargetHorizontal > 0.01f)
        {
            normalizedDirectionToTargetHorizontal = directionToTargetHorizontal.normalized;
        }

        Vector3 movementThisFrame = Vector3.zero;

        // 3. Логика движения или остановки у цели (слота или фоллбэк-точки)
        // Используем stoppingDistance как "допуск" для достижения цели
        // Если расстояние до цели больше stoppingDistance, то двигаемся
        if (distanceToTargetHorizontal > stoppingDistance)
        {
            movementThisFrame = normalizedDirectionToTargetHorizontal * actualEnemySpeed * Time.fixedDeltaTime;
        }


        // 4. Проверка столкновений со стенами
        if (movementThisFrame.magnitude > 0.001f)
        {
            RaycastHit hitInfo;
            float sphereCastRadius = sphereCollider != null ? sphereCollider.radius * GetMaxScaleComponent() : 0.5f;

            if (Physics.SphereCast(rb.position, sphereCastRadius, movementThisFrame.normalized, out hitInfo, movementThisFrame.magnitude, environmentLayerMask))
            {
                if (hitInfo.distance > collisionOffset)
                {
                    rb.MovePosition(rb.position + movementThisFrame.normalized * (hitInfo.distance - collisionOffset));
                }
                // Если столкнулись со стеной, дальнейшее движение по rb.MovePosition в этом кадре не нужно
            }
            else
            {
                rb.MovePosition(rb.position + movementThisFrame);
            }
        }

        // 5. Поворот
        Vector3 lookDirectionInput = Vector3.zero;
        // Если есть игрок, и мы либо в слоте (и слот близко к игроку), либо в режиме фоллбэка, смотрим на игрока.
        if (playerTransform != null)
        {
            // Если в слоте и слот уже "достигнут" (враг близко к нему), или если это фоллбэк
            bool isNearSlotOrFallback = (currentFormationSlot != null && distanceToTargetHorizontal <= stoppingDistance + 0.5f) || currentFormationSlot == null;

            if (isNearSlotOrFallback)
            {
                Vector3 dirToPlayer = playerTransform.position - rb.position;
                dirToPlayer.y = 0;
                if (dirToPlayer.sqrMagnitude > 0.01f) lookDirectionInput = dirToPlayer.normalized;
            }
            // Иначе, если мы активно движемся к слоту, смотрим на слот
            else if (normalizedDirectionToTargetHorizontal != Vector3.zero)
            {
                lookDirectionInput = normalizedDirectionToTargetHorizontal;
            }
        }
        // Если игрока нет, но есть слот и мы к нему движемся
        else if (normalizedDirectionToTargetHorizontal != Vector3.zero)
        {
            lookDirectionInput = normalizedDirectionToTargetHorizontal;
        }


        if (lookDirectionInput.sqrMagnitude > 0.001f) // Проверка, что вектор не нулевой
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirectionInput);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    void ConstrainYPosition()
    {
        Vector3 currentPosition = rb.position;
        float clampedY = Mathf.Clamp(currentPosition.y, minYPosition, maxYPosition);

        if (Mathf.Abs(currentPosition.y - clampedY) > 0.001f)
        {
            rb.position = new Vector3(currentPosition.x, clampedY, currentPosition.z);
            if (rb.linearVelocity.y < 0 && currentPosition.y <= minYPosition + 0.01f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }
        }
    }

    float GetMaxScaleComponent()
    {
        if (sphereCollider == null) return 1f;
        Vector3 scale = transform.localScale;
        return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        float currentActualSpeedDisplay = (playerMovementScript != null) ? Mathf.Max(playerMovementScript.currentMoveSpeed * playerSpeedMultiplier, minEnemySpeed) : minEnemySpeed;
        Debug.LogFormat("{0} получил {1} урона. Здоровье: {2}. Расчетная скорость: {3:F2}", gameObject.name, damageAmount, currentHealth, currentActualSpeedDisplay);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " умер!");
        ReleaseFormationSlot(); // Убедимся, что слот освобожден при смерти
        RunStatsManager.Instance.RegisterKill();
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnMouseDown()
    {
        if (playerMovementScript == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerMovementScript = playerObj.GetComponent<PlayerMovement>();
        }
        TakeDamage(10);
    }

    void OnDrawGizmosSelected()
    {
        // Показываем текущую целевую точку (слот или фоллбэк)
        if (Application.isPlaying && ((currentFormationSlot != null && EnemyFormationManager.Instance != null) || (currentFormationSlot == null && playerTransform != null)))
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(targetSlotWorldPosition, 0.3f); // targetSlotWorldPosition обновляется в FixedUpdate
            Gizmos.DrawLine(transform.position, targetSlotWorldPosition);
        }

        // Показываем stoppingDistance вокруг текущей позиции врага
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
#endif
}