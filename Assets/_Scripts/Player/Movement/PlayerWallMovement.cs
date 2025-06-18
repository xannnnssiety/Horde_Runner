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

    [Header("�������� ���� ��� ����������")]
    [Tooltip("������������ ���� (� ��������) ��� ��������� ����������. 90 = ����� �����, 180 = ����� ����� ����� ����.")]
    [Range(91f, 180f)]
    public float minAngleForWallSlide = 160f;

    [Header("��������� ���������� �� �����")]
    [Tooltip("������������ ������������ ���������� �� ����� � ��������")]
    public float maxWallSlideDuration = 2f;

    // ������ �� ����������
    private PlayerController _controller;

    // ��������� ���������� ���������
    private Vector3 wallNormal;
    private float wallSlideTimer;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // ���������� �� Update() �������� �����������, ����� �������� � �������
    public void TickUpdate()
    {
        // �� ��������� �����, ���� ��� ������� ��� �� �����
        if (_controller.GetComponent<PlayerWallRun>().IsWallRunning)
        {
            ResetAndStopSliding();
            return;
        }

        bool wallInFront = CheckForWallInFront();

        if (wallInFront && !_controller.IsGrounded)
        {
            // ���� �� ������ ��� ������ ���������
            if (!_controller.IsWallSliding)
            {
                StartWallSliding();
            }
            UpdateWallSliding();
        }
        else
        {
            // ���� �� ������ �� � �����
            if (_controller.IsWallSliding)
            {
                ResetAndStopSliding();
            }
        }
    }

    private void UpdateWallSliding()
    {
        wallSlideTimer -= Time.deltaTime;

        // ���� ����� �����, "��������" �� �����
        if (wallSlideTimer <= 0)
        {
            ResetAndStopSliding();
            return;
        }

        // ���������� �������
        var velocity = _controller.PlayerVelocity;
        if (velocity.y < -wallSlideSpeed)
        {
            velocity.y = -wallSlideSpeed;
        }
        _controller.PlayerVelocity = velocity;

        // ��������� ������ �� ����� (������ �� ������� �������)
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
            ResetAndStopSliding();
        }
    }

    private void ResetAndStopSliding()
    {
        if (!_controller.IsWallSliding) return; // �������, ���� � ��� �� ��������

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
        wallSlideTimer = maxWallSlideDuration; // ��������� ������
    }

    private bool CheckForWallInFront()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, wallJumpableLayer))
        {
            float angle = Vector3.Angle(transform.forward, -hit.normal);
            if (angle < (180 - minAngleForWallSlide))
            {
                wallNormal = hit.normal; // ��������� �������, ���� ����� �������
                return true;
            }
        }
        return false;
    }

    private void CheckForWall()
    {
        // --- ����� �������� �� ���������� ������ ������� ---
        // �� ��������� �����, ���� ��� ������� ��� �� �����
        if (_controller.GetComponent<PlayerWallRun>().IsWallRunning)
        {
            // ���� ���� ���������� ��� �������, ���������� ���
            if (_controller.IsWallSliding)
            {
                _controller.IsWallSliding = false;
                _controller.SetState(PlayerController.PlayerState.InAir);
            }
            return;
        }

        if (!_controller.IsGrounded && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, wallJumpableLayer))
        {
            // --- ����� �������� ���� ---
            // Vector3.forward - ��� ���� ������� �������� (����������� �����)
            // hit.normal - ��� ������, "��������" �� ����� � ���� �������
            float angle = Vector3.Angle(transform.forward, -hit.normal);

            // ���� ���� ����� ����� ������������ � ������������ "� �����" ���������� ���
            // (��� ������������ �������� ���� ����� ����� ������������ � �������� �����),
            // �� �� ����� �� ��� "� ���".
            if (angle < (180 - minAngleForWallSlide))
            {
                if (!_controller.IsWallSliding)
                {
                    _controller.SetState(PlayerController.PlayerState.WallSliding);
                }
                _controller.IsWallSliding = true;
                wallNormal = hit.normal;
                return; // �������, ����� �� ���������� ���� ����
            }
        }

        // ���� �� ���� �� ������� ���������� �� �����������, ��������� ���
        if (_controller.IsWallSliding)
        {
            _controller.IsWallSliding = false;
            _controller.SetState(PlayerController.PlayerState.InAir);
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