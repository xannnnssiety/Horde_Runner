using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет экраном повышения уровня, генерацией выбора умений и применением выбора игрока.
/// </summary>
public class LevelUpManager : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [Tooltip("Панель, которая появляется при повышении уровня.")]
    [SerializeField] private GameObject levelUpScreenPanel;
    [Tooltip("Префаб карточки для выбора умения.")]
    [SerializeField] private GameObject skillChoiceCardPrefab;
    [Tooltip("Контейнер, в который будут помещаться карточки выбора.")]
    [SerializeField] private Transform cardContainer;

    [Header("Ссылки на Менеджеры и Базы Данных")]
    [SerializeField] private ActiveSkillDatabase skillDatabase;
    [SerializeField] private ActiveSkillManager activeSkillManager;
    [SerializeField] private PlayerStats playerStats;

    private void Start()
    {
        // Убедимся, что экран выключен при старте игры
        levelUpScreenPanel.SetActive(false);
    }

    /// <summary>
    /// Главный метод, который показывает экран выбора умений.
    /// </summary>
    public void ShowSelectionScreen()
    {
        Time.timeScale = 0f; // Ставим игру на паузу
        levelUpScreenPanel.SetActive(true);
        GenerateChoices();

        Cursor.visible = true; // Показываем курсор
        Cursor.lockState = CursorLockMode.None; // Открепляем его от центра
    }

    /// <summary>
    /// Вызывается, когда игрок нажимает на одну из карточек выбора.
    /// </summary>
    public void OnSkillSelected(ActiveSkillData chosenSkill)
    {
        activeSkillManager.AddSkill(chosenSkill); // Добавляем/улучшаем выбранный скилл
        levelUpScreenPanel.SetActive(false); // Прячем панель
        Time.timeScale = 1f; // Снимаем игру с паузы

        Cursor.visible = false; // Прячем курсор
        Cursor.lockState = CursorLockMode.Locked; // Закрепляем его в центре
    }

    private void GenerateChoices()
    {
        // 1. Очищаем старые карточки, если они остались
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Собираем пул всех возможных вариантов
        List<ActiveSkillData> possibleChoices = new List<ActiveSkillData>();
        List<ActiveSkillData> currentSkills = activeSkillManager.GetCurrentlyEquippedSkills();

        // 2а. Добавляем улучшения для текущих скиллов
        foreach (var skill in currentSkills)
        {
            if (skill.nextLevelSkill != null)
            {
                possibleChoices.Add(skill.nextLevelSkill);
            }
            // Здесь в будущем можно добавить логику для ультимейтов
        }

        // 2б. Добавляем новые скиллы, которых еще нет у игрока
        foreach (var newSkill in skillDatabase.allFirstLevelSkills)
        {
            // Проверяем, есть ли у игрока уже какая-либо версия этого скилла
            bool alreadyHasSkill = currentSkills.Any(s => s.skillID.StartsWith(newSkill.skillID.Substring(0, newSkill.skillID.Length - 1)));
            if (!alreadyHasSkill)
            {
                possibleChoices.Add(newSkill);
            }
        }

        // 3. Определяем, сколько вариантов показать, на основе удачи
        int choiceCount = 3;
        if (Random.Range(0f, 100f) < playerStats.GetStat(StatType.Luck))
        {
            choiceCount = 4;
        }

        // 4. Выбираем случайные уникальные варианты из пула
        List<ActiveSkillData> finalChoices = possibleChoices
            .OrderBy(x => Random.value) // Перемешиваем список
            .Take(Mathf.Min(choiceCount, possibleChoices.Count)) // Берем нужное количество (но не больше, чем есть в пуле)
            .ToList();

        // 5. Создаем и настраиваем карточки для каждого варианта
        foreach (var choice in finalChoices)
        {
            GameObject cardObject = Instantiate(skillChoiceCardPrefab, cardContainer);
            SkillChoiceCard card = cardObject.GetComponent<SkillChoiceCard>();
            // Передаем в карточку данные о скилле и ссылку на этот менеджер
            card.Setup(choice, this, currentSkills);
        }
    }
}