using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerController;

/// <summary>
/// ������� "�������" ��� ���� �������� ������ ������.
/// ��������� �� ��������� ������: �����������, ������������, ���������� � �����������.
/// </summary>
public class ActiveSkillManager : MonoBehaviour
{
    // ���������� �����-������� ��� �������� ���������� ������ �������
    private class ActiveSkillInstance
    {
        public ActiveSkill skillLogic; // ������ �� ��������� � ������� ������
        public float cooldownTimer;    // ������������ ������ �����������
    }

    [Header("������")]
    [Tooltip("������ �� ��������� PlayerStats. ���� �� �������, ����� �������� �� ���� �� �������.")]
    [SerializeField] private PlayerStats playerStats;
    

    // "�������" ������ - ������ ���� �������� ������, ������� � ���� ����
    private readonly List<ActiveSkillInstance> _activeSkills = new List<ActiveSkillInstance>();

    private void Awake()
    {
        

        // ������������� ������� PlayerStats, ���� �� �� ������ � ����������
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
    }

    private void OnEnable()
    {
        // ������������� �� ������� ��������� ������ ������.
        // ��� �������� ��� ����������� ��������� �������������� ������.
        if (playerStats != null)
        {
            playerStats.OnStatChanged += HandleStatChanged;
        }
    }

    private void OnDisable()
    {
        // ����������� ������������, ����� �������� ������ ������ � ������.
        if (playerStats != null)
        {
            playerStats.OnStatChanged -= HandleStatChanged;
        }
    }

    private void Update()
    {
        // �������� �� ������� ������ � ����� ��������
        foreach (var instance in _activeSkills)
        {
            // ��������� ��� ������ �����������
            instance.cooldownTimer -= Time.deltaTime;

            // ���� ������ ����� �� ���� (��� ����)
            if (instance.cooldownTimer <= 0)
            {
                // 1. ���������� ������ ������ (��������� ������, ������� ���� � �.�.)
                instance.skillLogic.Activate();

                // 2. ��������� ��� ���� � ���, ��� ������ ���� ������������.
                // ��� ����� ��� ������ ����� "������ �������".
                GameEvents.ReportPlayerAbilityUsed(instance.skillLogic.skillData);

                // 3. ���������� ������ �� ������� �������� ����������� ������
                // �� ���������� �������� .currentCooldown �� ������ ������, ������� ��� ����������
                instance.cooldownTimer += instance.skillLogic.currentCooldown;
            }
        }
    }




    /// <summary>
    /// ��������� ����� ������ � ������� ������ ��� �������� ������������.
    /// </summary>
    public void AddSkill(ActiveSkillData skillToAdd)
    {
        // --- ������ ��������� ---
        // ����, �� �������� �� ����� ������ ���������� ��� ��� �������������.
        ActiveSkillInstance skillToUpgrade = _activeSkills.FirstOrDefault(s =>
            (s.skillLogic.skillData.nextLevelSkill != null && s.skillLogic.skillData.nextLevelSkill == skillToAdd) ||
            (s.skillLogic.skillData.ultimateVersionSkill != null && s.skillLogic.skillData.ultimateVersionSkill == skillToAdd)
        );

        // ���� ����� ������ ��� ���������...
        if (skillToUpgrade != null)
        {
            // ...������� ��� �� ������ ������ � ���������� ��� ������� ������.
            _activeSkills.Remove(skillToUpgrade);
            Destroy(skillToUpgrade.skillLogic.gameObject);
            Debug.Log($"�������� ������: {skillToUpgrade.skillLogic.skillData.skillName} -> {skillToAdd.skillName}");
        }
        else
        {
            Debug.Log($"��������� ����� ������: {skillToAdd.skillName}");
        }

        // --- ������ ���������� ---
        // 1. ������� ��������� ������� � ������� ������.
        // ������ ��� �������� � ����� ��������� ��� ������� � ��������.
        GameObject skillObject = Instantiate(skillToAdd.skillLogicPrefab, transform);
        ActiveSkill newSkill = skillObject.GetComponent<ActiveSkill>();

        // 2. �������������� ������, ��������� ��� ������ � ������ �� ����� ������.
        newSkill.Initialize(skillToAdd, playerStats);

        // 3. ������� ����� ���������-������� � ��������� ��� � ��� �������.
        _activeSkills.Add(new ActiveSkillInstance
        {
            skillLogic = newSkill,
            cooldownTimer = newSkill.currentCooldown // �������� � ������� ��������
        });
    }

    /// <summary>
    /// �����, ���������� �������� OnStatChanged �� PlayerStats.
    /// </summary>
    private void HandleStatChanged(StatType type, float value)
    {
        RecalculateAllSkillStats();
    }

    /// <summary>
    /// ���������� ��� �������� ������ � �������� ����������� ���� ��������������.
    /// </summary>
    private void RecalculateAllSkillStats()
    {
        Debug.Log("����� ������ ����������. ������������� �������������� ���� �������� ������.");
        foreach (var instance in _activeSkills)
        {
            instance.skillLogic.RecalculateStats();
        }
    }

    public void ForceRecalculateAllSkills()
    {
        RecalculateAllSkillStats();
    }

}