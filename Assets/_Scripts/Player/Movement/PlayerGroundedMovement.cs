using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerGroundedMovement : MonoBehaviour
{
    // --- ��������� ������ (����� � ����������) ---
    [Header("��������� �������� �� �����")]

    [Tooltip("��������� ������ �������� ��������������� �� ����� (������)")]
    public float groundFriction = 10f;



    // ������ �� ������� ���������� ��� ������� � ����� ������
    private PlayerController _controller;
    private CharacterController _characterController; // �������� ��� ��������

    private void Awake()
    {
        // �������� ������ ��� ������
        _controller = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    // ���� ����� ���������� �� Update() �������� �����������,
    // ����� CurrentState == PlayerState.Grounded
    public void TickUpdate()
    {
        HandleSpeed();
        HandleMovement();
        HandleJump();
    }

    private void HandleSpeed()
    {
        // ����������, ������ �� �������� ����������
        bool shouldAccelerate = _controller.InputDirection.magnitude >= 0.1f;

        // �������� ������� ��������
        float targetSpeed = shouldAccelerate ? _controller.maxMoveSpeed : _controller.baseMoveSpeed;

        // ������ �������� ������� �������� � �������
        float newSpeed = Mathf.MoveTowards(_controller.CurrentMoveSpeed, targetSpeed, _controller.speedChangeRate * Time.deltaTime);

        // ���������� ����� �������� � ����������
        _controller.CurrentMoveSpeed = newSpeed;
    }

    private void HandleMovement()
    {
        // �������� ������� �������� �� �����������
        Vector3 currentVelocity = _controller.PlayerVelocity;

        // ���� ���� ���� �� ������
        if (_controller.InputDirection.magnitude >= 0.1f)
        {
            // --- ������� ��������� ---
            // ��������� ���� �������� ������������ ������
            float targetAngle = Mathf.Atan2(_controller.InputDirection.x, _controller.InputDirection.z) * Mathf.Rad2Deg + _controller.MainCamera.transform.eulerAngles.y;

            // ������ ������������ ���������
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _controller.TurnSmoothVelocity, _controller.turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);


            // --- �������� ��������� ---
            // ����������� �������� ��������� � ������������ ��������
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // ������������� �������������� ��������
            currentVelocity.x = moveDir.x * _controller.CurrentMoveSpeed;
            currentVelocity.z = moveDir.z * _controller.CurrentMoveSpeed;
        }
        else // ���� ����� ���, ��������� ������
        {
            // ������ ��������� �������������� �������� �� ����
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, 0, groundFriction * Time.deltaTime);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, 0, groundFriction * Time.deltaTime);
        }

        // ���������� ���������� ������ �������� ������� � ����������
        _controller.PlayerVelocity = currentVelocity;
    }

    private void HandleJump()
    {
        // ���������, ������ �� ������ ������ � �������� �� "����� ������"
        if (Input.GetButtonDown("Jump") && _controller.CanUseCoyoteTime())
        {
            // ���������� ������ ������, ����� ������ ���� �������� ������
            _controller.ConsumeCoyoteTime();

            // �������� ������� ��������
            var velocity = _controller.PlayerVelocity;

            // ������������ � ��������� ������������ �������� ��� ������
            velocity.y = Mathf.Sqrt(_controller.jumpHeight * -2f * _controller.GravityValue);

            // ���������� ���������� �������� ������� � ����������
            _controller.PlayerVelocity = velocity;

            // �������� �����������, ��� ����� ������� ��������� �� "� �������"
            _controller.SetState(PlayerController.PlayerState.InAir);
        }
    }
}