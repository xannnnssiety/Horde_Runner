using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveShop_UI_Manager : MonoBehaviour
{
    [Header("������")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PassiveSkillTree skillTreeAsset;
    [SerializeField] private ShopTooltip tooltip; // ������ �� ������ ������

    [Header("��������� ���������")]
    [SerializeField] private GameObject perkIconPrefab;
    [SerializeField] private Transform contentContainer; // ������ Content ������ ScrollView
    [SerializeField] private GameObject checkmarkPrefab; // ������ �� ������ �������

    private List<ShopPerkIcon> _perkIcons = new List<ShopPerkIcon>();

    void Start()
    {
        // ����������� �����
        
        if (gameManager == null || skillTreeAsset == null || perkIconPrefab == null || contentContainer == null || tooltip == null)
        {
            Debug.LogError("���� ��� ��������� ������ � PassiveShop_UI_Manager �� ����������� � ����������!", this);
            return; // ��������� ����������, ����� �������� ������ ������
        }

        GenerateShop();
        tooltip.HideTooltip(); // ������ ������ ��� ������
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

            // �������� ������ ������� � ������
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
            // �������� ������ ��� ������� �����
            PassiveSkillData data = icon.GetSkillData(); // ��� ����������� ���� �����-������ � ShopPerkIcon

            saveData.unlockedPassives.TryGetValue(data.skillID, out int currentLevel);

            // ��������� ������� ���������
            int currentCost = gameManager.GetCurrentSkillCost(data);
            bool canAfford = saveData.currency >= currentCost;

            // ��������� � ��������� ������, � ��������� ������
            icon.UpdateLevelIndicator(currentLevel, data.maxPurchaseCount, canAfford);
        }
    }

    // ����������, ����� �� ������� ���� �� ������
    public void OnPerkHover(PassiveSkillData skillData)
    {
        // 1. �������� ������� ������� �������� �� ����������
        gameManager.CurrentSaveData.unlockedPassives.TryGetValue(skillData.skillID, out int currentLevel);

        // 2. �������� ������������ ������� �� ������ ������ �����
        int maxLevel = skillData.maxPurchaseCount;

        // 3. ����������� � GameManager ���������� ���� ��� ����� �����
        int currentCost = gameManager.GetCurrentSkillCost(skillData);

        // 4. �������� ��� ������ � ������ ��� �����������
        tooltip.ShowTooltip(skillData, currentLevel, maxLevel, currentCost);
    }

    // ����������, ����� �� ������� �� ������
    public void OnPerkClick(PassiveSkillData skillData)
    {
        gameManager.UnlockPassive(skillData);
        // ����� ������� ��������� ��� ������, ����� ���������� ����� �������
        UpdateAllPerkVisuals();
        // � ��������� ������ ��� �������� �����
        OnPerkHover(skillData);
    }
}