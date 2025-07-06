using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveShop_UI_Manager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PassiveSkillTree skillTreeAsset;
    [SerializeField] private ShopTooltip tooltip; // Ссылка на нижнюю панель

    [Header("Настройки генерации")]
    [SerializeField] private GameObject perkIconPrefab;
    [SerializeField] private Transform contentContainer; // Объект Content внутри ScrollView
    [SerializeField] private GameObject checkmarkPrefab; // Ссылка на префаб галочки

    private List<ShopPerkIcon> _perkIcons = new List<ShopPerkIcon>();

    void Start()
    {
        // Настраиваем сетку
        
        if (gameManager == null || skillTreeAsset == null || perkIconPrefab == null || contentContainer == null || tooltip == null)
        {
            Debug.LogError("Одна или несколько ссылок в PassiveShop_UI_Manager не установлены в инспекторе!", this);
            return; // Прерываем выполнение, чтобы избежать других ошибок
        }

        GenerateShop();
        tooltip.HideTooltip(); // Прячем тултип при старте
    }

    private void OnEnable()
    {
        UpdateAllPerkVisuals();
    }

    private void GenerateShop()
    {
        foreach (PassiveSkillData skillData in skillTreeAsset.allSkills)
        {
            GameObject iconObj = Instantiate(perkIconPrefab, contentContainer);
            ShopPerkIcon perkIcon = iconObj.GetComponent<ShopPerkIcon>();

            // Передаем префаб галочки в иконку
            perkIcon.GetComponent<ShopPerkIcon>().checkmarkPrefab = this.checkmarkPrefab;

            perkIcon.Setup(skillData, this);
            _perkIcons.Add(perkIcon);
        }
        UpdateAllPerkVisuals();
    }

    public void UpdateAllPerkVisuals()
    {
        SaveData saveData = gameManager.CurrentSaveData;
        foreach (ShopPerkIcon icon in _perkIcons)
        {
            // Получаем данные для каждого перка
            PassiveSkillData data = icon.GetSkillData(); // Нам понадобится этот метод-геттер в ShopPerkIcon

            saveData.unlockedPassives.TryGetValue(data.skillID, out int currentLevel);

            // Вычисляем текущую стоимость
            int currentCost = gameManager.GetCurrentSkillCost(data);
            bool canAfford = saveData.currency >= currentCost;

            // Обновляем и индикатор уровня, и состояние кнопки
            icon.UpdateLevelIndicator(currentLevel, data.maxPurchaseCount, canAfford);
        }
    }

    // Вызывается, когда мы наводим мышь на иконку
    public void OnPerkHover(PassiveSkillData skillData)
    {
        // 1. Получаем текущий уровень прокачки из сохранения
        gameManager.CurrentSaveData.unlockedPassives.TryGetValue(skillData.skillID, out int currentLevel);

        // 2. Получаем максимальный уровень из данных самого перка
        int maxLevel = skillData.maxPurchaseCount;

        // 3. Запрашиваем у GameManager актуальную цену для этого перка
        int currentCost = gameManager.GetCurrentSkillCost(skillData);

        // 4. Передаем ВСЕ данные в тултип для отображения
        tooltip.ShowTooltip(skillData, currentLevel, maxLevel, currentCost);
    }

    // Вызывается, когда мы кликаем по иконке
    public void OnPerkClick(PassiveSkillData skillData)
    {
        gameManager.UnlockPassive(skillData);
        // После покупки обновляем все иконки, чтобы отобразить новый уровень
        UpdateAllPerkVisuals();
        // И обновляем тултип для текущего перка
        OnPerkHover(skillData);
    }
}