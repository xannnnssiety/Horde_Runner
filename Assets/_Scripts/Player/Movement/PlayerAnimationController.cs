using UnityEngine;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimationController : MonoBehaviour
{
    // --- ССЫЛКИ НА КОМПОНЕНТЫ И МОДУЛИ ---
    // Мы сделаем их публичными, но скроем из инспектора, чтобы можно было легко отладить,
    // но не изменять вручную.
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public PlayerWallRun playerWallRun;
    [HideInInspector] public PlayerDash playerDash;
    [HideInInspector] public PlayerSlide playerSlide;
    [HideInInspector] public PlayerGroundedMovement playerGroundedMovement;
    [HideInInspector] public PlayerWallMovement playerWallMovement;
    // Добавьте другие модули, если они будут иметь события

    // --- ВНУТРЕННИЕ ПЕРЕМЕННЫЕ ---
    private bool wasGroundedLastFrame;

    // --- ХЭШИ ПАРАМЕТРОВ АНИМАТОРА ---
    // Это самый быстрый способ работы с параметрами.
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int IsWallRunning = Animator.StringToHash("IsWallRunning");
    private static readonly int IsWallSliding = Animator.StringToHash("IsWallSliding");
    private static readonly int IsSliding = Animator.StringToHash("IsSliding");
    private static readonly int IsGrinding = Animator.StringToHash("IsGrinding");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int IsWallOnRight = Animator.StringToHash("IsWallOnRight");
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int LandTrigger = Animator.StringToHash("Land");

    // --- МЕТОДЫ ЖИЗНЕННОГО ЦИКЛА UNITY ---

    void Awake()
    {
        // 1. ПОЛУЧАЕМ ВСЕ ССЫЛКИ И ПРОВЕРЯЕМ КАЖДУЮ ИЗ НИХ
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("FATAL ERROR: PlayerAnimationController не нашел PlayerController на объекте! Отключаюсь.", this);
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("FATAL ERROR: PlayerController не смог предоставить Animator! Проверьте PlayerController.Awake(). Отключаюсь.", this);
            enabled = false;
            return;
        }

        // Получаем модули, они понадобятся для подписки и для опроса в Update
        playerWallRun = GetComponent<PlayerWallRun>();
        playerDash = GetComponent<PlayerDash>();
        playerSlide = GetComponent<PlayerSlide>();
        playerGroundedMovement = GetComponent<PlayerGroundedMovement>();
        playerWallMovement = GetComponent<PlayerWallMovement>();
    }

    void OnEnable()
    {
        // 2. ПОДПИСЫВАЕМСЯ НА СОБЫТИЯ. Проверяем каждую ссылку перед подпиской.
        Debug.Log("PlayerAnimationController: Подписка на события...");

        if (playerGroundedMovement != null) playerGroundedMovement.OnJump += TriggerJump;
        else Debug.LogWarning("Не удалось подписаться на прыжок: PlayerGroundedMovement не найден.");

        if (playerWallMovement != null) playerWallMovement.OnJump += TriggerJump;
        else Debug.LogWarning("Не удалось подписаться на прыжок: PlayerWallMovement не найден.");

        if (playerWallRun != null) playerWallRun.OnJump += TriggerJump;
        else Debug.LogWarning("Не удалось подписаться на прыжок: PlayerWallRun не найден.");

        if (playerSlide != null) playerSlide.OnJump += TriggerJump;
        else Debug.LogWarning("Не удалось подписаться на прыжок: PlayerSlide не найден.");

        // Добавьте здесь подписки на OnDash, если реализуете.
    }

    void OnDisable()
    {
        // 3. ВСЕГДА ОТПИСЫВАЕМСЯ, чтобы избежать ошибок и утечек памяти.
        Debug.Log("PlayerAnimationController: Отписка от событий...");

        if (playerGroundedMovement != null) playerGroundedMovement.OnJump -= TriggerJump;
        if (playerWallMovement != null) playerWallMovement.OnJump -= TriggerJump;
        if (playerWallRun != null) playerWallRun.OnJump -= TriggerJump;
        if (playerSlide != null) playerSlide.OnJump -= TriggerJump;
    }

    void Start()
    {
        // 4. ИНИЦИАЛИЗИРУЕМ СОСТОЯНИЕ, чтобы избежать "залипания" в первом кадре.
        if (animator != null)
        {
            UpdateAllParameters();
            wasGroundedLastFrame = playerController.IsGrounded;
        }
    }

    // 5. ИСПОЛЬЗУЕМ LateUpdate, чтобы гарантированно получать финальные данные за кадр.
    void LateUpdate()
    {
        if (animator != null && playerController != null)
        {
            UpdateAllParameters();
        }
    }

    // --- ОСНОВНЫЕ МЕТОДЫ ---

    private void UpdateAllParameters()
    {
        // --- ОБНОВЛЕНИЕ FLOAT ПАРАМЕТРОВ ---
        var horizontalVelocity = new Vector3(playerController.PlayerVelocity.x, 0, playerController.PlayerVelocity.z);
        animator.SetFloat(Speed, horizontalVelocity.magnitude, 0.1f, Time.deltaTime); // Добавляем сглаживание

        float verticalSpeedForAnimator = playerController.IsGrounded ? 0f : playerController.PlayerVelocity.y;
        animator.SetFloat(VerticalSpeed, verticalSpeedForAnimator, 0.1f, Time.deltaTime); // И здесь

        // --- ОБНОВЛЕНИЕ BOOL ПАРАМЕТРОВ ---
        animator.SetBool(IsGrounded, playerController.IsGrounded);
        animator.SetBool(IsWallSliding, playerController.IsWallSliding);

        // Проверяем ссылки перед использованием, чтобы избежать NullReferenceException
        if (playerWallRun != null) animator.SetBool(IsWallRunning, playerWallRun.IsWallRunning);
        if (playerSlide != null) animator.SetBool(IsSliding, playerSlide.IsSliding);
        if (playerDash != null) animator.SetBool(IsDashing, playerDash.IsDashing);

        animator.SetBool(IsGrinding, playerController.CurrentState == PlayerController.PlayerState.Grinding);

        if (playerWallRun != null && playerWallRun.IsWallRunning)
        {
            float dot = Vector3.Dot(transform.right, playerWallRun.WallNormal);
            animator.SetBool(IsWallOnRight, dot < 0);
        }

        // --- ОБРАБОТКА ТРИГГЕРОВ, НЕ СВЯЗАННЫХ С СОБЫТИЯМИ ---
        HandleLanding();
    }

    private void HandleLanding()
    {
        bool isGroundedNow = playerController.IsGrounded;
        if (!wasGroundedLastFrame && isGroundedNow)
        {
            // Проверяем, что мы не находимся в подкате, который тоже начинается с приземления
            if (playerSlide == null || !playerSlide.IsSliding)
            {
                animator.SetTrigger(LandTrigger);
            }
        }
        wasGroundedLastFrame = isGroundedNow;
    }

    // --- МЕТОДЫ-СЛУШАТЕЛИ СОБЫТИЙ ---
    private void TriggerJump()
    {
        // Проверяем, что у нас есть аниматор, прежде чем вызывать триггер
        if (animator != null)
        {
            Debug.Log("<color=cyan>ANIMATION: Jump Triggered!</color>");
            animator.SetTrigger(JumpTrigger);
        }
    }
}