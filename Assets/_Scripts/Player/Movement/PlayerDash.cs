using UnityEngine;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerDash : MonoBehaviour
{
    [Header("��������� ����")]
    [Tooltip("�������� �����")]
    public float dashSpeedMultiplier = 1.5f;
    [Tooltip("��� ����� ������ ����� � ��������")]
    public float dashDuration = 0.2f;
    [Tooltip("����� ����������� ����� � ��������")]
    public float dashCooldown = 2f;

    // ��������� ��������, ����� ������ ������ (� ����������) �����, ��� �� � �����
    public bool IsDashing { get; private set; }

    // ������
    private PlayerController _controller;
    public event Action OnDash;

    // ���������� �������
    private float cooldownTimer;
    private float dashTimer;
    private float targetDashSpeed;

    // ID ��������
    private readonly int animIDDash = Animator.StringToHash("Dash");

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // ���� ����� ����� ���������� ������ ���� �� �������� �����������
    public void TickUpdate()
    {
        // ������� ������ ������ �����������
        UpdateTimers();

        if (IsDashing)
        {
            // ���� �� � �����, �� ������ ���������� ��������� ������
            HandleDashing();
        }
        else
        {
            // ���� �� �� � �����, ���������, �� ����� �� ����� ������
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
        // ���������� "Fire3" (�� ��������� Left Shift). ����� �������� �� ���� ������ � Edit -> Project Settings -> Input Manager
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

        // 1. �������� ������� �������������� �������� � ������ ������ ����.
        float startSpeed = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z).magnitude;

        // 2. ������������ ������� �������� � ��������� ��.
        // ���� ����� ����� �� �����, ���������� ��� ������� ��������, ����� ��� ��� ����� ��� ������.
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
        // ���� ��� �������, �� ��������� ������������ ��������, ����� �� ��� �� ������, ��������, ����������
        Vector3 dashVelocity = transform.forward * targetDashSpeed;
        /*dashVelocity.y = 0;*/
        _controller.PlayerVelocity = new Vector3(dashVelocity.x, 0, dashVelocity.z);
    }

    private void EndDash()
    {
        IsDashing = false;

        // ����� ����� �������� ��������� �������� � ������� ���������,
        // � �� ���� ����� ������ ����������� ������� ���� (����������, ������).
        // �� �� �������� ��������, ����� �������� ���� �������.
    }
}