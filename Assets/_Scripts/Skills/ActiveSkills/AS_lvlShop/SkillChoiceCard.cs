using UnityEngine;
using UnityEngine.UI; // Необходимо для работы с UI элементами (Text, Image, Button)
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Управляет одной карточкой выбора умения на экране повышения уровня.
/// </summary>
public class SkillChoiceCard : MonoBehaviour
{
    [Header("Ссылки на UI элементы")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Text titleText; // Используем Text (Legacy)
    [SerializeField] private Text descriptionText;

    // Ссылки, которые устанавливаются из кода
    private ActiveSkillData _skillData;
    private LevelUpManager _levelUpManager;
    private Button _button;

    private void Awake()
    {
        // Получаем ссылку на наш компонент Button и добавляем слушателя
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnCardClicked);
    }

    /// <summary>
    /// Настраивает карточку, заполняя ее данными о конкретном умении.
    /// </summary>
    public void Setup(ActiveSkillData data, LevelUpManager manager, List<ActiveSkillData> currentSkills)
    {
        _skillData = data;
        _levelUpManager = manager;

        // Заполняем базовые UI элементы
        skillIcon.sprite = data.icon;
        descriptionText.text = data.description;

        // --- Формируем заголовок с уровнем ---
        // Ищем, есть ли у игрока уже какая-либо версия этого скилла
        ActiveSkillData currentVersion = currentSkills.FirstOrDefault(s => s.skillID.StartsWith(data.skillID.Substring(0, data.skillID.Length - 1)));

        if (currentVersion == null)
        {
            // Если у игрока нет этого скилла, это новый скилл
            titleText.text = $"{data.skillName} (НОВЫЙ!)";
        }
        else
        {
            // Если есть, показываем, до какого уровня он улучшится
            // Мы извлекаем последнюю цифру из ID (например, из "AS_Knives_2" получаем "2")
            char levelChar = data.skillID[data.skillID.Length - 1];
            titleText.text = $"{data.skillName} ур. {levelChar}";
        }
    }

    /// <summary>
    /// Вызывается при нажатии на карточку (кнопку).
    /// </summary>
    private void OnCardClicked()
    {
        // Сообщаем менеджеру, какой скилл был выбран
        _levelUpManager.OnSkillSelected(_skillData);
    }
}