using UnityEngine;

// ������� ��� �������� ������� ����� ���� ����� ���� Unity (Assets -> Create -> Skills -> Active Skill)
[CreateAssetMenu(fileName = "NewActiveSkill", menuName = "Skills/Active Skill")]
public class ActiveSkillData : ScriptableObject
{
    [Header("�������� ����������")]
    [Tooltip("���������� ID ������. ������������ ��� ���������� � �������������.")]
    public string skillID;

    [Tooltip("�������� ������, ������� ����� �����.")]
    public string skillName;

    [Tooltip("��������� �������� ������ ��� UI.")]
    [TextArea(3, 5)]
    public string description;

    [Tooltip("������ ������.")]
    public Sprite icon;

    [Header("������� ������ � �����")]
    [Tooltip("����, ������������ ��� ������. ��������� ��������� ������ ������ �� ����.")]
    public SkillTag tags;

    [Tooltip("������, ���������� ������ ������ (������, ����������� �� ActiveSkill).")]
    public GameObject skillLogicPrefab;

    [Header("������� ��������������")]
    [Tooltip("������� ���� ������.")]
    public float baseDamage = 10f;

    [Tooltip("������� ����������� � ��������.")]
    public float baseCooldown = 5f;

    [Tooltip("������� ������/������� ��������.")]
    public float baseAreaOfEffect = 1f;

    [Tooltip("������� ���������� ��������/������ �� ���.")]
    public int baseAmount = 1;

    [Tooltip("������� �������� ������ ��� ��������.")]
    public float baseProjectileSpeed = 10f;

    [Tooltip("������� ������������ �������� (��������, ��� ��� ��� DoT).")]
    public float baseDuration = 3f;

    [Header("������� �������")]
    [Tooltip("������ �� ScriptableObject ���������� ������ ����� ������. �������� ������ ��� ������������� ������.")]
    public ActiveSkillData nextLevelSkill;

    [Tooltip("������ �� ScriptableObject ������������� ������ ������. �������� ������, ���� �� ���.")]
    public ActiveSkillData ultimateVersionSkill;
}