using UnityEngine;

public abstract class BaseSkill : MonoBehaviour
{
    [Header("Base Skill Stats")]
    [Tooltip("����� ����, ���������� ���� ������� �� �����")]
    public float totalDamageDealt = 0;

    // ���� ����� ����� ���������� ��� ���������� � ������ �������� ������.
    // �� ����� �������� �� ���������� ���� ������.
    protected abstract void UpdateSkillStats();

    // ������������� �� �������, ����� ����� ���������� ��������
    protected virtual void OnEnable()
    {
        PlayerStatsManager.OnStatsChanged += UpdateSkillStats;
        // ����� ��������� ����� ��� ��������� ������
        UpdateSkillStats();
    }

    // ������������, ����� ����� ����������� ��� ������������
    protected virtual void OnDisable()
    {
        PlayerStatsManager.OnStatsChanged -= UpdateSkillStats;
    }
}