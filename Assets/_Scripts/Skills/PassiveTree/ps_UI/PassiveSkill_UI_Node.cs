using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class PassiveSkill_UI_Node : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ������ �� ���������� ����� �������
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage; // ������������, ��� � ��� ���� �����

    // ������ ������ � ������, ������� �� ������������
    private PassiveSkillData _skillData;
    // ������ �� ������� ��������, ����� �������� ��� � �����
    private PassiveTree_UI_Manager _uiManager;

    // ����� ��� ������������� ����� ���� �����
    public void Setup(PassiveSkillData data, PassiveTree_UI_Manager manager)
    {
        _skillData = data;
        _uiManager = manager;

        // ������������� ������
        iconImage.sprite = _skillData.icon;

        // ��������� ��������� �� ���� ������
        GetComponent<Button>().onClick.AddListener(OnNodeClicked);
    }

    // �����, ������� ���������� ��� ����� �� ���� ����
    private void OnNodeClicked()
    {
        // �������� �������� ���������, ��� �� ��� ��������, � �������� ��� ���� ������
        _uiManager.OnSkillNodeClicked(_skillData);
    }

    // ����� ��� ���������� �������� ���� (������, ��������, ������������)
    public void UpdateVisuals(Color frameColor)
    {
        // ������ ������������� ��� ����, ������� ��� ��������
        if (frameImage != null)
        {
            frameImage.color = frameColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // �������� �������� �������� � ������ ��� �������� ���������� � ����� ������
        TooltipManager.Instance.ShowTooltip(_skillData.skillName, _skillData.description);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        // ������ �������� �������� ���������
        TooltipManager.Instance.HideTooltip();
    }

}