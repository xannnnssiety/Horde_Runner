using UnityEngine;
using UnityEngine.UI; // ����������� �������� ��� ������ ��� ������ � TextMeshPro

/// <summary>
/// ��������� ������ ��� ������������ ������� �������� �������.
/// ��� ������� ��� ���������� �������� �����.
/// ��������� ��������� ���� � ������� ����������� �������.
/// </summary>
public class TempKillCounter : MonoBehaviour
{
    [Header("������ �� UI")]
    [Tooltip("���������� ���� ��������� ������ TextMeshPro �� ����� �����")]
    public Text killCountText;

    private void OnEnable()
    {
        // ������������� �� ������� ��������� ��������, ����� ��������� �����
        GameEvents.OnKillCountChanged += UpdateKillText;
    }

    private void OnDisable()
    {
        // ����������� ������������, ����� �������� ������
        GameEvents.OnKillCountChanged -= UpdateKillText;
    }

    void Update()
    {
        // ���������, ���� �� ������ ����� ������ ���� � ���� �����
        if (Input.GetMouseButtonDown(0))
        {
            // ���������� �������� �����, ������� ���������� �������.
            // GameManager ������� ��� ������� � �������� �������.
            Debug.Log("LMB Clicked! Simulating an enemy kill.");
            GameEvents.ReportEnemyDied();
        }
    }

    /// <summary>
    /// ���� ����� ���������� �������� OnKillCountChanged �� GameEvents.
    /// </summary>
    /// <param name="newTotalKills">����� ����� ���������� �������.</param>
    private void UpdateKillText(int newTotalKills)
    {
        // ���������, �� ������ �� �� ��������� ��������� ���� � ����������
        if (killCountText != null)
        {
            // ��������� ����� �� ������
            killCountText.text = $"Total Kills: {newTotalKills}";
        }
    }
}