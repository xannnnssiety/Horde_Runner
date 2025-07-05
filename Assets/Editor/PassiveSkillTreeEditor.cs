using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(PassiveSkillTree))]
public class PassiveSkillTreeEditor : Editor
{
    // Словарь для хранения состояния сворачиваемости (открыт/закрыт) для каждой подгруппы
    private Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

    public override void OnInspectorGUI()
    {
        // Получаем доступ к нашему объекту PassiveSkillTree
        PassiveSkillTree skillTree = (PassiveSkillTree)target;

        // --- Отрисовка базового списка (остается без изменений) ---
        EditorGUILayout.LabelField("Базовый список навыков", EditorStyles.boldLabel);
        serializedObject.Update();
        SerializedProperty allSkillsProperty = serializedObject.FindProperty("allSkills");
        EditorGUILayout.PropertyField(allSkillsProperty, true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Автоматически сгруппированное представление", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Это представление автоматически группирует навыки из списка выше. Редактировать их нужно в базовом списке.", MessageType.Info);
        EditorGUILayout.Space(5);

        // --- Группируем навыки по ТИРУ (Keystone, Notable, Normal) ---
        var skillsByTier = skillTree.allSkills
            .Where(s => s != null) // Фильтруем пустые элементы
            .GroupBy(s => s.skillTier) // Группируем по тиру
            .OrderByDescending(g => (int)g.Key); // Сортируем (Keystone -> Notable -> Normal)

        // --- Проходим по каждой группе ТИРОВ ---
        foreach (var tierGroup in skillsByTier)
        {
            PassiveSkillTier tier = tierGroup.Key;
            List<PassiveSkillData> skillsInTier = tierGroup.ToList();

            // Рисуем большой заголовок для тира
            EditorGUILayout.LabelField(tier.ToString(), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // --- ВНУТРЕННЯЯ ГРУППИРОВКА по основному СТАТУ ---
            // Группируем навыки внутри тира по первому модификатору стата
            var skillsByStat = skillsInTier
                .GroupBy(s => s.modifiers.Count > 0 ? s.modifiers[0].Stat.ToString() : "Без модификаторов")
                .OrderBy(g => g.Key); // Сортируем по алфавиту

            // --- Проходим по каждой группе СТАТОВ ---
            foreach (var statGroup in skillsByStat)
            {
                string statName = statGroup.Key;
                List<PassiveSkillData> skillsInStatGroup = statGroup.ToList();
                string foldoutKey = $"{tier}-{statName}"; // Уникальный ключ для каждого сворачиваемого списка

                // Инициализируем состояние, если его еще нет
                if (!_foldoutStates.ContainsKey(foldoutKey))
                {
                    _foldoutStates[foldoutKey] = false;
                }

                // Рисуем сворачиваемый список для стата
                _foldoutStates[foldoutKey] = EditorGUILayout.Foldout(_foldoutStates[foldoutKey], $"{statName} ({skillsInStatGroup.Count})", true);

                if (_foldoutStates[foldoutKey])
                {
                    // Рисуем список навыков внутри этой подгруппы
                    DrawSkillList(skillsInStatGroup);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    // Вспомогательный метод для отрисовки списка (остается без изменений)
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