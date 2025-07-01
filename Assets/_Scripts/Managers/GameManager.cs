using UnityEngine;

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

    public void LoadProgress()
    {
        // Загружаем данные из файла
        _saveData = SaveManager.LoadGame();

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
        if (_saveData.currency >= newSkill.cost && !_saveData.unlockedPassiveIDs.Contains(newSkill.skillID))
        {
            _saveData.currency -= newSkill.cost;
            _saveData.unlockedPassiveIDs.Add(newSkill.skillID);

            playerStats.ApplyPassive(newSkill);

            // Сохраняем игру после каждого важного изменения
            SaveProgress();

            // TODO: Обновить UI дерева и валюты
        }
    }
}