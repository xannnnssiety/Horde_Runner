using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(PassiveSkillTree))]
public class PassiveSkillTreeEditor : Editor
{
    // ������� ��� �������� ��������� ��������������� (������/������) ��� ������ ���������
    private Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

    public override void OnInspectorGUI()
    {
        // �������� ������ � ������ ������� PassiveSkillTree
        PassiveSkillTree skillTree = (PassiveSkillTree)target;

        // --- ��������� �������� ������ (�������� ��� ���������) ---
        EditorGUILayout.LabelField("������� ������ �������", EditorStyles.boldLabel);
        serializedObject.Update();
        SerializedProperty allSkillsProperty = serializedObject.FindProperty("allSkills");
        EditorGUILayout.PropertyField(allSkillsProperty, true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("������������� ��������������� �������������", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("��� ������������� ������������� ���������� ������ �� ������ ����. ������������� �� ����� � ������� ������.", MessageType.Info);
        EditorGUILayout.Space(5);

        // --- ���������� ������ �� ���� (Keystone, Notable, Normal) ---
        var skillsByTier = skillTree.allSkills
            .Where(s => s != null) // ��������� ������ ��������
            .GroupBy(s => s.skillTier) // ���������� �� ����
            .OrderByDescending(g => (int)g.Key); // ��������� (Keystone -> Notable -> Normal)

        // --- �������� �� ������ ������ ����� ---
        foreach (var tierGroup in skillsByTier)
        {
            PassiveSkillTier tier = tierGroup.Key;
            List<PassiveSkillData> skillsInTier = tierGroup.ToList();

            // ������ ������� ��������� ��� ����
            EditorGUILayout.LabelField(tier.ToString(), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // --- ���������� ����������� �� ��������� ����� ---
            // ���������� ������ ������ ���� �� ������� ������������ �����
            var skillsByStat = skillsInTier
                .GroupBy(s => s.modifiers.Count > 0 ? s.modifiers[0].Stat.ToString() : "��� �������������")
                .OrderBy(g => g.Key); // ��������� �� ��������

            // --- �������� �� ������ ������ ������ ---
            foreach (var statGroup in skillsByStat)
            {
                string statName = statGroup.Key;
                List<PassiveSkillData> skillsInStatGroup = statGroup.ToList();
                string foldoutKey = $"{tier}-{statName}"; // ���������� ���� ��� ������� �������������� ������

                // �������������� ���������, ���� ��� ��� ���
                if (!_foldoutStates.ContainsKey(foldoutKey))
                {
                    _foldoutStates[foldoutKey] = false;
                }

                // ������ ������������� ������ ��� �����
                _foldoutStates[foldoutKey] = EditorGUILayout.Foldout(_foldoutStates[foldoutKey], $"{statName} ({skillsInStatGroup.Count})", true);

                if (_foldoutStates[foldoutKey])
                {
                    // ������ ������ ������� ������ ���� ���������
                    DrawSkillList(skillsInStatGroup);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    // ��������������� ����� ��� ��������� ������ (�������� ��� ���������)
    private void DrawSkillList(List<PassiveSkillData> skills)
    {
        EditorGUI.indentLevel++;
        foreach (var skill in skills)
        {
            EditorGUILayout.ObjectField(skill.skillName, skill, typeof(PassiveSkillData), false);
        }
        EditorGUI.indentLevel--;
    }
}