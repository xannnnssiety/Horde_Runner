using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class PassiveSkill_UI_Node : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Ссылки на компоненты этого префаба
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage; // Предполагаем, что у вас есть рамка

    // Храним данные о навыке, который мы представляем
    private PassiveSkillData _skillData;
    // Ссылка на главный менеджер, чтобы сообщить ему о клике
    private PassiveTree_UI_Manager _uiManager;

    // Метод для инициализации этого узла извне
    public void Setup(PassiveSkillData data, PassiveTree_UI_Manager manager)
    {
        _skillData = data;
        _uiManager = manager;

        // Устанавливаем иконку
        iconImage.sprite = _skillData.icon;

        // Добавляем слушатель на клик кнопки
        GetComponent<Button>().onClick.AddListener(OnNodeClicked);
    }

    // Метод, который вызывается при клике на этот узел
    private void OnNodeClicked()
    {
        // Сообщаем главному менеджеру, что на нас кликнули, и передаем ему наши данные
        _uiManager.OnSkillNodeClicked(_skillData);
    }

    // Метод для обновления внешнего вида (изучен, доступен, заблокирован)
    public void UpdateVisuals(Color frameColor)
    {
        // Просто устанавливаем тот цвет, который нам передали
        if (frameImage != null)
        {
            frameImage.color = frameColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Вызываем менеджер тултипов и просим его показать информацию о нашем навыке
        TooltipManager.Instance.ShowTooltip(_skillData.skillName, _skillData.description);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        // Просим менеджер спрятать подсказку
        TooltipManager.Instance.HideTooltip();
    }

}