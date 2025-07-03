using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ������ �� �������� ����������
    public PlayerStats playerStats; // ���������� ���� ������ StatsAndSkills
    public PassiveSkillTree passiveSkillTree; // ���������� ���� ����� MainPassiveTree

    // ��������� �������� ���������
    private SaveData _saveData;

    void Start()
    {
        LoadProgress();
    }

    private void Update()
    {
        // ���������� �������: ����� ��������� �� ������� �� R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllProgress();
        }
    }

    public void LoadProgress()
    {
        // ��������� ������ �� �����
        _saveData = SaveManager.LoadGame();
        _saveData.currency = 100; // �������� ������������� ������ ��� �����
        // ��������� ����������� ��������
        ApplyLoadedPassives();

        // TODO: �������� UI � ����������� ������
        // UIManager.UpdateCurrency(_saveData.currency);
    }

    public void SaveProgress()
    {
        // ����� �� ����� �� �������� ������ ����� �����������,
        // ��������, _saveData.currency = player.Currency;

        SaveManager.SaveGame(_saveData);
    }

    private void ApplyLoadedPassives()
    {
        if (playerStats == null || passiveSkillTree == null)
        {
            Debug.LogError("������ �� PlayerStats ��� PassiveSkillTree �� ����������� � GameManager!");
            return;
        }

        // �������� �� ���� ID ��������� ������� �� ������ ����������
        foreach (string skillID in _saveData.unlockedPassiveIDs)
        {
            // ���� ��������������� ����� � ����� "���� ������"
            PassiveSkillData skillToApply = passiveSkillTree.allSkills.Find(s => s.skillID == skillID);

            if (skillToApply != null)
            {
                // ���� ����� ������, ��������� ��� ������� � ������ ������
                playerStats.ApplyPassive(skillToApply);
            }
            else
            {
                Debug.LogWarning($"��������� ����� � ID '{skillID}' �� ������ � ������!");
            }
        }
    }

    // ���� ����� ����� ���������� �� UI, ����� ����� �������� ����� �����
    public void UnlockPassive(PassiveSkillData newSkill)
    {
        // --- �������� 1: �� ������ �� ����� ���? ---
        if (_saveData.unlockedPassiveIDs.Contains(newSkill.skillID))
        {
            Debug.Log($"����� '{newSkill.skillName}' ��� ������.");
            return;
        }

        // --- �������� 2: ������� �� ������? ---
        if (_saveData.currency < newSkill.cost)
        {
            Debug.Log($"������������ ������ ��� �������� '{newSkill.skillName}'. �����: {newSkill.cost}, ����: {_saveData.currency}");
            // TODO: �������� ������ ��������� �� ������
            return;
        }

        // --- �������� 3: ������� �� ��� ���������� ������? ---
        foreach (var prerequisite in newSkill.prerequisites)
        {
            if (!_saveData.unlockedPassiveIDs.Contains(prerequisite.skillID))
            {
                Debug.Log($"�� ��������� ���������� ��� '{newSkill.skillName}'. ����� �������: '{prerequisite.skillName}'");
                // TODO: �������� ������ ��������� �� ������
                return;
            }
        }

        // --- ��� �������� ��������, ��������� ������� ---
        Debug.Log($"<color=green>������ ����� �����: {newSkill.skillName}</color>");

        // 1. ��������� ������
        _saveData.currency -= newSkill.cost;

        // 2. ��������� ID � ������ ���������
        _saveData.unlockedPassiveIDs.Add(newSkill.skillID);

        // 3. ��������� ������� � ������ ���������
        playerStats.ApplyPassive(newSkill);

        // 4. ���������� ���������� ���������, ���� ��� ����
        if (newSkill.uniqueBehaviourPrefab != null)
        {
            // ������� ��������� ������� � ������ ��� �������� � ������� �� �������
            Instantiate(newSkill.uniqueBehaviourPrefab, playerStats.transform);
        }

        // 5. ��������� ���� ����� ������� ������� ���������
        SaveProgress();

        // 6. UI ��������� �������������, ��� ��� ��� �������� PassiveTree_UI_Manager
    }

    public void ResetAllProgress()
    {
        Debug.LogWarning("--- �������� ��������� �������! ---");

        // 1. ������� ��������� �����, ������ ������ SaveData
        SaveData freshSaveData = new SaveData();

        // 2. ��������� ��� ������ ������ � ����, ������� ������ ����������
        SaveManager.SaveGame(freshSaveData);

        // 3. ������������� ������� �����, ����� ��� ��������� �����������.
        // ��� ����� �������� ������ ���������, ��� ��� ������� (PlayerStats, UI)
        // ������ �������� � ������� �����.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}