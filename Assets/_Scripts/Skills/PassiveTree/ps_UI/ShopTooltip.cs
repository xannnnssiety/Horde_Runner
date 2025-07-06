using UnityEngine;
using UnityEngine.UI;

public class ShopTooltip : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text costText;

    public void ShowTooltip(PassiveSkillData skillData, int currentLevel, int maxLevel, int currentCost)
    {
        gameObject.SetActive(true);
        iconImage.sprite = skillData.icon;
        titleText.text = skillData.skillName;
        descriptionText.text = skillData.description;

        if (currentLevel >= maxLevel)
        {
            
            costText.text = "Ã¿ —.";
        }
        else
        {
            
            costText.text = $"÷ÂÌ‡: {currentCost}";
        }

    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}