using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Ссылки на ключевые компоненты
    public PlayerStats playerStats; // Перетащите сюда объект StatsAndSkills
    public PassiveSkillTree passiveSkillTree; // Перетащите сюда ассет MainPassiveTree

    // Хранилище текущего прогресса
    private SaveData _saveData;

    void Start()
    {
        LoadProgress();
    }

    private void Update()
    {
        // Отладочная функция: сброс прогресса по нажатию на R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllProgress();
        }
    }

    public void LoadProgress()
    {
        // Загружаем данные из файла
        _saveData = SaveManager.LoadGame();
        _saveData.currency = 100; // Временно устанавливаем валюту для теста
        // Применяем загруженный прогресс
        ApplyLoadedPassives();

        // TODO: Обновить UI с количеством валюты
        // UIManager.UpdateCurrency(_saveData.currency);
    }

    public void SaveProgress()
    {
        // Здесь мы могли бы обновить данные перед сохранением,
        // например, _saveData.currency = player.Currency;

        SaveManager.SaveGame(_saveData);
    }

    private void ApplyLoadedPassives()
    {
        if (playerStats == null || passiveSkillTree == null)
        {
            Debug.LogError("Ссылки на PlayerStats или PassiveSkillTree не установлены в GameManager!");
            return;
        }

        // Проходим по всем ID изученных навыков из нашего сохранения
        foreach (string skillID in _saveData.unlockedPassiveIDs)
        {
            // Ищем соответствующий навык в нашей "базе данных"
            PassiveSkillData skillToApply = passiveSkillTree.allSkills.Find(s => s.skillID == skillID);

            if (skillToApply != null)
            {
                // Если навык найден, применяем его эффекты к статам игрока
                playerStats.ApplyPassive(skillToApply);
            }
            else
            {
                Debug.LogWarning($"Пассивный навык с ID '{skillID}' не найден в дереве!");
            }
        }
    }

    // Этот метод будет вызываться из UI, когда игрок покупает новый навык
    public void UnlockPassive(PassiveSkillData newSkill)
    {
        // --- ПРОВЕРКА 1: Не изучен ли навык уже? ---
        if (_saveData.unlockedPassiveIDs.Contains(newSkill.skillID))
        {
            Debug.Log($"Навык '{newSkill.skillName}' уже изучен.");
            return;
        }

        // --- ПРОВЕРКА 2: Хватает ли валюты? ---
        if (_saveData.currency < newSkill.cost)
        {
            Debug.Log($"Недостаточно валюты для изучения '{newSkill.skillName}'. Нужно: {newSkill.cost}, есть: {_saveData.currency}");
            // TODO: Показать игроку сообщение об ошибке
            return;
        }

        // --- ПРОВЕРКА 3: Изучены ли все предыдущие навыки? ---
        foreach (var prerequisite in newSkill.prerequisites)
        {
            if (!_saveData.unlockedPassiveIDs.Contains(prerequisite.skillID))
            {
                Debug.Log($"Не выполнены требования для '{newSkill.skillName}'. Нужно изучить: '{prerequisite.skillName}'");
                // TODO: Показать игроку сообщение об ошибке
                return;
            }
        }

        // --- ВСЕ ПРОВЕРКИ ПРОЙДЕНЫ, СОВЕРШАЕМ ПОКУПКУ ---
        Debug.Log($"<color=green>Изучен новый навык: {newSkill.skillName}</color>");

        // 1. Списываем валюту
        _saveData.currency -= newSkill.cost;

        // 2. Добавляем ID в список изученных
        _saveData.unlockedPassiveIDs.Add(newSkill.skillID);

        // 3. Применяем эффекты к статам персонажа
        playerStats.ApplyPassive(newSkill);

        // 4. Активируем уникальное поведение, если оно есть
        if (newSkill.uniqueBehaviourPrefab != null)
        {
            // Создаем экземпляр префаба и делаем его дочерним к объекту со статами
            Instantiate(newSkill.uniqueBehaviourPrefab, playerStats.transform);
        }

        // 5. Сохраняем игру после каждого важного изменения
        SaveProgress();

        // 6. UI обновится автоматически, так как его вызывает PassiveTree_UI_Manager
    }

    public void ResetAllProgress()
    {
        Debug.LogWarning("--- ПРОГРЕСС ПОЛНОСТЬЮ СБРОШЕН! ---");

        // 1. Создаем абсолютно новый, пустой объект SaveData
        SaveData freshSaveData = new SaveData();

        // 2. Сохраняем эти пустые данные в файл, затирая старое сохранение
        SaveManager.SaveGame(freshSaveData);

        // 3. Перезагружаем текущую сцену, чтобы все изменения применились.
        // Это самый надежный способ убедиться, что все системы (PlayerStats, UI)
        // начнут работать с чистого листа.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}