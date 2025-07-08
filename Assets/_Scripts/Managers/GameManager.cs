using UnityEngine;
using System.Linq;
using System.Collections.Generic; // --- ��������� --- ��������� ��� ������������� List<>

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

    public SaveData CurrentSaveData { get; private set; }

    // --- ��������� ---
    // ������ ��� ������������ ���� ��������� �������� � ���������� ����������.
    private List<GameObject> _activeUniqueBehaviours = new List<GameObject>();

    void Awake()
    {
        LoadProgress();
    }

    // --- ��������� ---
    // ��������� ������ �������� �������.
    private void OnEnable()
    {
        GameEvents.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= HandleEnemyDied;
    }

    public void LoadProgress()
    {
        CurrentSaveData = SaveManager.LoadGame();
        CurrentSaveData.currency = 100000;
        ApplyAllLoadedPassives();
    }

    public void SaveProgress()
    {
        SaveManager.SaveGame(CurrentSaveData);
    }

    private void ApplyAllLoadedPassives()
    {
        if (playerStats == null || passiveSkillTree == null) return;

        // --- ��������� ---
        // ����� ����������� ���� ������ (��������, ��� �������� ����)
        // ���������� ��� ������ �������, ����� �������� �� ������������.
        DestroyAllUniqueBehaviours();

        foreach (var unlockedPassive in CurrentSaveData.unlockedPassives)
        {
            string skillID = unlockedPassive.Key;
            int purchaseCount = unlockedPassive.Value;
            PassiveSkillData skillToApply = passiveSkillTree.allSkills.Find(s => s.skillID == skillID);

            if (skillToApply != null)
            {
                for (int i = 0; i < purchaseCount; i++)
                {
                    playerStats.ApplyPassive(skillToApply);
                }

                if (purchaseCount > 0 && skillToApply.uniqueBehaviourPrefab != null)
                {
                    // --- ��������� ---
                    // ������� ������ � ����� �� ��������� ��� � ������ ��� ������������.
                    GameObject instance = Instantiate(skillToApply.uniqueBehaviourPrefab, playerStats.transform);
                    _activeUniqueBehaviours.Add(instance);
                }
            }
        }
    }

    public void UnlockPassive(PassiveSkillData newSkill)
    {
        CurrentSaveData.unlockedPassives.TryGetValue(newSkill.skillID, out int currentLevel);

        if (currentLevel >= newSkill.maxPurchaseCount)
        {
            Debug.Log($"����� '{newSkill.skillName}' ��� �������� �� ���������.");
            return;
        }

        int currentCost = GetCurrentSkillCost(newSkill);
        if (CurrentSaveData.currency < currentCost)
        {
            Debug.Log($"������������ ������ ��� '{newSkill.skillName}'. �����: {currentCost}, ����: {CurrentSaveData.currency}");
            return;
        }

        Debug.Log($"<color=green>������� �����: {newSkill.skillName} �� ������ {currentLevel + 1}</color>");

        CurrentSaveData.currency -= currentCost;
        CurrentSaveData.totalCurrencySpent += currentCost;
        CurrentSaveData.totalPurchasesMade++;

        if (currentLevel > 0)
        {
            CurrentSaveData.unlockedPassives[newSkill.skillID]++;
        }
        else
        {
            CurrentSaveData.unlockedPassives.Add(newSkill.skillID, 1);
            if (newSkill.uniqueBehaviourPrefab != null)
            {
                // --- ��������� ---
                // ����� ��� ��, ��� ������ �������, ������� � ��������� � ������.
                GameObject instance = Instantiate(newSkill.uniqueBehaviourPrefab, playerStats.transform);
                _activeUniqueBehaviours.Add(instance);
            }
        }

        playerStats.ApplyPassive(newSkill);
        SaveProgress();
    }

    public int GetCurrentSkillCost(PassiveSkillData skill)
    {
        float costMultiplier = Mathf.Pow(1f + priceInflationRate, CurrentSaveData.totalPurchasesMade);
        int currentCost = Mathf.CeilToInt(skill.baseCost * costMultiplier);
        return currentCost;
    }

    public void ResetAllProgress()
    {
        Debug.LogWarning("--- �������� ��������� �������! ---");
        // --- ��������� ---
        // ��� ������ ������ ����� ���������� ���������� ��� �������.
        DestroyAllUniqueBehaviours();
        SaveManager.SaveGame(new SaveData());
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void RefundAllPassives()
    {
        if (CurrentSaveData == null || passiveSkillTree == null) return;

        Debug.LogWarning("--- ����� ���� ��������� ������� ---");

        int totalRefundAmount = CurrentSaveData.totalCurrencySpent;

        CurrentSaveData.currency += totalRefundAmount;
        Debug.Log($"���������� {totalRefundAmount} ������. ������� ������: {CurrentSaveData.currency}");

        CurrentSaveData.unlockedPassives.Clear();
        CurrentSaveData.totalPurchasesMade = 0;
        CurrentSaveData.totalCurrencySpent = 0;

        if (playerStats != null)
        {
            playerStats.ResetToDefaults();
        }

        // --- ��������� ---
        // ��� �������� �����������.
        // ����� ������ ������ �� ���������� ��� ������� ���������� ������.
        // ��� ������� �� ��� OnDisable(), ������� �� �� ������� � ������ �� �������.
        DestroyAllUniqueBehaviours();

        if (shopUIManager != null)
        {
            shopUIManager.UpdateAllPerkVisuals();
        }
        else
        {
            Debug.LogWarning("������ �� Shop UI Manager �� ����������� � GameManager, UI �� ����� �������� ����� ������.");
        }

        SaveProgress();
    }

    private void HandleEnemyDied()
    {
        if (CurrentSaveData == null) return;
        CurrentSaveData.totalKills++;
        GameEvents.ReportKillCountChanged(CurrentSaveData.totalKills);
        SaveProgress();
    }

    // --- ��������� ---
    // ����� �����, ������� ��������������� ���������� ��� ������������� �������.
    private void DestroyAllUniqueBehaviours()
    {
        foreach (GameObject behaviour in _activeUniqueBehaviours)
        {
            if (behaviour != null)
            {
                Destroy(behaviour);
            }
        }
        _activeUniqueBehaviours.Clear();
        Debug.Log("��� �������� ���������� ��������� ���� ����������.");
    }
}