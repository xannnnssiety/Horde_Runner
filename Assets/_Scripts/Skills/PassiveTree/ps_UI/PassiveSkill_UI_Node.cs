using UnityEngine;
using UnityEngine.UI;
using System;

public class PassiveSkill_UI_Node : MonoBehaviour
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
    public void UpdateVisuals(bool isUnlocked, bool canBeUnlocked)
    {
        // ������ ������� ������ ���������
        if (isUnlocked)
        {
            frameImage.color = Color.yellow; // ������ - �������
        }
        else if (canBeUnlocked)
        {
            frameImage.color = Color.white; // �������� ��� �������� - �����
        }
        else
        {
            frameImage.color = Color.gray; // ������������ - �����
        }
    }
}