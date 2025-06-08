using UnityEngine;

public class RunStatsManager : MonoBehaviour
{
    // �������� ��� ������� �������
    public static RunStatsManager Instance { get; private set; }

    // ����������, ������� �� �����������
    public int totalKills { get; private set; }

    private void Awake()
    {
        // ������������ ���������� ���������
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // ��������������/���������� �������� � ������
        totalKills = 0;
    }

    // �����, ������� �������� ����� ��� ������
    public void RegisterKill()
    {
        totalKills++;
    }
}