using UnityEngine;


[RequireComponent(typeof(PlayerController))]
public class PlayerSlide : MonoBehaviour
{
    [Header("Настройки подката")]
    [Tooltip("Скорость во время подката на земле")]
    public float slideSpeed = 10f;
    [Tooltip("Как долго длится подкат в секундах")]
    public float slideDuration = 0.7f;
    [Tooltip("Время перезарядки подката в секундах")]
    public float slideCooldown = 1f;
    [Tooltip("Усиленная гравитация при активации в воздухе (чтобы быстрее упасть)")]
    public float diveGravity = -200f;

    // Публичное свойство, чтобы контроллер и другие модули знали о нашем состоянии
    public bool IsSliding { get; private set; }

    // Ссылки
    private PlayerController _controller;
    private CharacterController _characterController;

    // Внутренние переменные
    private float slideTimer;
    private float cooldownTimer;
    private bool isDiving; // Флаг, показывающий, что мы активировали подкат в воздухе и падаем

    // ID Анимации
    private readonly int animIDSlide = Animator.StringToHash("Slide");

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    public void TickUpdate()
    {
        // --- НОВАЯ СЕКЦИЯ ДЛЯ ОБНОВЛЕНИЯ ТАЙМЕРА ---
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Проверяем ввод, только если мы не в другом особом состоянии,
        // слайд не активен И кулдаун прошел
        if (!IsSliding && cooldownTimer <= 0 && _controller.CurrentState != PlayerController.PlayerState.Grinding)
        {
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                StartSlide();
            }
        }

        UpdateSlideState();
    }

    private void StartSlide()
    {
        slideTimer = slideDuration;

        if (_controller.IsGrounded)
        {
            // Если мы на земле, сразу начинаем скользить
            BeginGroundSlide();
        }
        else
        {
            // Если мы в воздухе, начинаем "пикирование"
            isDiving = true;
        }
    }

private void UpdateSlideState()
{
        if (isDiving && !_controller.IsGrounded)
        {
            // Ждем приземления
        }
        else if (isDiving && _controller.IsGrounded)
        {
            isDiving = false; // <-- Используем свойство
            BeginGroundSlide();
        }

        // Если мы в состоянии скольжения на земле
        if (IsSliding)
    {
        // --- НОВАЯ ПРОВЕРКА НА ПРЫЖОК ---
        if (Input.GetButtonDown("Jump"))
        {
            // Получаем высоту прыжка из контроллера
            float jumpHeight = _controller.jumpHeight;
            
            // Рассчитываем вертикальную скорость прыжка
            float jumpVelocityY = Mathf.Sqrt(jumpHeight * -2f * _controller.GravityValue);

            // Получаем текущую горизонтальную скорость слайда
            Vector3 currentHorizontalVelocity = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z);
            
            // Комбинируем горизонтальную скорость от слайда с вертикальной от прыжка
            _controller.PlayerVelocity = currentHorizontalVelocity + Vector3.up * jumpVelocityY;
            
            // Немедленно заканчиваем слайд
            EndSlide();
            
            // Сообщаем контроллеру, что мы теперь в воздухе
            _controller.SetState(PlayerController.PlayerState.InAir);
            
            // Запускаем анимацию прыжка
            _controller.Animator.SetTrigger("Jump"); // Предполагается, что у вас есть триггер Jump

            return; // Выходим из метода, чтобы не обрабатывать остальную логику слайда
        }

        slideTimer -= Time.deltaTime;
        
        if (slideTimer <= 0)
        {
            EndSlide();
        }
        else
        {
            Vector3 slideVelocity = transform.forward * slideSpeed;
            _controller.PlayerVelocity = new Vector3(slideVelocity.x, _controller.PlayerVelocity.y, slideVelocity.z);
        }
    }
}

    private void BeginGroundSlide()
    {
        IsSliding = true;
        _controller.Animator.SetBool(animIDSlide, true); // Используем Bool, а не Trigger

        // Возможно, потребуется изменить размер коллайдера, чтобы персонаж "пригнулся"
        // Например: _characterController.height = 0.8f;
    }

    private void EndSlide()
    {
        // Проверяем, действительно ли мы были в состоянии скольжения, чтобы не запустить кулдаун случайно
        if (IsSliding)
        {
            cooldownTimer = slideCooldown; 
        }

        IsSliding = false;
        _controller.Animator.SetBool("Slide", false); // Можно использовать animIDSlide

        // Возвращаем размер коллайдера, если меняли
        // Например: _characterController.height = 1.8f;
    }

    // Публичный метод, который будет вызывать контроллер, чтобы применить гравитацию
    public float GetCurrentGravity()
    {
        return isDiving ? diveGravity : _controller.GravityValue;
    }
}