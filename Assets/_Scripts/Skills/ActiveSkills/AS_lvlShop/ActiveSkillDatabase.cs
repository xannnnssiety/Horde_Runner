using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject, ������� ������ ����� ������ ��� ���� �������� ������ 1-�� ������.
/// </summary>
[CreateAssetMenu(fileName = "ActiveSkillDatabase", menuName = "Skills/Active Skill Database")]
public class ActiveSkillDatabase : ScriptableObject
{
    [Header("���� ������ �������� ������")]
    [Tooltip("���������� ���� ��� ������ �������� ������ 1-�� ������, ������� ����� ���� ���������� ������.")]
    public List<ActiveSkillData> allFirstLevelSkills;
}