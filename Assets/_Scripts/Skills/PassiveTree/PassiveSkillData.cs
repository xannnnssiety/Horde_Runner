using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPassiveSkill", menuName = "Skills/Passive Skill Data")]
public class PassiveSkillData : ScriptableObject
{
    [Header("�������� ����������")]
    [Tooltip("���������� ID ������. �� ������ �����������.")]
    public string skillID;

    [Tooltip("��������, ������� ����� �����")]
    public string skillName;

    [Tooltip("��������� ��������, ������� ����� �����")]
    [TextArea(3, 5)]
    public string description;

    [Tooltip("������ ������")]
    public Sprite icon;

    [Header("������� ������")]
    [Tooltip("������� ��������� ������ �������")]
    public int baseCost = 1;

    [Tooltip("������������ ���������� ���, ������� ����� ������ ���� �����")]
    public int maxPurchaseCount = 1;

    [Header("������� �������")]
    [Tooltip("������ ���� �������������, ������� ���� ���� ����� �� ���� �������")]
    public List<StatModifier> modifiers;

    [Header("���������� ��������� (�����������)")]
    [Tooltip("������ � ���������� �������. ����� ������ ������ ��� ������ �������.")]
    public GameObject uniqueBehaviourPrefab;
}