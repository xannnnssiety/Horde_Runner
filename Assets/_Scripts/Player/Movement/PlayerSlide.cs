using UnityEngine;
using System;


[RequireComponent(typeof(PlayerController))]
public class PlayerSlide : MonoBehaviour
{
    [Header("��������� �������")]
    [Tooltip("�������� �� ����� ������� �� �����")]
    public float slideSpeedMultiplier = 2f;
    [Tooltip("��� ����� ������ ������ � ��������")]
    public float slideDuration = 0.7f;
    [Tooltip("����� ����������� ������� � ��������")]
    public float slideCooldown = 1f;
    [Tooltip("��������� ���������� ��� ��������� � ������� (����� ������� ������)")]
    public float diveGravity = -200f;
    public event Action OnJump;
    public event Action OnSlideEnd;

    // ��������� ��������, ����� ���������� � ������ ������ ����� � ����� ���������
    public bool IsSliding { get; private set; }

    // ������
    private PlayerController _controller;
    private CharacterController _characterController;

    // ���������� ����������
    private float slideTimer;
    private float cooldownTimer;
    private bool isDiving; // ����, ������������, ��� �� ������������ ������ � ������� � ������
    private float targetSlideSpeed;
    // ID ��������
    private readonly int animIDSlide = Animator.StringToHash("Slide");

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    public void TickUpdate()
    {
        // --- ����� ������ ��� ���������� ������� ---
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // ��������� ����, ������ ���� �� �� � ������ ������ ���������,
        // ����� �� ������� � ������� ������
        if (!IsSliding && cooldownTimer <= 0 && _controller.CurrentState != PlayerController.PlayerState.Grinding)
        {
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                StartSlide();
            }
        }

        UpdateSlideState();
    }

    private void StartSlide()
    {
        slideTimer = slideDuration;

        if (_controller.IsGrounded)
        {
            // ���� �� �� �����, ����� �������� ���������
            BeginGroundSlide();
        }
        else
        {
            // ���� �� � �������, �������� "�����������"
            isDiving = true;
        }
    }

private void UpdateSlideState()
{
        if (isDiving && !_controller.IsGrounded)
        {
            // ���� �����������
        }
        else if (isDiving && _controller.IsGrounded)
        {
            isDiving = false; // <-- ���������� ��������
            BeginGroundSlide();
        }

        // ���� �� � ��������� ���������� �� �����
        if (IsSliding)
        {
        // --- ����� �������� �� ������ ---
            if (Input.GetButtonDown("Jump"))
            {
            
                // �������� ������ ������ �� �����������
                float jumpHeight = _controller.jumpHeight;
            
                // ������������ ������������ �������� ������
                float jumpVelocityY = Mathf.Sqrt(jumpHeight * -2f * _controller.GravityValue);

                // �������� ������� �������������� �������� ������
                Vector3 currentHorizontalVelocity = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z);
            
                // ����������� �������������� �������� �� ������ � ������������ �� ������
                _controller.PlayerVelocity = currentHorizontalVelocity + Vector3.up * jumpVelocityY;
            
                // ���������� ����������� �����
                EndSlide();
            
                // �������� �����������, ��� �� ������ � �������
                _controller.SetState(PlayerController.PlayerState.InAir);

                OnJump?.Invoke();
                
                

                return; // ������� �� ������, ����� �� ������������ ��������� ������ ������
            }

            slideTimer -= Time.deltaTime;
        
            if (slideTimer <= 0)
            {
                EndSlide();
            }
            else
            {
                Vector3 slideVelocity = transform.forward * targetSlideSpeed;
                _controller.PlayerVelocity = new Vector3(slideVelocity.x, _controller.PlayerVelocity.y, slideVelocity.z);
            }
        }
}

    private void BeginGroundSlide()
    {
        IsSliding = true;
        _controller.Animator.SetBool(animIDSlide, true); // ���������� Bool, � �� Trigger

        // 1. �������� ������� �������������� �������� � ������ ������ �������.
        float startSpeed = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z).magnitude;

        // 2. ������������ ������� �������� � ��������� �� � ���� ����������.
        // ���� ��������� �������� ���� ����� ������ (��������, 0), ���������� ���� �� ������� ��������, ����� ��� �������.
        if (startSpeed < _controller.baseMoveSpeed)
        {
            startSpeed = _controller.baseMoveSpeed;
        }
        targetSlideSpeed = startSpeed * slideSpeedMultiplier;
        // ��������, ����������� �������� ������ ����������, ����� �������� "���������"
        // ��������: _characterController.height = 0.8f;
    }

    private void EndSlide()
    {
        
        // ���������, ������������� �� �� ���� � ��������� ����������, ����� �� ��������� ������� ��������
        if (IsSliding)
        {
            cooldownTimer = slideCooldown;

            var velocity = _controller.PlayerVelocity;
            velocity.y = 1.0f;
            _controller.PlayerVelocity = velocity;
        }

        IsSliding = false;
        _controller.Animator.SetBool("Slide", false); // ����� ������������ animIDSlide

        // ���������� ������ ����������, ���� ������
        // ��������: _characterController.height = 1.8f;
    }

    // ��������� �����, ������� ����� �������� ����������, ����� ��������� ����������
    public float GetCurrentGravity()
    {
        return isDiving ? diveGravity : _controller.GravityValue;
    }
}