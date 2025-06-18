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

    [Header("Проверка угла для скольжения")]
    [Tooltip("Максимальный угол (в градусах) для активации скольжения. 90 = стена сбоку, 180 = стена прямо перед нами.")]
    [Range(91f, 180f)]
    public float minAngleForWallSlide = 160f;

    [Header("Настройки скольжения по стене")]
    [Tooltip("Максимальная длительность скольжения по стене в секундах")]
    public float maxWallSlideDuration = 2f;

    // Ссылки на компоненты
    private PlayerController _controller;

    // Приватные переменные состояния
    private Vector3 wallNormal;
    private float wallSlideTimer;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // Вызывается из Update() главного контроллера, когда персонаж в воздухе
    public void TickUpdate()
    {
        // Не проверяем стену, если уже активен бег по стене
        if (_controller.GetComponent<PlayerWallRun>().IsWallRunning)
        {
            ResetAndStopSliding();
            return;
        }

        bool wallInFront = CheckForWallInFront();

        if (wallInFront && !_controller.IsGrounded)
        {
            // Если мы только что начали скользить
            if (!_controller.IsWallSliding)
            {
                StartWallSliding();
            }
            UpdateWallSliding();
        }
        else
        {
            // Если мы больше не у стены
            if (_controller.IsWallSliding)
            {
                ResetAndStopSliding();
            }
        }
    }

    private void UpdateWallSliding()
    {
        wallSlideTimer -= Time.deltaTime;

        // Если время вышло, "отлипаем" от стены
        if (wallSlideTimer <= 0)
        {
            ResetAndStopSliding();
            return;
        }

        // Замедление падения
        var velocity = _controller.PlayerVelocity;
        if (velocity.y < -wallSlideSpeed)
        {
            velocity.y = -wallSlideSpeed;
        }
        _controller.PlayerVelocity = velocity;

        // Обработка прыжка от стены (логика из старого скрипта)
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
            ResetAndStopSliding();
        }
    }

    private void ResetAndStopSliding()
    {
        if (!_controller.IsWallSliding) return; // Выходим, если и так не скользим

        _controller.IsWallSliding = false;
        if (_controller.CurrentState == PlayerController.PlayerState.WallSliding)
        {
            _controller.SetState(PlayerController.PlayerState.InAir);
        }
    }

    private void StartWallSliding()
    {
        _controller.IsWallSliding = true;
        _controller.SetState(PlayerController.PlayerState.WallSliding);
        wallSlideTimer = maxWallSlideDuration; // Запускаем таймер
    }

    private bool CheckForWallInFront()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, wallJumpableLayer))
        {
            float angle = Vector3.Angle(transform.forward, -hit.normal);
            if (angle < (180 - minAngleForWallSlide))
            {
                wallNormal = hit.normal; // Сохраняем нормаль, если стена найдена
                return true;
            }
        }
        return false;
    }

    private void CheckForWall()
    {
        // --- НОВАЯ ПРОВЕРКА НА АКТИВНОСТЬ ДРУГИХ МОДУЛЕЙ ---
        // Не проверяем стену, если уже активен бег по стене
        if (_controller.GetComponent<PlayerWallRun>().IsWallRunning)
        {
            // Если флаг скольжения еще активен, сбрасываем его
            if (_controller.IsWallSliding)
            {
                _controller.IsWallSliding = false;
                _controller.SetState(PlayerController.PlayerState.InAir);
            }
            return;
        }

        if (!_controller.IsGrounded && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, wallJumpableLayer))
        {
            // --- НОВАЯ ПРОВЕРКА УГЛА ---
            // Vector3.forward - это куда смотрит персонаж (направление атаки)
            // hit.normal - это вектор, "торчащий" из стены в нашу сторону
            float angle = Vector3.Angle(transform.forward, -hit.normal);

            // Если угол между нашим направлением и направлением "в стену" достаточно мал
            // (что эквивалентно большому углу между нашим направлением и нормалью стены),
            // то мы летим на нее "в лоб".
            if (angle < (180 - minAngleForWallSlide))
            {
                if (!_controller.IsWallSliding)
                {
                    _controller.SetState(PlayerController.PlayerState.WallSliding);
                }
                _controller.IsWallSliding = true;
                wallNormal = hit.normal;
                return; // Выходим, чтобы не сбрасывать флаг ниже
            }
        }

        // Если ни одно из условий скольжения не выполнилось, выключаем его
        if (_controller.IsWallSliding)
        {
            _controller.IsWallSliding = false;
            _controller.SetState(PlayerController.PlayerState.InAir);
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