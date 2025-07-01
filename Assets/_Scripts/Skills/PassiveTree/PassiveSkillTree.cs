using System.Collections.Generic;
using UnityEngine;

// ���� ������� �������� ��� ������� ��������� ����� ��������� ����� � ���������
[CreateAssetMenu(fileName = "PassiveSkillTree", menuName = "Skills/Passive Skill Tree")]
public class PassiveSkillTree : ScriptableObject
{
    // ������ ������, � ������� �� ����� ������������� ��� ���� ������ ��������� �������
    public List<PassiveSkillData> allSkills;
}