using UnityEngine;
using System.Collections.Generic; // ���������� ��� ������������� List<>

/// <summary>
/// �������� ������ ��� ���������� �������� ������ �� ������� ������.
/// </summary>
public class TestSkillAdder : MonoBehaviour
{
    // ���������� ����� ��� ������� ��������� � ����������
    [System.Serializable]
    public class SkillTestEntry
    {
        [Tooltip("������ ��� ������� � ����������, �� ������ �� ������.")]
        public string description;
        [Tooltip("�������, �� ������� �� ������� ����� ��������� ������.")]
        public KeyCode triggerKey;
        [Tooltip("����� ������, ������� ����� ��������.")]
        public ActiveSkillData skillToAdd;
    }

    [Header("������")]
    [Tooltip("������ �� �������� �������� ������ �� ������. ���������� ���� ������ � ���� �����������.")]
    public ActiveSkillManager skillManager;
    [Tooltip("������ �� �������� ��������� ������.")]
    public LevelUpManager levelUpManager; 

    [Header("��������� �����")]
    [Tooltip("������ ������ ��� ������������ �� ������� ������.")]
    public List<SkillTestEntry> skillsToTest;
    [Tooltip("������� ��� ������ ������ ��������� ������.")]
    public KeyCode levelUpKey = KeyCode.L; 

    void Update()
    {
        // ���������, �������� �� ��������, ����� �������� ������
        if (skillManager == null)
        {
            return;
        }

        // �������� �� ������� ������ ��������� ������
        foreach (var entry in skillsToTest)
        {
            // ���� ���� ������ �������, ��������� � ������
            if (Input.GetKeyDown(entry.triggerKey))
            {
                // � ���� � ������ ������� ������ ��� ����������
                if (entry.skillToAdd != null)
                {
                    Debug.Log($"������ ������� {entry.triggerKey}. ��������� ������: {entry.skillToAdd.skillName}");
                    // �������� ������� ����� ������ ���������
                    skillManager.AddSkill(entry.skillToAdd);
                }
            }

        }

        if (Input.GetKeyDown(levelUpKey))
        {
            // � ���� ������ �� �������� �����������
            if (levelUpManager != null)
            {
                Debug.Log("������ ������� ��������� ������. ���������� ����� ������.");
                levelUpManager.ShowSelectionScreen();
            }
        }

    }
}