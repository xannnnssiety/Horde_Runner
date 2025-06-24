using UnityEngine;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerDash : MonoBehaviour
{
    [Header("��������� ����")]
    [Tooltip("�������� �����")]
    public float dashSpeed = 25f;
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
        IsDashing = true;
        cooldownTimer = dashCooldown;
        dashTimer = dashDuration;

        // �������� ���������, ��� �� ������ �����
        _controller.Animator.SetTrigger(animIDDash);

        // ������������� �������� �����.
        // ����� ������ ���������� �� ����������� ������� ��������� (transform.forward)
        // �� �������� ������������ ��������, ����� ��� � ������� ��� ��������������
        Vector3 dashVelocity = transform.forward * dashSpeed;
        dashVelocity.y = 0;

        _controller.PlayerVelocity = dashVelocity;
    }

    private void HandleDashing()
    {
        // ���� ��� �������, �� ��������� ������������ ��������, ����� �� ��� �� ������, ��������, ����������
        Vector3 dashVelocity = transform.forward * dashSpeed;
        dashVelocity.y = 0;
        _controller.PlayerVelocity = dashVelocity;
    }

    private void EndDash()
    {
        IsDashing = false;

        // ����� ����� �������� ��������� �������� � ������� ���������,
        // � �� ���� ����� ������ ����������� ������� ���� (����������, ������).
        // �� �� �������� ��������, ����� �������� ���� �������.
    }
}