using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimation : MonoBehaviour
{
    // --- ID ���������� ��������� ---
    // ������������� StringToHash ������� ����������������, ��� �������� ����� ������ ����
    private readonly int animIDSpeed = Animator.StringToHash("Speed");
    private readonly int animIDGrounded = Animator.StringToHash("Grounded");
    private readonly int animIDJump = Animator.StringToHash("Jump");
    private readonly int animIDFreeFall = Animator.StringToHash("FreeFall");
    private readonly int animIDWallSliding = Animator.StringToHash("WallSliding"); // �����������, � ��� ���� ����� ��������
    private readonly int animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

    // ������
    private PlayerController _controller;
    private Animator _animator;

    // ���������� ��� ������������ ���������
    private bool hasJumpedThisFrame = false;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
    }

    // ���������� �� Update() �������� ����������� � ����� ����� �����
    public void TickUpdate()
    {
        UpdateGroundedAndFallingState();
        UpdateSpeed();
        HandleJumpAnimation();
    }

    private void UpdateGroundedAndFallingState()
    {
        // �������������, �� ����� �� ��������. ������� ��� ��������� � Idle/Locomotion.
        _animator.SetBool(animIDGrounded, _controller.IsGrounded);

        // �������������, �������� �� �������� �� �����.
        _animator.SetBool(animIDWallSliding, _controller.IsWallSliding);

        // ���� �� � ������� � �� �������� �� ����� - �� � ��������� �������.
        bool isFalling = _controller.CurrentState == PlayerController.PlayerState.InAir && !_controller.IsWallSliding;
        _animator.SetBool(animIDFreeFall, isFalling);
    }

    private void UpdateSpeed()
    {
        // ��������� �������������� ��������
        float horizontalSpeed = new Vector3(_controller.PlayerVelocity.x, 0.0f, _controller.PlayerVelocity.z).magnitude;

        // �������� ����������� �����, ����� �������� ������/���� ���� 1, � �� 0.5
        float inputMagnitude = _controller.InputDirection.magnitude;

        // �������� �������� � �������� ����� � ��������.
        // animIDSpeed ������������ ��� �������� �������� (1 = ���, 0 = ������).
        // animIDMotionSpeed ������������ ��� ���������� ��������� ����� ��������, ����� �������� "������ �������".
        _animator.SetFloat(animIDSpeed, horizontalSpeed);
        _animator.SetFloat(animIDMotionSpeed, inputMagnitude);
    }

    private void HandleJumpAnimation()
    {
        // ���� ����� ������� �������, ��� ��� ������ - ��� ����������� ������� (�������).
        // ��� ����� "�������" ������, ����� ������ ���������.

        // ������� ������: ���� �� ���� �� �����, � � ��������� ����� ��������� � �������, ������ ��� ������.
        if (!_controller.IsGrounded && _controller.CharacterController.velocity.y > 0 && !hasJumpedThisFrame)
        {
            if (_controller.CurrentState == PlayerController.PlayerState.InAir)
            {
                _animator.SetTrigger(animIDJump);
                hasJumpedThisFrame = true; // ������������� ����, ����� �� ���������� �������� ������ ���� ������ �����
            }
        }

        // ���������� ����, ��� ������ ��������� �����
        if (_controller.IsGrounded)
        {
            hasJumpedThisFrame = false;
        }
    }
}