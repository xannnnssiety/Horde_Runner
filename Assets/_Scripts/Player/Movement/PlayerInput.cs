using UnityEngine;

// ���� ��������� �������, ����� �� ������� ��� ������� PlayerController
[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour
{
    // ������ �� ������� ���������� ��� ������ ������
    private PlayerController _controller;

    private void Awake()
    {
        // �������� ������ �� ���������� ��� ������
        _controller = GetComponent<PlayerController>();
    }

    // ���� ����� ����� ���������� �� Update() �������� �����������
    public void TickUpdate()
    {
        // ��������� ��� �����
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // ������� ������ ����������� � ����������� ���, ����� �������� ��������� �� ���������
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // ���������� ���������� ����������� � ��������� �������� �������� �����������,
        // ����� ��� ��������� ������ ����� ��� ���������
        _controller.InputDirection = direction;

        // ����� �� ����� ����� ������������ � ������ �������, ��������:
        // if (Input.GetButtonDown("Dash")) { _controller.OnDashInput(); }
        // if (Input.GetButtonDown("Shoot")) { _controller.OnShootInput(); }
    }
}