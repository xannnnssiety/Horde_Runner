using UnityEngine;
using System.Linq; // Убедитесь, что этот using есть

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

    // Хранилище текущего прогресса
    public SaveData CurrentSaveData { get; private set; }

    void Awake()
    {   
        
        // Загружаем прогресс в Awake, чтобы он был доступен другим скриптам в их Start()
        LoadProgress();
    }

    public void LoadProgress()
    {
        
        CurrentSaveData = SaveManager.LoadGame();
        CurrentSaveData.currency = 100000; // Временно устанавливаем огромное количество валюты
        ApplyAllLoadedPassives();
    }

    public void SaveProgress()
    {
        SaveManager.SaveGame(CurrentSaveData);
    }

    private void ApplyAllLoadedPassives()
    {
        if (playerStats == null || passiveSkillTree == null) return;

        // Проходим по словарю изученных навыков
        foreach (var unlockedPassive in CurrentSaveData.unlockedPassives)
        {
            string skillID = unlockedPassive.Key;
            int purchaseCount = unlockedPassive.Value;

            PassiveSkillData skillToApply = passiveSkillTree.allSkills.Find(s => s.skillID == skillID);

            if (skillToApply != null)
            {
                // Применяем модификаторы столько раз, сколько был куплен навык
                for (int i = 0; i < purchaseCount; i++)
                {
                    playerStats.ApplyPassive(skillToApply);
                }

                // Уникальное поведение активируем только один раз
                if (purchaseCount > 0 && skillToApply.uniqueBehaviourPrefab != null)
                {
                    Instantiate(skillToApply.uniqueBehaviourPrefab, playerStats.transform);
                }
            }
        }
    }

    /// <summary>
    /// Логика покупки или улучшения пассивного навыка.
    /// </summary>
    public void UnlockPassive(PassiveSkillData newSkill)
    {
        // Получаем текущий уровень прокачки навыка
        CurrentSaveData.unlockedPassives.TryGetValue(newSkill.skillID, out int currentLevel);

        // ПРОВЕРКА 1: Не достигнут ли максимальный уровень?
        if (currentLevel >= newSkill.maxPurchaseCount)
        {
            Debug.Log($"Навык '{newSkill.skillName}' уже прокачан до максимума.");
            return;
        }

        // ПРОВЕРКА 2: Хватает ли валюты?
        int currentCost = GetCurrentSkillCost(newSkill);
        if (CurrentSaveData.currency < currentCost)
        {
            Debug.Log($"Недостаточно валюты для '{newSkill.skillName}'. Нужно: {currentCost}, есть: {CurrentSaveData.currency}");
            return;
        }

        // --- ВСЕ ПРОВЕРКИ ПРОЙДЕНЫ, СОВЕРШАЕМ ПОКУПКУ ---
        Debug.Log($"<color=green>Улучшен навык: {newSkill.skillName} до уровня {currentLevel + 1}</color>");

        

        // 1. Списываем валюту
        CurrentSaveData.currency -= currentCost;

        // 2. ДОБАВЛЯЕМ ПОТРАЧЕННУЮ СУММУ В ОБЩИЙ КОТЕЛ
        CurrentSaveData.totalCurrencySpent += currentCost;

        // 2. Увеличиваем общий счетчик покупок для инфляции
        CurrentSaveData.totalPurchasesMade++;

        // 3. Обновляем уровень навыка в словаре
        if (currentLevel > 0)
        {
            CurrentSaveData.unlockedPassives[newSkill.skillID]++;
        }
        else // Если это первая покупка, добавляем запись в словарь
        {
            CurrentSaveData.unlockedPassives.Add(newSkill.skillID, 1);
            // Активируем уникальное поведение только при первой покупке
            if (newSkill.uniqueBehaviourPrefab != null)
            {
                Instantiate(newSkill.uniqueBehaviourPrefab, playerStats.transform);
            }
        }

        // 4. Применяем эффекты к статам персонажа
        playerStats.ApplyPassive(newSkill);

        // 5. Сохраняем игру
        SaveProgress();
    }

    /// <summary>
    /// Вычисляет текущую стоимость навыка с учетом инфляции.
    /// Этот метод будет использоваться UI для отображения цены.
    /// </summary>
    public int GetCurrentSkillCost(PassiveSkillData skill)
    {
        float costMultiplier = Mathf.Pow(1f + priceInflationRate, CurrentSaveData.totalPurchasesMade);
        int currentCost = Mathf.CeilToInt(skill.baseCost * costMultiplier);
        return currentCost;
    }

    // Отладочная функция сброса
    public void ResetAllProgress()
    {
        Debug.LogWarning("--- ПРОГРЕСС ПОЛНОСТЬЮ СБРОШЕН! ---");
        SaveManager.SaveGame(new SaveData());
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void RefundAllPassives()
    {
        if (CurrentSaveData == null || passiveSkillTree == null) return;

        Debug.LogWarning("--- СБРОС ВСЕХ ПАССИВНЫХ НАВЫКОВ ---");

        int totalRefundAmount = CurrentSaveData.totalCurrencySpent;
        



        // Возвращаем валюту
        CurrentSaveData.currency += totalRefundAmount;
        Debug.Log($"Возвращено {totalRefundAmount} валюты. Текущий баланс: {CurrentSaveData.currency}");

        // Очищаем данные
        CurrentSaveData.unlockedPassives.Clear();
        CurrentSaveData.totalPurchasesMade = 0;
        CurrentSaveData.totalCurrencySpent = 0;

        // Сбрасываем статы персонажа до базовых
        if (playerStats != null)
        {
            playerStats.ResetToDefaults();
        }

        // Обновляем UI магазина, чтобы он показал сброшенный прогресс
        if (shopUIManager != null)
        {
            shopUIManager.UpdateAllPerkVisuals();
        }
        else
        {
            Debug.LogWarning("Ссылка на Shop UI Manager не установлена в GameManager, UI не будет обновлен после сброса.");
        }

        // Сохраняем "чистые" данные
        SaveProgress();
    }
}
