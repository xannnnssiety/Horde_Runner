using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] �����������, ����� ���� ����� ����������� � ����������
[System.Serializable]
public class PrerequisiteGroup
{
    // ����������, ��� ������ ���� ��������� ���������� ������ ���� ������
    public enum GroupLogicType { AND, OR }

    [Tooltip("AND: ����� ������� ��� ������ � ���� ������. OR: ����� ������� ���� �� ���� ����� � ���� ������.")]
    public GroupLogicType logicType = GroupLogicType.AND;

    [Tooltip("������ �������, � ������� ����������� ������ ���� ������.")]
    public List<PassiveSkillData> requiredSkills;
}