using UnityEngine;

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

    public void LoadProgress()
    {
        // ��������� ������ �� �����
        _saveData = SaveManager.LoadGame();

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
        if (_saveData.currency >= newSkill.cost && !_saveData.unlockedPassiveIDs.Contains(newSkill.skillID))
        {
            _saveData.currency -= newSkill.cost;
            _saveData.unlockedPassiveIDs.Add(newSkill.skillID);

            playerStats.ApplyPassive(newSkill);

            // ��������� ���� ����� ������� ������� ���������
            SaveProgress();

            // TODO: �������� UI ������ � ������
        }
    }
}