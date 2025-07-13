using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ��������� ������� ��������� ������, ���������� ������ ������ � ����������� ������ ������.
/// </summary>
public class LevelUpManager : MonoBehaviour
{
    [Header("������ �� UI")]
    [Tooltip("������, ������� ���������� ��� ��������� ������.")]
    [SerializeField] private GameObject levelUpScreenPanel;
    [Tooltip("������ �������� ��� ������ ������.")]
    [SerializeField] private GameObject skillChoiceCardPrefab;
    [Tooltip("���������, � ������� ����� ���������� �������� ������.")]
    [SerializeField] private Transform cardContainer;

    [Header("������ �� ��������� � ���� ������")]
    [SerializeField] private ActiveSkillDatabase skillDatabase;
    [SerializeField] private ActiveSkillManager activeSkillManager;
    [SerializeField] private PlayerStats playerStats;

    private void Start()
    {
        // ��������, ��� ����� �������� ��� ������ ����
        levelUpScreenPanel.SetActive(false);
    }

    /// <summary>
    /// ������� �����, ������� ���������� ����� ������ ������.
    /// </summary>
    public void ShowSelectionScreen()
    {
        Time.timeScale = 0f; // ������ ���� �� �����
        levelUpScreenPanel.SetActive(true);
        GenerateChoices();

        Cursor.visible = true; // ���������� ������
        Cursor.lockState = CursorLockMode.None; // ���������� ��� �� ������
    }

    /// <summary>
    /// ����������, ����� ����� �������� �� ���� �� �������� ������.
    /// </summary>
    public void OnSkillSelected(ActiveSkillData chosenSkill)
    {
        activeSkillManager.AddSkill(chosenSkill); // ���������/�������� ��������� �����
        levelUpScreenPanel.SetActive(false); // ������ ������
        Time.timeScale = 1f; // ������� ���� � �����

        Cursor.visible = false; // ������ ������
        Cursor.lockState = CursorLockMode.Locked; // ���������� ��� � ������
    }

    private void GenerateChoices()
    {
        // 1. ������� ������ ��������, ���� ��� ��������
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. �������� ��� ���� ��������� ���������
        List<ActiveSkillData> possibleChoices = new List<ActiveSkillData>();
        List<ActiveSkillData> currentSkills = activeSkillManager.GetCurrentlyEquippedSkills();

        // 2�. ��������� ��������� ��� ������� �������
        foreach (var skill in currentSkills)
        {
            if (skill.nextLevelSkill != null)
            {
                possibleChoices.Add(skill.nextLevelSkill);
            }
            // ����� � ������� ����� �������� ������ ��� �����������
        }

        // 2�. ��������� ����� ������, ������� ��� ��� � ������
        foreach (var newSkill in skillDatabase.allFirstLevelSkills)
        {
            // ���������, ���� �� � ������ ��� �����-���� ������ ����� ������
            bool alreadyHasSkill = currentSkills.Any(s => s.skillID.StartsWith(newSkill.skillID.Substring(0, newSkill.skillID.Length - 1)));
            if (!alreadyHasSkill)
            {
                possibleChoices.Add(newSkill);
            }
        }

        // 3. ����������, ������� ��������� ��������, �� ������ �����
        int choiceCount = 3;
        if (Random.Range(0f, 100f) < playerStats.GetStat(StatType.Luck))
        {
            choiceCount = 4;
        }

        // 4. �������� ��������� ���������� �������� �� ����
        List<ActiveSkillData> finalChoices = possibleChoices
            .OrderBy(x => Random.value) // ������������ ������
            .Take(Mathf.Min(choiceCount, possibleChoices.Count)) // ����� ������ ���������� (�� �� ������, ��� ���� � ����)
            .ToList();

        // 5. ������� � ����������� �������� ��� ������� ��������
        foreach (var choice in finalChoices)
        {
            GameObject cardObject = Instantiate(skillChoiceCardPrefab, cardContainer);
            SkillChoiceCard card = cardObject.GetComponent<SkillChoiceCard>();
            // �������� � �������� ������ � ������ � ������ �� ���� ��������
            card.Setup(choice, this, currentSkills);
        }
    }
}