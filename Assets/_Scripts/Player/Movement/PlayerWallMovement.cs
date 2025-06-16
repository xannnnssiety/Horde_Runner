using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerWallMovement : MonoBehaviour
{
    [Header("Настройки скольжения по стене")]
    [Tooltip("Слой для стен, от которых можно отталкиваться")]
    public LayerMask wallJumpableLayer;
    [Tooltip("Как медленно персонаж скользит по стене вниз")]
    public float wallSlideSpeed = 2f;
    [Tooltip("Дистанция для проверки стены перед персонажем")]
    public float wallCheckDistance = 0.5f;

    [Header("Настройки прыжка от стены")]
    [Tooltip("Высота прыжка от стены")]
    public float wallJumpHeight = 4f;
    [Tooltip("Сила отталкивания в сторону от стены")]
    public float wallJumpSidewaysForce = 8f;

    // Ссылки на компоненты
    private PlayerController _controller;

    // Приватные переменные состояния
    private Vector3 wallNormal;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // Вызывается из Update() главного контроллера, когда персонаж в воздухе
    public void TickUpdate()
    {
        CheckForWall();

        if (_controller.IsWallSliding)
        {
            HandleWallSliding();
            HandleWallJumpInput();
        }
    }

    private void CheckForWall()
    {
        // Проверяем наличие стены только если мы не на земле.
        // Используем transform.forward, чтобы луч шел по направлению взгляда персонажа.
        if (!_controller.IsGrounded && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, wallJumpableLayer))
        {
            // Стена найдена
            if (!_controller.IsWallSliding)
            {
                // Начинаем скольжение
                _controller.SetState(PlayerController.PlayerState.WallSliding);
            }
            _controller.IsWallSliding = true;
            wallNormal = hit.normal; // Сохраняем нормаль для отталкивания
        }
        else
        {
            // Стены нет
            if (_controller.IsWallSliding)
            {
                // Заканчиваем скольжение
                _controller.IsWallSliding = false;
                _controller.SetState(PlayerController.PlayerState.InAir);
            }
        }
    }

    private void HandleWallSliding()
    {
        // Замедление падения
        var velocity = _controller.PlayerVelocity;
        if (velocity.y < -wallSlideSpeed)
        {
            velocity.y = -wallSlideSpeed;
        }
        _controller.PlayerVelocity = velocity;
    }

    private void HandleWallJumpInput()
    {
        // Если игрок нажал прыжок во время скольжения
        if (Input.GetButtonDown("Jump"))
        {
            // Прекращаем скольжение
            _controller.IsWallSliding = false;

            // --- Вертикальная составляющая прыжка ---
            float verticalVelocity = Mathf.Sqrt(wallJumpHeight * -2f * _controller.GravityValue);

            // --- Горизонтальная составляющая прыжка ---
            // Толкаем персонажа в направлении, обратном нормали стены
            Vector3 jumpDirection = wallNormal * wallJumpSidewaysForce;

            // Устанавливаем новую скорость в главном контроллере
            _controller.PlayerVelocity = new Vector3(jumpDirection.x, verticalVelocity, jumpDirection.z);

            // Поворачиваем персонажа лицом от стены для лучшего визуального эффекта
            transform.rotation = Quaternion.LookRotation(wallNormal);

            // Переходим в состояние "в воздухе", так как мы только что отпрыгнули
            _controller.SetState(PlayerController.PlayerState.InAir);

            // В будущем здесь нужно будет вызвать триггер анимации прыжка
            // _controller.Animator.SetTrigger("WallJump");
        }
    }
}