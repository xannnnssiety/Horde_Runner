using UnityEngine;
using UnityEngine.UI; // ���������� ��� ������ � UI ���������� (Text, Image, Button)
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ��������� ����� ��������� ������ ������ �� ������ ��������� ������.
/// </summary>
public class SkillChoiceCard : MonoBehaviour
{
    [Header("������ �� UI ��������")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Text titleText; // ���������� Text (Legacy)
    [SerializeField] private Text descriptionText;

    // ������, ������� ��������������� �� ����
    private ActiveSkillData _skillData;
    private LevelUpManager _levelUpManager;
    private Button _button;

    private void Awake()
    {
        // �������� ������ �� ��� ��������� Button � ��������� ���������
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnCardClicked);
    }

    /// <summary>
    /// ����������� ��������, �������� �� ������� � ���������� ������.
    /// </summary>
    public void Setup(ActiveSkillData data, LevelUpManager manager, List<ActiveSkillData> currentSkills)
    {
        _skillData = data;
        _levelUpManager = manager;

        // ��������� ������� UI ��������
        skillIcon.sprite = data.icon;
        descriptionText.text = data.description;

        // --- ��������� ��������� � ������� ---
        // ����, ���� �� � ������ ��� �����-���� ������ ����� ������
        ActiveSkillData currentVersion = currentSkills.FirstOrDefault(s => s.skillID.StartsWith(data.skillID.Substring(0, data.skillID.Length - 1)));

        if (currentVersion == null)
        {
            // ���� � ������ ��� ����� ������, ��� ����� �����
            titleText.text = $"{data.skillName} (�����!)";
        }
        else
        {
            // ���� ����, ����������, �� ������ ������ �� ���������
            // �� ��������� ��������� ����� �� ID (��������, �� "AS_Knives_2" �������� "2")
            char levelChar = data.skillID[data.skillID.Length - 1];
            titleText.text = $"{data.skillName} ��. {levelChar}";
        }
    }

    /// <summary>
    /// ���������� ��� ������� �� �������� (������).
    /// </summary>
    private void OnCardClicked()
    {
        // �������� ���������, ����� ����� ��� ������
        _levelUpManager.OnSkillSelected(_skillData);
    }
}