using System;
using UnityEngine;

public static class GameEvents
{
    // ������ �������, ������� ������ �������� "���� ����"
    public static event Action OnEnemyDied;
    public static event Action<int> OnKillCountChanged;

    [Tooltip("�������, ������� �����������, ����� ����� ���������� �������� ������.")]
    public static event Action<ActiveSkillData> OnPlayerAbilityUsed;

    public static event Action<Vector3, Quaternion> OnDashStarted;
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

    /// <summary>
    /// ���� ����� ������ ���������� ActiveSkillManager'�� � ������ ������������� ������.
    /// �� �������� ������ �� �������������� ������ ���� �����������.
    /// </summary>
    /// <param name="skillData">������ ��������������� ������.</param>
    public static void ReportPlayerAbilityUsed(ActiveSkillData skillData)
    {
        OnPlayerAbilityUsed?.Invoke(skillData);
    }

    public static void ReportDashStarted(Vector3 startPosition, Quaternion startRotation)
    {
        OnDashStarted?.Invoke(startPosition, startRotation);
    }

}