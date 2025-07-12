using UnityEngine;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerDash : MonoBehaviour
{
    [Header("Настройки Дэша")]
    [Tooltip("Скорость рывка")]
    public float dashSpeedMultiplier = 1.5f;
    [Tooltip("Как долго длится рывок в секундах")]
    public float dashDuration = 0.2f;
    [Tooltip("Время перезарядки рывка в секундах")]
    public float dashCooldown = 2f;

    // Публичное свойство, чтобы другие модули (и контроллер) знали, что мы в рывке
    public bool IsDashing { get; private set; }

    // Ссылки
    private PlayerController _controller;
    public event Action OnDash;

    // Внутренние таймеры
    private float cooldownTimer;
    private float dashTimer;
    private float targetDashSpeed;

    // ID Анимации
    private readonly int animIDDash = Animator.StringToHash("Dash");

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // Этот метод будет вызываться каждый кадр из главного контроллера
    public void TickUpdate()
    {
        // Таймеры всегда должны обновляться
        UpdateTimers();

        if (IsDashing)
        {
            // Если мы в рывке, мы просто продолжаем двигаться вперед
            HandleDashing();
        }
        else
        {
            // Если мы не в рывке, проверяем, не нажал ли игрок кнопку
            CheckForDashInput();
        }
    }

    private void UpdateTimers()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }
    }

    private void CheckForDashInput()
    {

        PlayerController.PlayerState currentState = _controller.CurrentState;
        bool canDash = currentState != PlayerController.PlayerState.Grinding;
        // Используем "Fire3" (по умолчанию Left Shift). Можно изменить на свою кнопку в Edit -> Project Settings -> Input Manager
        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimer <= 0 && canDash)
        {
            OnDash?.Invoke();
            StartDash();
        }
    }

    private void StartDash()
    {
        GameEvents.ReportDashStarted(transform.position, transform.rotation);

        IsDashing = true;
        cooldownTimer = dashCooldown;
        dashTimer = dashDuration;

        // 1. Получаем текущую горизонтальную скорость в момент начала дэша.
        float startSpeed = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z).magnitude;

        // 2. Рассчитываем целевую скорость и СОХРАНЯЕМ ее.
        // Если игрок стоял на месте, используем его базовую скорость, чтобы дэш все равно был мощным.
        if (startSpeed < _controller.baseMoveSpeed)
        {
            startSpeed = _controller.baseMoveSpeed;
        }

        targetDashSpeed = startSpeed * dashSpeedMultiplier;
        Vector3 dashVelocity = transform.forward * targetDashSpeed;
        /*dashVelocity.y = 0;*/

        _controller.PlayerVelocity = dashVelocity;
    }

    private void HandleDashing()
    {
        // Пока дэш активен, мы постоянно поддерживаем скорость, чтобы на нее не влияла, например, гравитация
        Vector3 dashVelocity = transform.forward * targetDashSpeed;
        /*dashVelocity.y = 0;*/
        _controller.PlayerVelocity = new Vector3(dashVelocity.x, 0, dashVelocity.z);
    }

    private void EndDash()
    {
        IsDashing = false;

        // После рывка персонаж продолжит движение с текущей скоростью,
        // и на него снова начнут действовать обычные силы (гравитация, трение).
        // Мы не обнуляем скорость, чтобы движение было плавным.
    }
}