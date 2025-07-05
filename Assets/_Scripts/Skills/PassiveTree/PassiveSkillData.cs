using System.Collections.Generic;
using UnityEngine;

// ������� CreateAssetMenu ��������� ��������� ���������� ����� �������
// ����� � ��������� Unity ����� ���� Create -> ...
[CreateAssetMenu(fileName = "NewPassiveSkill", menuName = "Skills/Passive Skill Data")]
public class PassiveSkillData : ScriptableObject
{
    [Header("�������� ����������")]
    [Tooltip("���������� ID ������. �� ������ �����������.")]
    public string skillID;

    [Tooltip("��������, ������� ����� �����")]
    public string skillName;

    [Tooltip("��������� ��������, ������� ����� �����")]
    [TextArea(3, 5)] // ������ ��������� ���� � ���������� ������
    public string description;

    [Tooltip("������ ������")]
    public Sprite icon;

    [Header("��������� � ������")]
    [Tooltip("��������� ������������� � ����� ��������")]
    public int cost = 1;

    [Tooltip("���/������ ������ � ������")]
    public PassiveSkillTier skillTier = PassiveSkillTier.Normal;

    [Tooltip("����������� ���������� ��� ����������� � UI-�����")]
    public Vector2 gridPosition;

    public enum InterGroupLogicType { AND, OR }

    [Header("���������� ��� ��������")]
    [Tooltip("AND: ������ ���� ��������� ��� ������ ����������. OR: ������ ���� ��������� ���� �� ���� ������.")]
    public InterGroupLogicType groupLogicType = InterGroupLogicType.AND;

    [Tooltip("������ ����� ����������. ������ ������ ����� ���� ���������� ������.")]
    public List<PrerequisiteGroup> prerequisiteGroups;
    /*public List<PassiveSkillData> prerequisites;*/

    [Header("������� �������")]
    [Tooltip("������ ���� �������������, ������� ���� ���� �����")]
    public List<StatModifier> modifiers;

    [Header("���������� ��������� (�����������)")]
    [Tooltip("������ � ���������� ������� (��������, ���� �� �����)")]
    public GameObject uniqueBehaviourPrefab;
}