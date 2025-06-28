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
    private static readonly int LandFlairTrigger = Animator.StringToHash("LandFlair"); 
    private static readonly int FlairIndex = Animator.StringToHash("FlairIndex");
    private static readonly int JumpFlairTrigger = Animator.StringToHash("JumpFlair");
    private static readonly int JumpFlairIndex = Animator.StringToHash("JumpFlairIndex");

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

        if (playerSlide != null) playerSlide.OnSlideEnd += OnSlideFinished;
        else Debug.LogWarning("Не удалось подписаться на окончание слайда: PlayerSlide не найден.");

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
        if (playerSlide != null) playerSlide.OnSlideEnd -= OnSlideFinished;
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


    private void OnSlideFinished()
    {
        // Когда слайд заканчивается, мы тоже вызываем логику приземления
        Debug.Log("<color=yellow>SLIDE ENDED, triggering landing logic.</color>");
        
        PlayLandingAnimation();
    }

    private void UpdateAllParameters()
    {
        // --- ОБНОВЛЕНИЕ FLOAT ПАРАМЕТРОВ ---
        // Вычисляем горизонтальную скорость для анимаций бега/ходьбы
        var horizontalVelocity = new Vector3(playerController.PlayerVelocity.x, 0, playerController.PlayerVelocity.z);
        animator.SetFloat(Speed, horizontalVelocity.magnitude, 0.1f, Time.deltaTime);

        // Вычисляем вертикальную скорость для анимаций прыжка/падения,
        // считая, что на земле она равна нулю для аниматора.
        float verticalSpeedForAnimator = playerController.IsGrounded ? 0f : playerController.PlayerVelocity.y;
        animator.SetFloat(VerticalSpeed, verticalSpeedForAnimator, 0.1f, Time.deltaTime);

        // --- ОБНОВЛЕНИЕ BOOL ПАРАМЕТРОВ С ПРИОРИТЕТАМИ ---

        // 1. Сначала получаем "сырые" данные о текущих активных состояниях из модулей.
        bool isGrinding = playerController.CurrentState == PlayerController.PlayerState.Grinding;
        bool isWallRunning = playerWallRun != null && playerWallRun.IsWallRunning;
        bool isWallSliding = playerController.IsWallSliding;
        bool isSliding = playerSlide != null && playerSlide.IsSliding;
        bool isDashing = playerDash != null && playerDash.IsDashing;

        // 2. Устанавливаем флаги для всех "особых" состояний в аниматор.
        animator.SetBool(IsGrinding, isGrinding);
        animator.SetBool(IsWallRunning, isWallRunning);
        animator.SetBool(IsWallSliding, isWallSliding);
        animator.SetBool(IsSliding, isSliding);
        animator.SetBool(IsDashing, isDashing);

        // 3. Теперь вычисляем и устанавливаем флаг IsGrounded с учетом приоритетов.
        // Персонаж считается "на земле" для аниматора, только если он физически на земле
        // И при этом НЕ выполняет грайнд или подкат (которые тоже могут касаться земли).
        bool isGroundedForAnimator = playerController.IsGrounded && !isGrinding && !isSliding;

 

        animator.SetBool(IsGrounded, isGroundedForAnimator);

        // 4. Обрабатываем специфичную логику для бега по стене (определение стороны).
        if (isWallRunning)
        {
            // Скалярное произведение определяет, с какой стороны от персонажа находится нормаль стены.
            float dot = Vector3.Dot(transform.right, playerWallRun.WallNormal);
            // Нормаль "смотрит" из стены. Если она справа от нас, dot будет положительным.
            // Если мы хотим, чтобы IsWallOnRight было true, когда стена справа, условие должно быть `dot > 0`.
            // Если анимация бега по правой стене "зеркальная" (персонаж наклоняется влево), то может понадобиться `dot < 0`.
            // Оставим `dot < 0` как в прошлый раз.
            animator.SetBool(IsWallOnRight, dot < 0);
        }

        // 5. Вызываем обработчик для триггеров, не связанных с событиями (например, приземление).
        HandleLanding();

        
    }



    private void HandleLanding()
    {
        bool isGroundedNow = playerController.IsGrounded;
        if (!wasGroundedLastFrame && isGroundedNow)
        {
            // Теперь этот метод просто вызывает нашу общую логику
            PlayLandingAnimation();
        }
        wasGroundedLastFrame = isGroundedNow;
    }



    private void PlayLandingAnimation()
    {
        // Проверяем, что мы не в подкате (на всякий случай, чтобы избежать зацикливания)
        if (playerSlide != null && playerSlide.IsSliding) return;

        // Проверяем, что мы на земле, а не, например, спрыгнули с рельсы
        if (playerController.CurrentState != PlayerController.PlayerState.Grounded) return;

        // Решаем, какую анимацию приземления играть
        float randomChance = UnityEngine.Random.Range(0f, 1f);
        if (randomChance <= 0.5f)
        {
            Debug.Log("<color=magenta>FLAIR ANIMATION TRIGGERED!</color>");
            int randomIndex = UnityEngine.Random.Range(0, 5); // Диапазон анимаций: max (НЕ включительно).
            animator.SetFloat(FlairIndex, randomIndex);
            animator.SetTrigger(LandFlairTrigger);
            // Мы больше не блокируем управление, так как это чисто визуальный эффект
        }
        else
        {
            Debug.Log("<color=green>NORMAL LANDING TRIGGERED!</color>");
            animator.SetTrigger(LandTrigger);
        }
    }

    // --- МЕТОДЫ-СЛУШАТЕЛИ СОБЫТИЙ ---
    private void TriggerJump()
    {
        if (animator == null) return; // Проверка на всякий случай

        // --- НОВАЯ ЛОГИКА С ШАНСОМ ---
        // Генерируем случайное число от 0.0 до 1.0
        float randomChance = UnityEngine.Random.Range(0f, 1f);

        if (randomChance <= 0.75f) // 75% шанс на "стильный" прыжок
        {
            Debug.Log("<color=cyan>FLAIR JUMP TRIGGERED!</color>");

            // Выбираем случайную анимацию из двух
            int randomIndex = UnityEngine.Random.Range(0, 3); // вернет 0 или 1

            // Устанавливаем индекс для Blend Tree
            animator.SetFloat(JumpFlairIndex, randomIndex);

            // Взводим триггер для стильного прыжка
            animator.SetTrigger(JumpFlairTrigger);
        }
        else // 25% шанс на обычный прыжок
        {
            Debug.Log("<color=blue>NORMAL JUMP TRIGGERED!</color>");

            // Взводим триггер для обычного прыжка
            animator.SetTrigger(JumpTrigger);
        }
    }
}