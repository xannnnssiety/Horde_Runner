using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopPerkIcon : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Ссылки на UI")]
    [SerializeField] private Text titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Transform levelIndicatorContainer;
    [SerializeField] public GameObject checkmarkPrefab; // Префаб одной галочки

    public PassiveSkillData GetSkillData() { return _skillData; }
    public PassiveSkillData _skillData;
    private PassiveShop_UI_Manager _shopManager;
    private Button _button;

    public void Setup(PassiveSkillData data, PassiveShop_UI_Manager manager)
    {
        _skillData = data;
        _shopManager = manager;
        _button = GetComponent<Button>();

        titleText.text = _skillData.skillName;
        iconImage.sprite = _skillData.icon;
    }

    public void UpdateLevelIndicator(int currentLevel, int maxLevel, bool canAfford)
    {
        // Очищаем старые индикаторы
        foreach (Transform child in levelIndicatorContainer)
        {
            Destroy(child.gameObject);
        }

        // Создаем новые индикаторы (галочки)
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject checkmarkObj = Instantiate(checkmarkPrefab, levelIndicatorContainer);
            // Если текущий уровень больше или равен итерации, делаем галочку видимой/яркой
            checkmarkObj.GetComponent<Image>().color = (i < currentLevel) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        foreach (Transform child in levelIndicatorContainer) Destroy(child.gameObject);
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject checkmarkObj = Instantiate(checkmarkPrefab, levelIndicatorContainer);
            checkmarkObj.GetComponent<Image>().color = (i < currentLevel) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        // Обновляем состояние кнопки
        if (currentLevel >= maxLevel)
        {
            _button.interactable = false; // Делаем кнопку неактивной, если макс. уровень
        }
        else
        {
            _button.interactable = canAfford; // Кнопка активна, только если хватает денег
        }
    }

    // Когда мы наводим мышь на иконку
    public void OnPointerEnter(PointerEventData eventData)
    {
        _shopManager.OnPerkHover(_skillData);
    }

    // Когда мы кликаем по иконке
    public void OnPointerClick(PointerEventData eventData)
    {
        _shopManager.OnPerkClick(_skillData);
    }
}