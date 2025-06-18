using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerWallRun : MonoBehaviour
{
    [Header("Настройки бега по стенам")]
    public string wallRunnableTag = "WallRunnable";
    public float wallAttractionForce = 20f;
    public float wallJumpSideForce = 12f; // Увеличил значение по умолчанию для более явного эффекта

    [Header("Проверка угла для бега")]
    [Range(1f, 90f)]
    public float maxAngleForWallRun = 45f;

    // Публичные свойства
    public bool IsWallRunning { get; private set; }
    public Vector3 WallNormal { get; private set; }

    // Ссылки
    private PlayerController _controller;

    // Внутренние переменные
    private Vector3 wallRunDirection;
    private float wallJumpCooldownTimer; // Таймер "иммунитета" после прыжка

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    public void TickUpdate()
    {
        // Обновляем таймер иммунитета
        if (wallJumpCooldownTimer > 0)
        {
            wallJumpCooldownTimer -= Time.deltaTime;
            // Если мы в фазе иммунитета, ничего больше не делаем.
            // Персонаж летит по инерции от прыжка.
            return;
        }

        // Проверяем стены только если не на земле
        if (!_controller.IsGrounded)
        {
            CheckForWallAndManageState();
        }
        else if (IsWallRunning)
        {
            // Если приземлились, прекращаем бег
            StopWallRun();
        }
    }

    private void CheckForWallAndManageState()
    {
        bool isWallRight = Physics.Raycast(transform.position, transform.right, out RaycastHit rightWallHit, 1f) && rightWallHit.collider.CompareTag(wallRunnableTag);
        bool isWallLeft = Physics.Raycast(transform.position, -transform.right, out RaycastHit leftWallHit, 1f) && leftWallHit.collider.CompareTag(wallRunnableTag);

        if (IsWallRunning)
        {
            // Если мы уже бежим, продолжаем бег или останавливаемся
            // Проверяем, та же ли стена все еще рядом
            if ((isWallRight && rightWallHit.normal == WallNormal) || (isWallLeft && leftWallHit.normal == WallNormal))
            {
                ContinueWallRun();
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            // Если мы не бежим, проверяем, можем ли начать
            if (CanStartWallRun(isWallRight, isWallLeft, rightWallHit, leftWallHit))
            {
                StartWallRun(isWallRight ? rightWallHit : leftWallHit);
            }
        }
    }

    private bool CanStartWallRun(bool isWallRight, bool isWallLeft, RaycastHit rightWallHit, RaycastHit leftWallHit)
    {
        if (!isWallRight && !isWallLeft) return false;

        RaycastHit activeWallHit = isWallRight ? rightWallHit : leftWallHit;
        Vector3 wallDirection = Vector3.Cross(activeWallHit.normal, Vector3.up);
        float angle = Vector3.Angle(transform.forward, wallDirection);
        float angleReversed = Vector3.Angle(transform.forward, -wallDirection);
        float horizontalSpeed = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z).magnitude;

        return Mathf.Min(angle, angleReversed) < maxAngleForWallRun && horizontalSpeed > 2f;
    }

    private void StartWallRun(RaycastHit hit)
    {
        IsWallRunning = true;
        _controller.SetState(PlayerController.PlayerState.WallRunning);

        WallNormal = hit.normal;
        wallRunDirection = Vector3.Cross(WallNormal, Vector3.up);
        if (Vector3.Dot(wallRunDirection, transform.forward) < 0)
        {
            wallRunDirection = -wallRunDirection;
        }
    }

    private void ContinueWallRun()
    {
        // --- ПРЫЖОК ---
        if (Input.GetButtonDown("Jump"))
        {
            HandleWallJump();
            return;
        }

        // --- ДВИЖЕНИЕ ---
        UpdateSpeed();
        Vector3 runVelocity = wallRunDirection * _controller.CurrentMoveSpeed;
        Vector3 attractionVelocity = -WallNormal * wallAttractionForce;
        _controller.PlayerVelocity = runVelocity + attractionVelocity; // Y-составляющая обнулится в runVelocity, если wallRunDirection горизонтальна
        _controller.PlayerVelocity = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z); // Принудительное обнуление Y

        // --- ПОВОРОТ ---
        Quaternion targetRotation = Quaternion.LookRotation(wallRunDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    private void HandleWallJump()
    {
        // --- НОВЫЙ ПОДХОД К ПРЫЖКУ ---
        // 1. Немедленно прекращаем бег по стене
        StopWallRun();

        // 2. Рассчитываем и применяем скорость прыжка
        Vector3 sidewaysForce = WallNormal * wallJumpSideForce;
        float verticalVelocity = Mathf.Sqrt(_controller.jumpHeight * -2f * _controller.GravityValue);
        _controller.PlayerVelocity = new Vector3(sidewaysForce.x, verticalVelocity, sidewaysForce.z);

        // 3. Запускаем таймер "иммунитета" на очень короткое время
        wallJumpCooldownTimer = 0.2f; // В течение 0.2с на игрока не будут действовать другие силы из этого скрипта

        _controller.Animator.SetTrigger("Jump");
    }

    private void UpdateSpeed()
    {
        float targetSpeed = _controller.maxMoveSpeed;
        float newSpeed = Mathf.MoveTowards(_controller.CurrentMoveSpeed, targetSpeed, _controller.speedChangeRate * Time.deltaTime);
        _controller.CurrentMoveSpeed = newSpeed;
    }

    private void StopWallRun()
    {
        IsWallRunning = false;
        // Переходим в состояние полета, только если мы не на земле
        if (!_controller.IsGrounded && _controller.CurrentState == PlayerController.PlayerState.WallRunning)
        {
            _controller.SetState(PlayerController.PlayerState.InAir);
        }
    }
}