using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerGroundedMovement : MonoBehaviour
{
    // --- НАСТРОЙКИ МОДУЛЯ (видны в инспекторе) ---
    [Header("Настройки движения по земле")]

    [Tooltip("Насколько быстро персонаж останавливается на земле (трение)")]
    public float groundFriction = 10f;



    // Ссылка на главный контроллер для доступа к общим данным
    private PlayerController _controller;
    private CharacterController _characterController; // Кэшируем для удобства

    private void Awake()
    {
        // Получаем ссылки при старте
        _controller = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    // Этот метод вызывается из Update() главного контроллера,
    // когда CurrentState == PlayerState.Grounded
    public void TickUpdate()
    {
        HandleSpeed();
        HandleMovement();
        HandleJump();
    }

    private void HandleSpeed()
    {
        // Определяем, должен ли персонаж ускоряться
        bool shouldAccelerate = _controller.InputDirection.magnitude >= 0.1f;

        // Выбираем целевую скорость
        float targetSpeed = shouldAccelerate ? _controller.maxMoveSpeed : _controller.baseMoveSpeed;

        // Плавно изменяем текущую скорость к целевой
        float newSpeed = Mathf.MoveTowards(_controller.CurrentMoveSpeed, targetSpeed, _controller.speedChangeRate * Time.deltaTime);

        // Записываем новое значение в контроллер
        _controller.CurrentMoveSpeed = newSpeed;
    }

    private void HandleMovement()
    {
        // Получаем текущую скорость из контроллера
        Vector3 currentVelocity = _controller.PlayerVelocity;

        // Если есть ввод от игрока
        if (_controller.InputDirection.magnitude >= 0.1f)
        {
            // --- Поворот персонажа ---
            // Вычисляем угол поворота относительно камеры
            float targetAngle = Mathf.Atan2(_controller.InputDirection.x, _controller.InputDirection.z) * Mathf.Rad2Deg + _controller.MainCamera.transform.eulerAngles.y;

            // Плавно поворачиваем персонажа
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _controller.TurnSmoothVelocity, _controller.turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);


            // --- Движение персонажа ---
            // Направление движения совпадает с направлением поворота
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Устанавливаем горизонтальную скорость
            currentVelocity.x = moveDir.x * _controller.CurrentMoveSpeed;
            currentVelocity.z = moveDir.z * _controller.CurrentMoveSpeed;
        }
        else // Если ввода нет, применяем трение
        {
            // Плавно замедляем горизонтальную скорость до нуля
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, 0, groundFriction * Time.deltaTime);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, 0, groundFriction * Time.deltaTime);
        }

        // Возвращаем измененный вектор скорости обратно в контроллер
        _controller.PlayerVelocity = currentVelocity;
    }

    private void HandleJump()
    {
        // Проверяем, нажата ли кнопка прыжка и доступно ли "время койота"
        if (Input.GetButtonDown("Jump") && _controller.CanUseCoyoteTime())
        {
            // Сбрасываем таймер койота, чтобы нельзя было прыгнуть дважды
            _controller.ConsumeCoyoteTime();

            // Получаем текущую скорость
            var velocity = _controller.PlayerVelocity;

            // Рассчитываем и применяем вертикальную скорость для прыжка
            velocity.y = Mathf.Sqrt(_controller.jumpHeight * -2f * _controller.GravityValue);

            // Записываем измененную скорость обратно в контроллер
            _controller.PlayerVelocity = velocity;

            // Сообщаем контроллеру, что нужно сменить состояние на "в воздухе"
            _controller.SetState(PlayerController.PlayerState.InAir);
        }
    }
}