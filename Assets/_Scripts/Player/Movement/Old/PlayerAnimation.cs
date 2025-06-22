using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimation : MonoBehaviour
{
    // --- ID ПАРАМЕТРОВ АНИМАТОРА ---
    // Использование StringToHash намного производительнее, чем передача строк каждый кадр
    private readonly int animIDSpeed = Animator.StringToHash("Speed");
    private readonly int animIDGrounded = Animator.StringToHash("Grounded");
    private readonly int animIDJump = Animator.StringToHash("Jump");
    private readonly int animIDFreeFall = Animator.StringToHash("FreeFall");
    private readonly int animIDWallSliding = Animator.StringToHash("WallSliding"); // Предположим, у вас есть такой параметр
    private readonly int animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

    // Ссылки
    private PlayerController _controller;
    private Animator _animator;

    // Переменные для отслеживания состояния
    private bool hasJumpedThisFrame = false;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
    }

    // Вызывается из Update() главного контроллера в самом конце кадра
    public void TickUpdate()
    {
        UpdateGroundedAndFallingState();
        UpdateSpeed();
        HandleJumpAnimation();
    }

    private void UpdateGroundedAndFallingState()
    {
        // Устанавливаем, на земле ли персонаж. Полезно для переходов в Idle/Locomotion.
        _animator.SetBool(animIDGrounded, _controller.IsGrounded);

        // Устанавливаем, скользит ли персонаж по стене.
        _animator.SetBool(animIDWallSliding, _controller.IsWallSliding);

        // Если мы в воздухе и не скользим по стене - мы в свободном падении.
        bool isFalling = _controller.CurrentState == PlayerController.PlayerState.InAir && !_controller.IsWallSliding;
        _animator.SetBool(animIDFreeFall, isFalling);
    }

    private void UpdateSpeed()
    {
        // Вычисляем горизонтальную скорость
        float horizontalSpeed = new Vector3(_controller.PlayerVelocity.x, 0.0f, _controller.PlayerVelocity.z).magnitude;

        // Получаем направление ввода, чтобы анимация ходьбы/бега была 1, а не 0.5
        float inputMagnitude = _controller.InputDirection.magnitude;

        // Передаем скорость и величину ввода в аниматор.
        // animIDSpeed используется для скорости анимации (1 = бег, 0 = стойка).
        // animIDMotionSpeed используется для управления скоростью самой анимации, чтобы избежать "лунной походки".
        _animator.SetFloat(animIDSpeed, horizontalSpeed);
        _animator.SetFloat(animIDMotionSpeed, inputMagnitude);
    }

    private void HandleJumpAnimation()
    {
        // Этот метод немного сложнее, так как прыжок - это однократное событие (триггер).
        // Нам нужно "поймать" момент, когда прыжок произошел.

        // Простой способ: если мы были на земле, а в следующем кадре оказались в воздухе, значит был прыжок.
        if (!_controller.IsGrounded && _controller.CharacterController.velocity.y > 0 && !hasJumpedThisFrame)
        {
            if (_controller.CurrentState == PlayerController.PlayerState.InAir)
            {
                _animator.SetTrigger(animIDJump);
                hasJumpedThisFrame = true; // Устанавливаем флаг, чтобы не триггерить анимацию каждый кадр полета вверх
            }
        }

        // Сбрасываем флаг, как только коснулись земли
        if (_controller.IsGrounded)
        {
            hasJumpedThisFrame = false;
        }
    }
}