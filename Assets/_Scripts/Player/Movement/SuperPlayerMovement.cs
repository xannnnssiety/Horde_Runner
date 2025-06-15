using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class SuperPlayerMovement : MonoBehaviour
{
    public enum PlayerState
    {
        Grounded,
        InAir,
        Grinding
    }

    [Header("Ссылки на компоненты")]
    public Camera mainCamera;

    [Header("Настройки движения")]
    public float maxMoveSpeed = 20f;
    public float turnSmoothTime = 0.08f;

    [Tooltip("Как быстро персонаж набирает скорость")]
    public float acceleration = 10f;

    [Tooltip("Как быстро персонаж останавливается")]
    public float deceleration = 12f;

    [Header("Настройки прыжка и гравитации")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float coyoteTime = 0.15f;

    [Header("Настройки грайнда (рельсы)")]
    public float grindSpeed = 10f;
    public LayerMask grindableLayer;
    public float grindDetectionDistance = 1.5f;
    // <<-- НОВЫЙ ПАРАМЕТР ИЗ РАБОЧЕЙ ВЕРСИИ -->>
    [Tooltip("Радиус, в котором персонаж ищет рельсы вокруг себя")]
    public float grindSearchRadius = 3f;

    [Header("Состояние (для отладки)")]
    [SerializeField] private PlayerState currentState;

    // Приватные переменные
    private CharacterController controller;
    private Animator animator;
    private Vector3 playerVelocity;
    private float turnSmoothVelocity;
    private float grindCooldownTimer;
    private const float GRIND_COOLDOWN = 0.2f;
    private Transform currentGrindRail;
    private Vector3 grindDirection;
    private float coyoteTimeCounter;
    private float currentSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (mainCamera == null) Debug.LogError("Камера не назначена!");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentState = controller.isGrounded ? PlayerState.Grounded : PlayerState.InAir;
    }

    void Update()
    {
        if (grindCooldownTimer > 0)
        {
            grindCooldownTimer -= Time.deltaTime;
        }

        if (controller.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

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

        if (currentState != PlayerState.Grinding)
        {
            ApplyGravity();
        }
    }

    // --- ОБЫЧНОЕ ДВИЖЕНИЕ (БЕЗ ИЗМЕНЕНИЙ) ---
    private void HandleGroundedMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Плавно увеличиваем скорость до максимальной
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxMoveSpeed, acceleration * Time.deltaTime);

            // Поворот персонажа
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else
        {
            // Если нет ввода, плавно снижаем скорость до нуля
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }

        controller.Move(transform.forward * currentSpeed * Time.deltaTime);

        // Передаем в аниматор не 0 или 1, а нормализованную скорость (от 0 до 1)
        animator.SetFloat("Speed", currentSpeed / maxMoveSpeed);

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
            SetState(PlayerState.InAir);
            coyoteTimeCounter = 0f;
        }

        if (coyoteTimeCounter <= 0f && !controller.isGrounded)
        {
            SetState(PlayerState.InAir);
        }
    }

    private void HandleAirborneMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // В воздухе тоже можно немного влиять на движение, используя текущую скорость
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Движение в воздухе использует currentSpeed, но с меньшим влиянием (например, 20%)
            Vector3 airMove = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * (currentSpeed * 0.2f);
            controller.Move(airMove * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            SetState(PlayerState.Grounded);
        }
    }

    // <<-- НОВАЯ, НАДЕЖНАЯ ЛОГИКА ГРАЙНДА ИЗ ПОСЛЕДНЕЙ ВЕРСИИ -->>
    private void HandleGrinding()
    {
        Transform bestRail = FindBestRail();

        if (bestRail == null)
        {
            // <<-- ИСПРАВЛЕНИЕ 2: Если рельс нет, заканчиваем грайнд с сохранением инерции
            EndGrind(false);
            return;
        }

        if (currentGrindRail != bestRail)
        {
            currentGrindRail = bestRail;
            float dot = Vector3.Dot(grindDirection, currentGrindRail.forward);
            grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
        }

        Vector3 closestPoint = currentGrindRail.GetComponent<Collider>().ClosestPoint(transform.position);
        controller.Move((closestPoint - transform.position));

        // <<-- ИЗМЕНЕНИЕ: Применяем скорость грайнда и обновляем currentSpeed
        controller.Move(grindDirection * grindSpeed * Time.deltaTime);
        currentSpeed = grindSpeed; // Сохраняем скорость грайнда

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(grindDirection), turnSmoothTime * 15f);
        animator.SetFloat("Speed", 1f);

        if (Input.GetButtonDown("Jump"))
        {
            EndGrind(true);
        }
    }

    // <<-- НОВЫЙ, НАДЕЖНЫЙ МЕТОД ПОИСКА ИЗ ПОСЛЕДНЕЙ ВЕРСИИ -->>
    private Transform FindBestRail()
    {
        var nearbyRails = Physics.OverlapSphere(transform.position, grindSearchRadius, grindableLayer);
        if (nearbyRails.Length == 0) return null;

        return nearbyRails.OrderBy(rail => Vector3.Distance(transform.position, rail.ClosestPoint(transform.position)))
                          .FirstOrDefault()?
                          .transform;
    }

    // --- СТАРЫЕ МЕТОДЫ ГРАЙНДА ОБНОВЛЕНЫ ДЛЯ РАБОТЫ С НОВОЙ СИСТЕМОЙ ---
    private void CheckForGrindStart()
    {
        // Не можем начать, если кулдаун или мы уже скользим
        if (currentState == PlayerState.Grinding || grindCooldownTimer > 0f) return;

        // Проверяем, есть ли под нами рельса
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, grindDetectionDistance, grindableLayer))
        {
            // Если мы не на земле (падаем) ИЛИ на земле, но движемся - начинаем грайнд
            if (!controller.isGrounded || controller.velocity.magnitude > 0.1f)
            {
                StartGrind(hit);
            }
        }
    }

    private void StartGrind(RaycastHit hit)
    {
        SetState(PlayerState.Grinding);
        playerVelocity = Vector3.zero; // Отключаем гравитацию
        currentGrindRail = hit.transform;

        // Определяем начальное направление
        float dot = Vector3.Dot(transform.forward, currentGrindRail.forward);
        grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
    }

    private void EndGrind(bool withJump)
    {
        SetState(PlayerState.InAir);
        currentGrindRail = null;
        grindCooldownTimer = GRIND_COOLDOWN;

        if (withJump)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }
        // <<-- ИЗМЕНЕНИЕ: Инерция сохраняется!
        // После грайнда персонаж продолжит движение вперед с высокой скоростью,
        // а не будет привязан к вводу игрока.
        // Мы задаем ему импульс в том направлении, в котором он летел с рельсы.
        Vector3 inertiaDirection = grindDirection;
        // Мы применяем этот импульс как горизонтальную часть playerVelocity
        playerVelocity.x = inertiaDirection.x * currentSpeed;
        playerVelocity.z = inertiaDirection.z * currentSpeed;

        // ВАЖНО: Мы больше не меняем currentSpeed напрямую.
        // В HandleGroundedMovement он сам плавно снизится до maxMoveSpeed или до 0.
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (БЕЗ ИЗМЕНЕНИЙ) ---
    private void SetState(PlayerState newState) { if (currentState != newState) currentState = newState; }

    private void ApplyGravity()
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}