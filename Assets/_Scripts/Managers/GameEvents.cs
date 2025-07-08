using System;

public static class GameEvents
{
    // ������ �������, ������� ������ �������� "���� ����"
    public static event Action OnEnemyDied;

    // --- ��������� ---
    // ����� �������, ������� �������� "������� ������� ���������" � �������� ����� ����.
    public static event Action<int> OnKillCountChanged;

    public static void ReportEnemyDied()
    {
        OnEnemyDied?.Invoke();
    }

    // --- ��������� ---
    // ����� ��� ������ ������ �������
    public static void ReportKillCountChanged(int newTotalKills)
    {
        OnKillCountChanged?.Invoke(newTotalKills);
    }
}