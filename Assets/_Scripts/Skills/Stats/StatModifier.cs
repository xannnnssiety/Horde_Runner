// ������� [System.Serializable] ����� �����. �� ��������� Unity
// ���������� ������� ����� ������ � ���������� ������ ������ ������� (��������, � ������).
using UnityEngine;

[System.Serializable]
public class StatModifier
{
    [Tooltip("��������������, ������� ����� ��������")]
    public StatType Stat;

    [Tooltip("�������� ������������")]
    public float Value;

    [Tooltip("��� ������ ��������� ����������� (��������, ��������� � �.�.)")]
    public ModifierType Type;

    [Tooltip("�� ����� ���� ������ ��������� ���� ����������� (����� ������� ���������)")]
    public SkillTag AppliesToTags;

    // ����������� ��� �������� �������� ������������� �� ���� (�����������, �� �������)
    public StatModifier(StatType stat, float value, ModifierType type, SkillTag tags)
    {
        Stat = stat;
        Value = value;
        Type = type;
        AppliesToTags = tags;
    }
}