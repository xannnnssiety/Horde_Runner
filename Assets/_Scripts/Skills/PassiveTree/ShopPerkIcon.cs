using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopPerkIcon : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("������ �� UI")]
    [SerializeField] private Text titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Transform levelIndicatorContainer;
    [SerializeField] public GameObject checkmarkPrefab; // ������ ����� �������

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
        // ������� ������ ����������
        foreach (Transform child in levelIndicatorContainer)
        {
            Destroy(child.gameObject);
        }

        // ������� ����� ���������� (�������)
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject checkmarkObj = Instantiate(checkmarkPrefab, levelIndicatorContainer);
            // ���� ������� ������� ������ ��� ����� ��������, ������ ������� �������/�����
            checkmarkObj.GetComponent<Image>().color = (i < currentLevel) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        foreach (Transform child in levelIndicatorContainer) Destroy(child.gameObject);
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject checkmarkObj = Instantiate(checkmarkPrefab, levelIndicatorContainer);
            checkmarkObj.GetComponent<Image>().color = (i < currentLevel) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        // ��������� ��������� ������
        if (currentLevel >= maxLevel)
        {
            _button.interactable = false; // ������ ������ ����������, ���� ����. �������
        }
        else
        {
            _button.interactable = canAfford; // ������ �������, ������ ���� ������� �����
        }
    }

    // ����� �� ������� ���� �� ������
    public void OnPointerEnter(PointerEventData eventData)
    {
        _shopManager.OnPerkHover(_skillData);
    }

    // ����� �� ������� �� ������
    public void OnPointerClick(PointerEventData eventData)
    {
        _shopManager.OnPerkClick(_skillData);
    }
}