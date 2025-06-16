using UnityEngine;
using UnityEngine.UI; // ����������� �������� ��� ������ ��� ������ � UI

[RequireComponent(typeof(PlayerController))]
public class PlayerUI : MonoBehaviour
{
    [Header("UI ������")]
    [Tooltip("���������� ���� ��������� ������� (Legacy) ��� ����������� ��������")]
    public Text speedDisplayText;

    // ������ �� ������� ���������� ��� ��������� ������
    private PlayerController _controller;

    private void Awake()
    {
        // �������� ������ �� ����������
        _controller = GetComponent<PlayerController>();
    }

    // ��� UI-���������, ������� ������ "������" ������, 
    // ����� ������� ������������ ����������� Update,
    // ����� ����������� �������� �� �� ������� ������.
    private void Update()
    {
        // ������ ���������, ��������� �� ������ � ����������, ����� �������� ������
        if (speedDisplayText != null)
        {
            // ��������� ����������� �������������� �������� ���������
            float horizontalSpeed = new Vector3(_controller.PlayerVelocity.x, 0f, _controller.PlayerVelocity.z).magnitude;

            // ��������� �����
            speedDisplayText.text = $"Speed: {horizontalSpeed:F2}";
        }
    }
}