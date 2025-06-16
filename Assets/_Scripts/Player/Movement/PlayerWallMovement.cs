using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerWallMovement : MonoBehaviour
{
    [Header("��������� ���������� �� �����")]
    [Tooltip("���� ��� ����, �� ������� ����� �������������")]
    public LayerMask wallJumpableLayer;
    [Tooltip("��� �������� �������� �������� �� ����� ����")]
    public float wallSlideSpeed = 2f;
    [Tooltip("��������� ��� �������� ����� ����� ����������")]
    public float wallCheckDistance = 0.5f;

    [Header("��������� ������ �� �����")]
    [Tooltip("������ ������ �� �����")]
    public float wallJumpHeight = 4f;
    [Tooltip("���� ������������ � ������� �� �����")]
    public float wallJumpSidewaysForce = 8f;

    // ������ �� ����������
    private PlayerController _controller;

    // ��������� ���������� ���������
    private Vector3 wallNormal;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // ���������� �� Update() �������� �����������, ����� �������� � �������
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
        // ��������� ������� ����� ������ ���� �� �� �� �����.
        // ���������� transform.forward, ����� ��� ��� �� ����������� ������� ���������.
        if (!_controller.IsGrounded && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, wallJumpableLayer))
        {
            // ����� �������
            if (!_controller.IsWallSliding)
            {
                // �������� ����������
                _controller.SetState(PlayerController.PlayerState.WallSliding);
            }
            _controller.IsWallSliding = true;
            wallNormal = hit.normal; // ��������� ������� ��� ������������
        }
        else
        {
            // ����� ���
            if (_controller.IsWallSliding)
            {
                // ����������� ����������
                _controller.IsWallSliding = false;
                _controller.SetState(PlayerController.PlayerState.InAir);
            }
        }
    }

    private void HandleWallSliding()
    {
        // ���������� �������
        var velocity = _controller.PlayerVelocity;
        if (velocity.y < -wallSlideSpeed)
        {
            velocity.y = -wallSlideSpeed;
        }
        _controller.PlayerVelocity = velocity;
    }

    private void HandleWallJumpInput()
    {
        // ���� ����� ����� ������ �� ����� ����������
        if (Input.GetButtonDown("Jump"))
        {
            // ���������� ����������
            _controller.IsWallSliding = false;

            // --- ������������ ������������ ������ ---
            float verticalVelocity = Mathf.Sqrt(wallJumpHeight * -2f * _controller.GravityValue);

            // --- �������������� ������������ ������ ---
            // ������� ��������� � �����������, �������� ������� �����
            Vector3 jumpDirection = wallNormal * wallJumpSidewaysForce;

            // ������������� ����� �������� � ������� �����������
            _controller.PlayerVelocity = new Vector3(jumpDirection.x, verticalVelocity, jumpDirection.z);

            // ������������ ��������� ����� �� ����� ��� ������� ����������� �������
            transform.rotation = Quaternion.LookRotation(wallNormal);

            // ��������� � ��������� "� �������", ��� ��� �� ������ ��� ����������
            _controller.SetState(PlayerController.PlayerState.InAir);

            // � ������� ����� ����� ����� ������� ������� �������� ������
            // _controller.Animator.SetTrigger("WallJump");
        }
    }
}