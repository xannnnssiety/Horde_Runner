using UnityEngine;
using System;

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
    [Tooltip("Угол наклона самого персонажа во время бега по стене")]
    public float playerTiltAngle = 15f;

    // Публичные свойства
    public bool IsWallRunning { get; private set; }
    public Vector3 WallNormal { get; private set; }
    public event Action OnJump;
    // Ссылки
    private PlayerController _controller;

    // Внутренние переменные
    private Vector3 wallRunDirection;
    private float wallJumpCooldownTimer; // Таймер "иммунитета" после прыжка
    private Coroutine resetTiltCoroutine;

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
        if (resetTiltCoroutine != null)
        {
            StopCoroutine(resetTiltCoroutine);
            resetTiltCoroutine = null;
        }

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
        Quaternion lookRotation = Quaternion.LookRotation(wallRunDirection);

        // 2. Определяем наклон
        // Узнаем, справа или слева стена, чтобы наклониться в нужную сторону
        bool isWallOnRight = Vector3.Dot(WallNormal, transform.right) > 0;
        float tiltAngle = isWallOnRight ? -playerTiltAngle : playerTiltAngle;  // Наклон в градусах. Можно вынести в настройки.

        // 3. Создаем кватернион для наклона
        Quaternion tilt = Quaternion.Euler(0, 0, tiltAngle);

        // 4. Комбинируем поворот и наклон
        Quaternion targetRotation = lookRotation * tilt;

        // 5. Плавно применяем финальный поворот
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

        OnJump?.Invoke();

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
        if (!IsWallRunning) return;
        IsWallRunning = false;

        if (resetTiltCoroutine == null)
        {
            resetTiltCoroutine = StartCoroutine(ResetTilt());
        }

        // Переходим в состояние полета, только если мы не на земле
        if (!_controller.IsGrounded && _controller.CurrentState == PlayerController.PlayerState.WallRunning)
        {
            _controller.SetState(PlayerController.PlayerState.InAir);
        }
    }

    private System.Collections.IEnumerator ResetTilt()
    {
        Quaternion currentRotation = transform.rotation;
        // Целевой поворот - это текущий поворот по Y, но с нулевым наклоном по X и Z
        Quaternion targetRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y, 0);

        float timer = 0f;
        float duration = 0.25f; // Как быстро выпрямиться

        while (timer < duration)
        {
            // Плавно интерполируем к вертикальному положению
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, timer / duration);
            timer += Time.deltaTime;
            yield return null; // Ждем следующего кадра
        }

        // Гарантируем точное финальное положение
        transform.rotation = targetRotation;
        resetTiltCoroutine = null; // Сбрасываем ссылку на корутину
    }

}