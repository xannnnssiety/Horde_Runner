using UnityEngine;
using System.Linq; // ���������, ��� ���� using ����

public class GameManager : MonoBehaviour
{
    [Header("������")]
    public PassiveShop_UI_Manager shopUIManager;
    [Tooltip("������ �� ��������� PlayerStats. ������ ���� �� �����.")]
    public PlayerStats playerStats;
    [Tooltip("������ �� �����, �������� ��� ��������� ������.")]
    public PassiveSkillTree passiveSkillTree;

    [Header("��������� ���������")]
    [Tooltip("�������, �� ������� ������������� ������� ���� ����� ������ �������. 0.1 = 10%")]
    [Range(0f, 1f)]
    public float priceInflationRate = 0.1f;

    // ��������� �������� ���������
    public SaveData CurrentSaveData { get; private set; }

    void Awake()
    {   
        
        // ��������� �������� � Awake, ����� �� ��� �������� ������ �������� � �� Start()
        LoadProgress();
    }

    public void LoadProgress()
    {
        
        CurrentSaveData = SaveManager.LoadGame();
        CurrentSaveData.currency = 100000; // �������� ������������� �������� ���������� ������
        ApplyAllLoadedPassives();
    }

    public void SaveProgress()
    {
        SaveManager.SaveGame(CurrentSaveData);
    }

    private void ApplyAllLoadedPassives()
    {
        if (playerStats == null || passiveSkillTree == null) return;

        // �������� �� ������� ��������� �������
        foreach (var unlockedPassive in CurrentSaveData.unlockedPassives)
        {
            string skillID = unlockedPassive.Key;
            int purchaseCount = unlockedPassive.Value;

            PassiveSkillData skillToApply = passiveSkillTree.allSkills.Find(s => s.skillID == skillID);

            if (skillToApply != null)
            {
                // ��������� ������������ ������� ���, ������� ��� ������ �����
                for (int i = 0; i < purchaseCount; i++)
                {
                    playerStats.ApplyPassive(skillToApply);
                }

                // ���������� ��������� ���������� ������ ���� ���
                if (purchaseCount > 0 && skillToApply.uniqueBehaviourPrefab != null)
                {
                    Instantiate(skillToApply.uniqueBehaviourPrefab, playerStats.transform);
                }
            }
        }
    }

    /// <summary>
    /// ������ ������� ��� ��������� ���������� ������.
    /// </summary>
    public void UnlockPassive(PassiveSkillData newSkill)
    {
        // �������� ������� ������� �������� ������
        CurrentSaveData.unlockedPassives.TryGetValue(newSkill.skillID, out int currentLevel);

        // �������� 1: �� ��������� �� ������������ �������?
        if (currentLevel >= newSkill.maxPurchaseCount)
        {
            Debug.Log($"����� '{newSkill.skillName}' ��� �������� �� ���������.");
            return;
        }

        // �������� 2: ������� �� ������?
        int currentCost = GetCurrentSkillCost(newSkill);
        if (CurrentSaveData.currency < currentCost)
        {
            Debug.Log($"������������ ������ ��� '{newSkill.skillName}'. �����: {currentCost}, ����: {CurrentSaveData.currency}");
            return;
        }

        // --- ��� �������� ��������, ��������� ������� ---
        Debug.Log($"<color=green>������� �����: {newSkill.skillName} �� ������ {currentLevel + 1}</color>");

        

        // 1. ��������� ������
        CurrentSaveData.currency -= currentCost;

        // 2. ��������� ����������� ����� � ����� �����
        CurrentSaveData.totalCurrencySpent += currentCost;

        // 2. ����������� ����� ������� ������� ��� ��������
        CurrentSaveData.totalPurchasesMade++;

        // 3. ��������� ������� ������ � �������
        if (currentLevel > 0)
        {
            CurrentSaveData.unlockedPassives[newSkill.skillID]++;
        }
        else // ���� ��� ������ �������, ��������� ������ � �������
        {
            CurrentSaveData.unlockedPassives.Add(newSkill.skillID, 1);
            // ���������� ���������� ��������� ������ ��� ������ �������
            if (newSkill.uniqueBehaviourPrefab != null)
            {
                Instantiate(newSkill.uniqueBehaviourPrefab, playerStats.transform);
            }
        }

        // 4. ��������� ������� � ������ ���������
        playerStats.ApplyPassive(newSkill);

        // 5. ��������� ����
        SaveProgress();
    }

    /// <summary>
    /// ��������� ������� ��������� ������ � ������ ��������.
    /// ���� ����� ����� �������������� UI ��� ����������� ����.
    /// </summary>
    public int GetCurrentSkillCost(PassiveSkillData skill)
    {
        float costMultiplier = Mathf.Pow(1f + priceInflationRate, CurrentSaveData.totalPurchasesMade);
        int currentCost = Mathf.CeilToInt(skill.baseCost * costMultiplier);
        return currentCost;
    }

    // ���������� ������� ������
    public void ResetAllProgress()
    {
        Debug.LogWarning("--- �������� ��������� �������! ---");
        SaveManager.SaveGame(new SaveData());
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void RefundAllPassives()
    {
        if (CurrentSaveData == null || passiveSkillTree == null) return;

        Debug.LogWarning("--- ����� ���� ��������� ������� ---");

        int totalRefundAmount = CurrentSaveData.totalCurrencySpent;
        



        // ���������� ������
        CurrentSaveData.currency += totalRefundAmount;
        Debug.Log($"���������� {totalRefundAmount} ������. ������� ������: {CurrentSaveData.currency}");

        // ������� ������
        CurrentSaveData.unlockedPassives.Clear();
        CurrentSaveData.totalPurchasesMade = 0;
        CurrentSaveData.totalCurrencySpent = 0;

        // ���������� ����� ��������� �� �������
        if (playerStats != null)
        {
            playerStats.ResetToDefaults();
        }

        // ��������� UI ��������, ����� �� ������� ���������� ��������
        if (shopUIManager != null)
        {
            shopUIManager.UpdateAllPerkVisuals();
        }
        else
        {
            Debug.LogWarning("������ �� Shop UI Manager �� ����������� � GameManager, UI �� ����� �������� ����� ������.");
        }

        // ��������� "������" ������
        SaveProgress();
    }
}
