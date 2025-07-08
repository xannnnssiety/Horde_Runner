using UnityEngine;
using System.Linq;
using System.Collections.Generic; // --- ИЗМЕНЕНИЕ --- Добавлено для использования List<>

public class GameManager : MonoBehaviour
{
    [Header("Ссылки")]
    public PassiveShop_UI_Manager shopUIManager;
    [Tooltip("Ссылка на компонент PlayerStats. Должен быть на сцене.")]
    public PlayerStats playerStats;
    [Tooltip("Ссылка на ассет, хранящий все пассивные навыки.")]
    public PassiveSkillTree passiveSkillTree;

    [Header("Настройки экономики")]
    [Tooltip("Процент, на который увеличивается базовая цена после каждой покупки. 0.1 = 10%")]
    [Range(0f, 1f)]
    public float priceInflationRate = 0.1f;

    public SaveData CurrentSaveData { get; private set; }

    // --- ИЗМЕНЕНИЕ ---
    // Список для отслеживания всех созданных префабов с уникальным поведением.
    private List<GameObject> _activeUniqueBehaviours = new List<GameObject>();

    void Awake()
    {
        LoadProgress();
    }

    // --- ИЗМЕНЕНИЕ ---
    // Добавлена логика подсчета убийств.
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

        // --- ИЗМЕНЕНИЕ ---
        // Перед применением всех перков (например, при загрузке игры)
        // уничтожаем все старые объекты, чтобы избежать их дублирования.
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
                    // --- ИЗМЕНЕНИЕ ---
                    // Создаем объект и СРАЗУ ЖЕ добавляем его в список для отслеживания.
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
            Debug.Log($"Навык '{newSkill.skillName}' уже прокачан до максимума.");
            return;
        }

        int currentCost = GetCurrentSkillCost(newSkill);
        if (CurrentSaveData.currency < currentCost)
        {
            Debug.Log($"Недостаточно валюты для '{newSkill.skillName}'. Нужно: {currentCost}, есть: {CurrentSaveData.currency}");
            return;
        }

        Debug.Log($"<color=green>Улучшен навык: {newSkill.skillName} до уровня {currentLevel + 1}</color>");

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
                // --- ИЗМЕНЕНИЕ ---
                // Точно так же, при первой покупке, создаем и добавляем в список.
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
        Debug.LogWarning("--- ПРОГРЕСС ПОЛНОСТЬЮ СБРОШЕН! ---");
        // --- ИЗМЕНЕНИЕ ---
        // При полном сбросе также необходимо уничтожить все объекты.
        DestroyAllUniqueBehaviours();
        SaveManager.SaveGame(new SaveData());
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void RefundAllPassives()
    {
        if (CurrentSaveData == null || passiveSkillTree == null) return;

        Debug.LogWarning("--- СБРОС ВСЕХ ПАССИВНЫХ НАВЫКОВ ---");

        int totalRefundAmount = CurrentSaveData.totalCurrencySpent;

        CurrentSaveData.currency += totalRefundAmount;
        Debug.Log($"Возвращено {totalRefundAmount} валюты. Текущий баланс: {CurrentSaveData.currency}");

        CurrentSaveData.unlockedPassives.Clear();
        CurrentSaveData.totalPurchasesMade = 0;
        CurrentSaveData.totalCurrencySpent = 0;

        if (playerStats != null)
        {
            playerStats.ResetToDefaults();
        }

        // --- ИЗМЕНЕНИЕ ---
        // ЭТО КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ.
        // После сброса статов мы уничтожаем все префабы уникальных перков.
        // Это вызовет на них OnDisable(), отпишет их от событий и уберет их эффекты.
        DestroyAllUniqueBehaviours();

        if (shopUIManager != null)
        {
            shopUIManager.UpdateAllPerkVisuals();
        }
        else
        {
            Debug.LogWarning("Ссылка на Shop UI Manager не установлена в GameManager, UI не будет обновлен после сброса.");
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

    // --- ИЗМЕНЕНИЕ ---
    // Новый метод, который централизованно уничтожает все отслеживаемые объекты.
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
        Debug.Log("Все активные уникальные поведения были уничтожены.");
    }
}