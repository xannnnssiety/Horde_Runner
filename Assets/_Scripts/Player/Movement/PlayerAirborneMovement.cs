using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAirborneMovement : MonoBehaviour
{
    // --- ��������� ������ ---
    [Header("��������� ���������� � �������")]
    [Tooltip("��������� ����� �������� ������ ����������� � �������")]
    public float airControlRate = 10f;

    // ������ �� ������� ����������
    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // ���� ����� ���������� �� Update() �������� �����������,
    // ����� CurrentState == PlayerState.InAir
    public void TickUpdate()
    {
        HandleAirControl();
    }

    private void HandleAirControl()
    {
        // ���� ���� ���� �� ������
        if (_controller.InputDirection.magnitude >= 0.1f)
        {
            // --- ������� ��������� (��� � �� �����) ---
            float targetAngle = Mathf.Atan2(_controller.InputDirection.x, _controller.InputDirection.z) * Mathf.Rad2Deg + _controller.MainCamera.transform.eulerAngles.y;

            // ������ ������������ ���������
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _controller.TurnSmoothVelocity, _controller.turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // --- ��������� �������� � ������ ---
            // �� ������ ������ ������� �������������� �������� � ������� ������ �����������
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // ������� �������������� ��������
            Vector3 targetVelocity = moveDir * _controller.CurrentMoveSpeed;

            // �������� ������� �������� �� �����������
            Vector3 currentVelocity = _controller.PlayerVelocity;

            // Lerp ������������ ������� ���������� � ������� ��� ������ ��������� ��� ���������.
            // �� ������ ������ �������������� ������������ (x � z).
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, targetVelocity.x, airControlRate * Time.deltaTime);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, targetVelocity.z, airControlRate * Time.deltaTime);

            // ���������� ���������� ������ �������� ������� � ����������
            _controller.PlayerVelocity = currentVelocity;
        }
    }
}