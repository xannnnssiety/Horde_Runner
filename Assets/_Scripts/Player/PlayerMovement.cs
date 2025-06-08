using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    private CharacterController controller;
    private Animator animator;
    public Transform cameraTransform;

    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;
    public float turnSmoothTime = 0.1f;
    public float speedIncreaseRate = 1f; // Скорость увеличения И уменьшения скорости в секунду
    public float maxMoveSpeed = 10f;

    public float currentMoveSpeed;
    private float turnSmoothVelocity;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float gravity = -19.62f;

    private Vector3 velocity;
    private bool isGrounded;

    private readonly int animIDSpeed = Animator.StringToHash("Speed");
    private readonly int animIDJump = Animator.StringToHash("Jump");

    // Убрали SpeedThreshold, так как теперь анимация зависит только от ввода
    // private const float SpeedThreshold = 0.01f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else if (cameraTransform == null)
        {
            Debug.LogError("PlayerMovement: Transform главной камеры не найден и не назначен в cameraTransform.");
        }

        currentMoveSpeed = baseMoveSpeed;

        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        HandleGroundCheck();
        HandlePlayerActions();
        ApplyGravityAndFinalMove();

        if (Input.GetKeyDown(KeyCode.U))
        {
            // Добавляем +10% к радиусу всех скиллов
            PlayerStatsManager.Instance.AddAreaBonus(0.1f);
        }

    }

    void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandlePlayerActions()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDir.magnitude >= 0.1f) // Если есть ввод для движения
        {
            // Увеличиваем текущую скорость плавно
            if (currentMoveSpeed < maxMoveSpeed)
            {
                currentMoveSpeed += speedIncreaseRate * Time.deltaTime;
                currentMoveSpeed = Mathf.Min(currentMoveSpeed, maxMoveSpeed);
            }

            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * currentMoveSpeed * Time.deltaTime);

            // Анимация бега включается только при активном вводе
            animator.SetFloat(animIDSpeed, 1f);
        }
        else // Если нет ввода для движения
        {
            // Плавно уменьшаем скорость до базовой
            if (currentMoveSpeed > baseMoveSpeed)
            {
                currentMoveSpeed -= speedIncreaseRate * Time.deltaTime;
                currentMoveSpeed = Mathf.Max(currentMoveSpeed, baseMoveSpeed); // Не опускаться ниже базовой
            }
            // Важно: controller.Move здесь не вызывается,
            // поэтому персонаж прекращает движение при отпускании кнопки.

            // Анимация покоя включается, как только ввод прекратился
            animator.SetFloat(animIDSpeed, 0f);
        }

        // Прыжок обрабатывается независимо от основной логики движения/остановки
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            animator.SetTrigger(animIDJump);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ApplyGravityAndFinalMove()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    public void AddSpeedBoost(float amount, float duration)
    {
        currentMoveSpeed += amount;
        currentMoveSpeed = Mathf.Min(currentMoveSpeed, maxMoveSpeed);
        Debug.Log($"Speed boosted by {amount}. New current speed: {currentMoveSpeed}");
    }
}