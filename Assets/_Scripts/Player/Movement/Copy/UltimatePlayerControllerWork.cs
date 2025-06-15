using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class UltimatePlayerControllerWork : MonoBehaviour
{
    // --- ПЕРЕЧИСЛЕНИЕ СОСТОЯНИЙ ---
    private enum PlayerState
    {
        Grounded,
        InAir,
        Grinding
    }

    // --- ПУБЛИЧНЫЕ НАСТРОЙКИ (видны в инспекторе) ---

    [Header("Ссылки")]
    public Camera mainCamera;
    [Tooltip("Перетащите сюда текстовый элемент (Legacy) для отображения скорости")]
    public Text speedDisplayText;

    [Header("Настройки движения")]
    public float baseMoveSpeed = 5f;
    public float maxMoveSpeed = 10f;
    [Tooltip("Как быстро персонаж набирает/сбрасывает скорость на земле")]
    public float speedChangeRate = 2f;
    public float turnSmoothTime = 0.1f;

    [Header("Настройки прыжка и гравитации")]
    public float jumpHeight = 2f;
    public float gravity = -19.62f;
    public float coyoteTime = 0.15f;

    [Tooltip("Насколько резко персонаж меняет направление в воздухе")]
    public float airControlRate = 10f;
    [Tooltip("Насколько быстро персонаж останавливается на земле (трение)")]
    public float groundFriction = 10f;

    // VVVV --- НОВОЕ --- VVVV
    [Header("Настройки отскока от стены")]
    [Tooltip("Слой для стен, от которых можно отталкиваться")]
    public LayerMask wallJumpableLayer;
    [Tooltip("Как медленно персонаж скользит по стене вниз")]
    public float wallSlideSpeed = 2f;
    [Tooltip("Высота прыжка от стены. По умолчанию в 2 раза выше обычного.")]
    public float wallJumpHeight = 4f;
    [Tooltip("Сила отталкивания в сторону от стены")]
    public float wallJumpSidewaysForce = 8f;
    [Tooltip("Дистанция для проверки стены перед персонажем")]
    public float wallCheckDistance = 0.5f;
    // ^^^^ --- КОНЕЦ НОВОГО --- ^^^^

    [Header("Настройки грайнда")]
    [Tooltip("Множитель ускорения на рельсе (1.5 = на 50% быстрее)")]
    public float grindAccelerationMultiplier = 1.5f;
    public LayerMask grindableLayer;
    public float grindSearchRadius = 3f;

    [Header("Состояние (для отладки)")]
    [SerializeField] private PlayerState currentState;


    // --- ПРИВАТНЫЕ ПЕРЕМЕННЫЕ ---

    // Компоненты
    private CharacterController controller;
    private Animator animator;

    // Состояние
    private Vector3 playerVelocity;
    private float coyoteTimeCounter;
    private float turnSmoothVelocity;
    private float currentMoveSpeed;
    private Vector3 inputDirection;

    // Грайнд
    private Transform currentGrindRail;
    private Vector3 grindDirection;
    private float grindCooldownTimer;
    private const float GRIND_COOLDOWN = 0.2f;

    // VVVV --- НОВОЕ --- VVVV
    // Отскок от стены
    private bool isWallSliding;
    private Vector3 wallNormal;
    // ^^^^ --- КОНЕЦ НОВОГО --- ^^^^

    // ID Анимаций
    private readonly int animIDSpeed = Animator.StringToHash("Speed");
    private readonly int animIDJump = Animator.StringToHash("Jump");


    // --- ОСНОВНЫЕ МЕТОДЫ UNITY ---

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (mainCamera == null && Camera.main != null) mainCamera = Camera.main;
        else if (mainCamera == null) Debug.LogError("Камера не найдена и не назначена!");

        currentMoveSpeed = baseMoveSpeed;
        currentState = controller.isGrounded ? PlayerState.Grounded : PlayerState.InAir;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (grindCooldownTimer > 0)
        {
            grindCooldownTimer -= Time.deltaTime;
        }

        HandleGroundedAndCoyoteTime();
        HandleSpeed();
        HandleWallSliding(); // Проверяем состояние скольжения по стене

        switch (currentState)
        {
            case PlayerState.Grounded:
                HandleGroundedMovement();
                CheckForGrindStart();
                break;
            case PlayerState.InAir:
                HandleAirborneMovement();
                CheckForGrindStart();
                break;
            case PlayerState.Grinding:
                HandleGrinding();
                break;
        }

        // VVVV --- ИЗМЕНЕНИЕ --- VVVV
        // Гравитация применяется ко всем состояниям, кроме грайнда
        if (currentState != PlayerState.Grinding)
        {
            ApplyGravity();
        }

        // Применяем всё накопленное движение (горизонтальное + вертикальное) одним вызовом
        controller.Move(playerVelocity * Time.deltaTime);
        // ^^^^ --- КОНЕЦ ИЗМЕНЕНИЯ --- ^^^^

        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"Speed: {currentMoveSpeed:F2}";
        }
    }

    // --- МЕТОДЫ ОБРАБОТКИ СОСТОЯНИЙ ---

    private void HandleGroundedAndCoyoteTime()
    {
        // VVVV --- ИЗМЕНЕНИЕ --- VVVV
        // Мы считаем персонажа "на земле", если коллайдер касается земли ИЛИ если он скользит по стене.
        // Это предотвращает накопление гравитации во время скольжения по стене.
        bool isPhysicallyGrounded = controller.isGrounded;

        if (isPhysicallyGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // Немного прижимаем к земле
        }

        if (isPhysicallyGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            // Если мы приземлились, обновляем состояние
            if (currentState == PlayerState.InAir) SetState(PlayerState.Grounded);
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            // Если мы в воздухе и не скользим по стене/рельсе, то мы в полете
            if (currentState == PlayerState.Grounded && coyoteTimeCounter <= 0)
            {
                SetState(PlayerState.InAir);
            }
        }
        // ^^^^ --- КОНЕЦ ИЗМЕНЕНИЯ --- ^^^^
    }

    private void HandleSpeed()
    {
        bool shouldAccelerate = inputDirection.magnitude >= 0.1f || (currentState == PlayerState.Grinding && currentGrindRail != null);
        float targetSpeed = shouldAccelerate ? maxMoveSpeed : baseMoveSpeed;
        float currentSpeedChangeRate = speedChangeRate;

        if (currentState == PlayerState.Grinding && currentGrindRail != null)
        {
            currentSpeedChangeRate *= grindAccelerationMultiplier;
        }

        currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetSpeed, currentSpeedChangeRate * Time.deltaTime);
    }

    private void HandleGroundedMovement()
    {
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // VVVV --- ИЗМЕНЕНИЕ --- VVVV
            // Вместо Move() мы теперь задаем горизонтальную скорость
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            playerVelocity.x = moveDir.x * currentMoveSpeed;
            playerVelocity.z = moveDir.z * currentMoveSpeed;
            // ^^^^ --- КОНЕЦ ИЗМЕНЕНИЯ --- ^^^^

            animator.SetFloat(animIDSpeed, 1f);
        }
        else
        {
            // VVVV --- ИЗМЕНЕНИЕ --- VVVV
            // Плавная остановка
            playerVelocity.x = Mathf.Lerp(playerVelocity.x, 0, groundFriction * Time.deltaTime);
            playerVelocity.z = Mathf.Lerp(playerVelocity.z, 0, groundFriction * Time.deltaTime);
            // ^^^^ --- КОНЕЦ ИЗМЕНЕНИЯ --- ^^^^
            animator.SetFloat(animIDSpeed, 0f);
        }

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            animator.SetTrigger(animIDJump);
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            SetState(PlayerState.InAir);
            coyoteTimeCounter = 0;
        }
    }

    private void HandleAirborneMovement()
    {
        // Если мы отталкиваемся от стены, прыжок уже обработан в HandleWallSliding
        if (isWallSliding && Input.GetButtonDown("Jump"))
        {
            DoWallJump();
            return; // Выходим, чтобы не применять обычное управление в этот кадр
        }

        // VVVV --- ИЗМЕНЕНИЕ: СОХРАНЕНО АРКАДНОЕ УПРАВЛЕНИЕ В ВОЗДУХЕ --- VVVV
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Мы плавно меняем текущую скорость в сторону нового направления
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 targetVelocity = moveDir * currentMoveSpeed;

            // Lerp обеспечивает плавное управление в воздухе без резких остановок
            playerVelocity.x = Mathf.Lerp(playerVelocity.x, targetVelocity.x, airControlRate * Time.deltaTime);
            playerVelocity.z = Mathf.Lerp(playerVelocity.z, targetVelocity.z, airControlRate * Time.deltaTime);
        }
        // ^^^^ --- КОНЕЦ ИЗМЕНЕНИЯ --- ^^^^
    }

    private void HandleGrinding()
    {
        if (currentGrindRail == null) { EndGrind(false); return; }

        const float lookAheadDistance = 0.5f;
        Vector3 lookAheadPoint = transform.position + grindDirection * lookAheadDistance;
        Collider currentRailCollider = currentGrindRail.GetComponent<Collider>();
        Vector3 closestPointOnRail = currentRailCollider.ClosestPoint(lookAheadPoint);

        if (Vector3.Distance(lookAheadPoint, closestPointOnRail) > lookAheadDistance)
        {
            Transform nextRail = FindBestRail(currentGrindRail);
            if (nextRail != null)
            {
                currentGrindRail = nextRail;
                float dot = Vector3.Dot(grindDirection, currentGrindRail.forward);
                grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
            }
            else
            {
                EndGrind(false);
                return;
            }
        }
        else
        {
            Transform bestRail = FindBestRail();
            if (bestRail != null && bestRail != currentGrindRail)
            {
                currentGrindRail = bestRail;
                float dot = Vector3.Dot(grindDirection, currentGrindRail.forward);
                grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
            }
        }

        Vector3 snapToPoint = currentGrindRail.GetComponent<Collider>().ClosestPoint(transform.position);
        controller.Move(snapToPoint - transform.position); // Прилипание отдельным Move

        // VVVV --- ИЗМЕНЕНИЕ --- VVVV
        // Задаем скорость для основного цикла движения
        playerVelocity = grindDirection * currentMoveSpeed;
        // ^^^^ --- КОНЕЦ ИЗМЕНЕНИЯ --- ^^^^

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(grindDirection), turnSmoothTime * 15f);
        animator.SetFloat(animIDSpeed, 1f);

        if (Input.GetButtonDown("Jump"))
        {
            EndGrind(true);
        }
    }


    // --- МЕТОДЫ ДЛЯ ГРАЙНДА И СТЕН ---

    // VVVV --- НОВЫЙ МЕТОД --- VVVV
    private void HandleWallSliding()
    {
        isWallSliding = false;
        // Скольжение возможно только в воздухе и когда мы не на земле и не на рельсе
        if (currentState == PlayerState.InAir && !controller.isGrounded)
        {
            // Выпускаем луч прямо вперед по ходу движения персонажа
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, wallJumpableLayer))
            {
                // Если мы врезались в стену
                isWallSliding = true;
                wallNormal = hit.normal; // Сохраняем нормаль стены (направление "от стены")
            }
        }

        // Если мы скользим, замедляем падение
        if (isWallSliding)
        {
            if (playerVelocity.y < -wallSlideSpeed)
            {
                playerVelocity.y = -wallSlideSpeed;
            }
        }
    }
    // ^^^^ --- КОНЕЦ НОВОГО МЕТОДА --- ^^^^

    // VVVV --- НОВЫЙ МЕТОД --- VVVV
    private void DoWallJump()
    {
        isWallSliding = false;

        // Вертикальная составляющая прыжка
        // Используем твою формулу и новую переменную высоты
        playerVelocity.y = Mathf.Sqrt(wallJumpHeight * -2f * gravity);

        // Горизонтальная составляющая прыжка
        // Толкаем персонажа в направлении, обратном нормали стены
        Vector3 jumpDirection = wallNormal * wallJumpSidewaysForce;
        playerVelocity.x = jumpDirection.x;
        playerVelocity.z = jumpDirection.z;

        // Поворачиваем персонажа лицом от стены для лучшего визуального эффекта
        transform.rotation = Quaternion.LookRotation(wallNormal);

        // Запускаем анимацию прыжка
        animator.SetTrigger(animIDJump);
    }
    // ^^^^ --- КОНЕЦ НОВОГО МЕТОДА --- ^^^^

    private Transform FindBestRail(Transform railToIgnore = null)
    {
        var nearbyRails = Physics.OverlapSphere(transform.position, grindSearchRadius, grindableLayer);
        var bestRail = nearbyRails
            .Where(rail => rail.transform != railToIgnore)
            .OrderBy(rail => Vector3.Distance(transform.position, rail.ClosestPoint(transform.position)))
            .FirstOrDefault();
        return bestRail?.transform;
    }

    private void CheckForGrindStart()
    {
        if (currentState == PlayerState.Grinding || grindCooldownTimer > 0f) return;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 1.5f, grindableLayer))
        {
            if (!controller.isGrounded || controller.velocity.magnitude > 0.1f)
            {
                StartGrind(hit);
            }
        }
    }

    private void StartGrind(RaycastHit hit)
    {
        SetState(PlayerState.Grinding);
        playerVelocity.y = 0;
        currentGrindRail = hit.transform;
        float dot = Vector3.Dot(transform.forward, currentGrindRail.forward);
        grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
    }

    private void EndGrind(bool didJump)
    {
        Transform oldRail = currentGrindRail;
        currentGrindRail = null;
        SetState(PlayerState.InAir);
        grindCooldownTimer = GRIND_COOLDOWN;

        if (oldRail == null) return;

        // VVVV --- ИЗМЕНЕНИЕ --- VVVV
        // Эта логика теперь работает правильно благодаря единому вектору playerVelocity
        playerVelocity = grindDirection * currentMoveSpeed;
        // ^^^^ --- КОНЕЦ ИЗМЕНЕНИЯ --- ^^^^

        if (didJump)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger(animIDJump);
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---
    private void ApplyGravity()
    {
        // Просто применяем гравитацию к Y-составляющей скорости
        playerVelocity.y += gravity * Time.deltaTime;
    }

    private void SetState(PlayerState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
        }
    }
}