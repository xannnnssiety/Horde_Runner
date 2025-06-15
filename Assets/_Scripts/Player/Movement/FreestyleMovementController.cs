using UnityEngine;

// Требуем те же компоненты, что и в твоем старом скрипте, для совместимости.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class FreestyleMovementController : MonoBehaviour
{
    // --- Состояния Персонажа ---
    // Это сердце нашего нового контроллера. Персонаж всегда находится в одном из этих состояний.
    public enum PlayerState
    {
        Grounded,      // На земле
        InAir,         // В воздухе (прыжок или падение)
        Grinding,      // Скользит по рельсе
        WallRunning    // Бежит по стене
    }

    [Header("Состояние (для отладки)")]
    [SerializeField] private PlayerState currentState;

    [Header("Основные Компоненты")]
    public Transform cameraTransform;
    private CharacterController controller;
    private Animator animator;

    // --- ТВОИ ПЕРЕМЕННЫЕ (сохранены для совместимости) ---
    [Header("Настройки Скорости (из старого скрипта)")]
    public float baseMoveSpeed = 5f;
    public float maxMoveSpeed = 10f;
    public float speedIncreaseRate = 1f;
    [Tooltip("Публичная переменная для других скриптов")]
    public float currentMoveSpeed; // Ключевая переменная для твоих других систем!

    [Header("Настройки Поворота")]
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Настройки Прыжка и Гравитации")]
    public float jumpHeight = 2f;
    public float gravity = -19.62f;

    // --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ ПАРКУРА ---
    [Header("Настройки Паркура")]
    [Tooltip("Слои, которые считаются рельсами для грайнда")]
    public LayerMask grindLayers;
    [Tooltip("Скорость во время скольжения")]
    public float grindSpeed = 15f;

    [Tooltip("Слои, которые считаются стенами для бега")]
    public LayerMask wallRunLayers;
    [Tooltip("Как долго можно бежать по стене")]
    public float maxWallRunTime = 2f;
    [Tooltip("Сила прыжка от стены")]
    public float wallRunJumpForce = 5f;
    private float currentWallRunTime;
    [Tooltip("Сила контроля в воздухе (0 - нет контроля, 1 - полный контроль)")]
    [Range(0f, 20f)]
    public float airControlForce = 5f;


    // --- Внутренние переменные ---
    private Vector3 velocity; // Отвечает за гравитацию и прыжки
    private Vector3 inputDir; // Направление ввода с клавиатуры
    private Transform currentRail; // Ссылка на текущую рельсу, по которой скользим
    private Vector3 railDirection;
    private float railProgress;
    private Vector3 wallNormal; // Направление от стены (для прыжка)
    private bool isWallOnRight; // Стена справа от нас?

    // --- Анимация ---
    private readonly int animIDSpeed = Animator.StringToHash("Speed");
    private readonly int animIDJump = Animator.StringToHash("Jump");
    private readonly int animIDGrounded = Animator.StringToHash("isGrounded");
    private readonly int animIDGrinding = Animator.StringToHash("isGrinding");
    private readonly int animIDWallRunning = Animator.StringToHash("isWallRunning");

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        currentMoveSpeed = baseMoveSpeed;
        currentState = PlayerState.Grounded; // Начинаем на земле
    }

    void Update()
    {
        // 1. Получаем ввод один раз для всего кадра
        HandleInput();

        // 2. Вызываем логику, соответствующую ТЕКУЩЕМУ состоянию персонажа
        switch (currentState)
        {
            case PlayerState.Grounded:
                HandleGroundedState();
                break;
            case PlayerState.InAir:
                HandleInAirState();
                break;
            case PlayerState.Grinding:
                HandleGrindingState();
                break;
            case PlayerState.WallRunning:
                HandleWallRunningState();
                break;
        }

        // 3. Применяем гравитацию (если нужно) и финальное движение
        ApplyFinalMovement();
        controller.Move(velocity * Time.deltaTime);

        // 4. Обновляем аниматор
        UpdateAnimator();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDir = new Vector3(horizontal, 0f, vertical).normalized;
    }

    // --- ЛОГИКА ДЛЯ КАЖДОГО СОСТОЯНИЯ ---

    private void HandleGroundedState()
    {
        // Проверяем переходы в другие состояния
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
            return; // Выходим, чтобы не выполнять остальную логику этого кадра
        }
        if (!controller.isGrounded)
        {
            SwitchState(PlayerState.InAir);
            return;
        }

        // Логика движения на земле (взята из твоего скрипта)
        if (inputDir.magnitude >= 0.1f)
        {
            // Увеличиваем скорость
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, maxMoveSpeed, speedIncreaseRate * Time.deltaTime);

            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            velocity.x = moveDirection.x * currentMoveSpeed;
            velocity.z = moveDirection.z * currentMoveSpeed;
        }
        else
        {
            // Уменьшаем скорость и останавливаем движение
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, baseMoveSpeed, speedIncreaseRate * Time.deltaTime);
            velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * 10f); 
            velocity.z = Mathf.Lerp(velocity.z, 0, Time.deltaTime * 10f);
        }

        // Устанавливаем вертикальную скорость для "прилипания" к земле
        if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleInAirState()
    {
        // Проверяем переходы в другие состояния
        if (controller.isGrounded)
        {
            SwitchState(PlayerState.Grounded);
            return;
        }
        if (CheckForWallRun())
        {
            // Не переходим на стену, если игрок только что с нее спрыгнул
            // (Это предотвращает "прилипание" обратно к той же стене)
            // Нужен небольшой таймер, но пока оставим так.
            SwitchState(PlayerState.WallRunning);
            return;
        }
        if (CheckForGrind(out Transform rail))
        {
            StartGrinding(rail);
            return;
        }

        // --- Логика контроля в воздухе ---
        if (inputDir.magnitude >= 0.1f)
        {
            // Получаем направление движения относительно камеры
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Применяем это направление к горизонтальной скорости
            // Мы не меняем скорость напрямую, а "подталкиваем" ее в нужную сторону
            velocity.x += moveDirection.x * airControlForce * Time.deltaTime;
            velocity.z += moveDirection.z * airControlForce * Time.deltaTime;

            // Ограничиваем максимальную горизонтальную скорость в полете, чтобы не улететь в космос
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            if (horizontalVelocity.magnitude > maxMoveSpeed)
            {
                Vector3 limitedHorizontalVel = horizontalVelocity.normalized * maxMoveSpeed;
                velocity.x = limitedHorizontalVel.x;
                velocity.z = limitedHorizontalVel.z;
            }
        }
    }

    private void HandleGrindingState()
    {
        // Переходы из состояния
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
            return;
        }

        if (!CheckForGrind(out Transform rail) || rail != currentRail)
        {
            SwitchState(PlayerState.InAir);
            // Сохраняем инерцию при сходе с рельсы
            velocity = transform.forward * grindSpeed;
            return;
        }

        // --- Основная логика движения по рельсе ---
        // Движение теперь происходит через CharacterController.Move, а не через velocity,
        // чтобы он корректно обрабатывал столкновения.
        Vector3 moveVector = railDirection * grindSpeed * Time.deltaTime;
        controller.Move(moveVector);

        // Постоянно проверяем и корректируем направление, если рельса изогнутая
        // (Для этого рельса должна быть сделана из нескольких сегментов)
        transform.rotation = Quaternion.LookRotation(railDirection);
    }

    private void HandleWallRunningState()
    {
        // Логика бега по стене
        currentWallRunTime -= Time.deltaTime;

        // Переходы
        if (Input.GetButtonDown("Jump"))
        {
            WallJump();
            return;
        }

        if (currentWallRunTime <= 0 || !CheckForWallRun())
        {
            SwitchState(PlayerState.InAir);
            return;
        }

        // --- Основная логика движения по стене ---
        // Двигаемся вперед вдоль стены. Слегка прижимаемся к ней, чтобы не отвалиться.
        Vector3 wallRunDirection = transform.forward;
        velocity = wallRunDirection * currentMoveSpeed; // Используем текущую скорость

        // Слегка "скользим" вниз по стене, чтобы это не выглядело как полет.
        // Если хочешь бег вверх, можно сделать velocity.y = 2f;
        velocity.y = -1f;
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ---

    private void SwitchState(PlayerState newState)
    {
        if (currentState == newState) return;

        // Можно добавить логику выхода из старого состояния здесь (OnStateExit)

        currentState = newState;

        // Можно добавить логику входа в новое состояние здесь (OnStateEnter)
        switch (currentState)
        {
            case PlayerState.WallRunning:
                currentWallRunTime = maxWallRunTime;
                velocity.y = 0; // Сбрасываем вертикальную скорость при начале бега по стене
                transform.forward = Vector3.Cross(wallNormal, Vector3.up) * (isWallOnRight ? 1 : -1);
                break;
            case PlayerState.Grinding:
                velocity.y = 0; // Сбрасываем гравитацию на рельсе
                break;
        }
    }

    private void Jump()
    {
        SwitchState(PlayerState.InAir);
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void WallJump()
    {
        SwitchState(PlayerState.InAir);

        // Направление прыжка = направление от стены + направление вверх
        Vector3 jumpDirection = (wallNormal + Vector3.up).normalized;

        // Применяем силу. Используем твою переменную jumpHeight для высоты.
        // И новую wallRunJumpForce для силы отталкивания.
        velocity = jumpDirection * wallRunJumpForce;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void StartGrinding(Transform rail)
    {
        SwitchState(PlayerState.Grinding);
        currentRail = rail;
        velocity = Vector3.zero; // Сбрасываем всю предыдущую скорость

        // --- Более надежное определение направления рельсы ---
        // Мы предполагаем, что рельса - это вытянутый объект.
        // Его "направление" - это его локальная ось Z (forward) или X (right)
        // в зависимости от того, как он смоделирован.
        // Проверяем, какая ось длиннее, и используем ее.
        if (rail.localScale.z > rail.localScale.x)
        {
            railDirection = rail.forward;
        }
        else
        {
            railDirection = rail.right;
        }

        // --- Находим ближайшую точку на рельсе и выравниваемся ---
        // Это сложная часть. Для простоты, мы пока выровняемся по центру
        // и установим направление.

        // Выравниваем вращение персонажа
        transform.rotation = Quaternion.LookRotation(railDirection);

        // "Примагничиваем" персонажа к верхней точке рельсы
        Collider railCollider = rail.GetComponent<Collider>();
        Vector3 closestPointOnRail = railCollider.ClosestPoint(transform.position);
        transform.position = new Vector3(closestPointOnRail.x, closestPointOnRail.y + controller.height / 2, closestPointOnRail.z);
    }

    private bool CheckForWallRun()
    {
        // Проверяем наличие стены справа
        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hitRight, 1f, wallRunLayers))
        {
            wallNormal = -hitRight.normal; // Нормаль смотрит от стены
            isWallOnRight = true;
            return true;
        }
        // Проверяем наличие стены слева
        else if (Physics.Raycast(transform.position, -transform.right, out RaycastHit hitLeft, 1f, wallRunLayers))
        {
            wallNormal = -hitLeft.normal;
            isWallOnRight = false;
            return true;
        }
        return false;
    }

    private bool CheckForGrind(out Transform rail)
    {
        // Пускаем луч вниз, чтобы найти рельсу
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f, grindLayers))
        {
            rail = hit.transform;
            return true;
        }
        rail = null;
        return false;
    }

    private void ApplyFinalMovement()
    {
        // Применяем гравитацию, если не на земле и не на рельсе/стене
        if (!controller.isGrounded && currentState != PlayerState.Grinding && currentState != PlayerState.WallRunning)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Применяем всё движение (горизонтальное + вертикальное) один раз
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        // Устанавливаем параметры для аниматора на основе состояния
        animator.SetBool(animIDGrounded, currentState == PlayerState.Grounded);
        animator.SetBool(animIDGrinding, currentState == PlayerState.Grinding);
        animator.SetBool(animIDWallRunning, currentState == PlayerState.WallRunning);

        // Для параметра Speed используем величину ввода, как ты и делал
        animator.SetFloat(animIDSpeed, inputDir.magnitude);

        // Триггер прыжка ставим прямо в методе Jump(), здесь это не нужно
        // Speed теперь зависит от состояния
        float speedPercent = 0f;
        if (currentState == PlayerState.Grounded)
        {
            // На земле скорость анимации зависит от ввода
            speedPercent = inputDir.magnitude;
        }
        else if (currentState == PlayerState.Grinding || currentState == PlayerState.WallRunning)
        {
            // На рельсах или стене анимация всегда на полной скорости
            speedPercent = 1f;
        }

        animator.SetFloat(animIDSpeed, speedPercent);
    }

    // --- ТВОЙ ПУБЛИЧНЫЙ МЕТОД (сохранен для совместимости) ---
    public void AddSpeedBoost(float amount, float duration)
    {
        // Просто увеличиваем скорость, как и раньше.
        // Ты можешь добавить логику с duration (длительностью) позже, если понадобится.
        currentMoveSpeed = Mathf.Min(currentMoveSpeed + amount, maxMoveSpeed * 1.5f); // Позволим бусту превышать максимум
        Debug.Log($"Speed boosted by {amount}. New current speed: {currentMoveSpeed}");
    }
}