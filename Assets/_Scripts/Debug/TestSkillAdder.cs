using UnityEngine;
using System.Collections.Generic; // Необходимо для использования List<>

/// <summary>
/// Тестовый скрипт для добавления активных умений по нажатию клавиш.
/// </summary>
public class TestSkillAdder : MonoBehaviour
{
    // Внутренний класс для удобной настройки в инспекторе
    [System.Serializable]
    public class SkillTestEntry
    {
        [Tooltip("Просто для заметки в инспекторе, на логику не влияет.")]
        public string description;
        [Tooltip("Клавиша, по нажатию на которую будет добавлено умение.")]
        public KeyCode triggerKey;
        [Tooltip("Ассет умения, который нужно добавить.")]
        public ActiveSkillData skillToAdd;
    }

    [Header("Ссылки")]
    [Tooltip("Ссылка на менеджер активных умений на игроке. Перетащите сюда объект с этим компонентом.")]
    public ActiveSkillManager skillManager;

    [Header("Настройки теста")]
    [Tooltip("Список умений для тестирования по нажатию клавиш.")]
    public List<SkillTestEntry> skillsToTest;

    void Update()
    {
        // Проверяем, назначен ли менеджер, чтобы избежать ошибок
        if (skillManager == null)
        {
            return;
        }

        // Проходим по каждому нашему тестовому набору
        foreach (var entry in skillsToTest)
        {
            // Если была нажата клавиша, указанная в наборе
            if (Input.GetKeyDown(entry.triggerKey))
            {
                // И если в наборе указано умение для добавления
                if (entry.skillToAdd != null)
                {
                    Debug.Log($"Нажата клавиша {entry.triggerKey}. Добавляем умение: {entry.skillToAdd.skillName}");
                    // Вызываем главный метод нашего менеджера
                    skillManager.AddSkill(entry.skillToAdd);
                }
            }
        }
    }
}