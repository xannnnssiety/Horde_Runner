using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAirborneMovement : MonoBehaviour
{
    // --- НАСТРОЙКИ МОДУЛЯ ---
    [Header("Настройки управления в воздухе")]
    [Tooltip("Насколько резко персонаж меняет направление в воздухе")]
    public float airControlRate = 10f;

    // Ссылка на главный контроллер
    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // Этот метод вызывается из Update() главного контроллера,
    // когда CurrentState == PlayerState.InAir
    public void TickUpdate()
    {
        HandleAirControl();
    }

    private void HandleAirControl()
    {
        // Если есть ввод от игрока
        if (_controller.InputDirection.magnitude >= 0.1f)
        {
            // --- Поворот персонажа (как и на земле) ---
            float targetAngle = Mathf.Atan2(_controller.InputDirection.x, _controller.InputDirection.z) * Mathf.Rad2Deg + _controller.MainCamera.transform.eulerAngles.y;

            // Плавно поворачиваем персонажа
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _controller.TurnSmoothVelocity, _controller.turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // --- Изменение скорости в полете ---
            // Мы плавно меняем текущую горизонтальную скорость в сторону нового направления
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Целевая горизонтальная скорость
            Vector3 targetVelocity = moveDir * _controller.CurrentMoveSpeed;

            // Получаем текущую скорость из контроллера
            Vector3 currentVelocity = _controller.PlayerVelocity;

            // Lerp обеспечивает плавное управление в воздухе без резких остановок или ускорений.
            // Мы меняем только горизонтальные составляющие (x и z).
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, targetVelocity.x, airControlRate * Time.deltaTime);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, targetVelocity.z, airControlRate * Time.deltaTime);

            // Возвращаем измененный вектор скорости обратно в контроллер
            _controller.PlayerVelocity = currentVelocity;
        }
    }
}