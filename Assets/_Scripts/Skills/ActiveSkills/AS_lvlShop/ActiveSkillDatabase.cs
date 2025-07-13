using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject, который служит базой данных для всех активных умений 1-го уровня.
/// </summary>
[CreateAssetMenu(fileName = "ActiveSkillDatabase", menuName = "Skills/Active Skill Database")]
public class ActiveSkillDatabase : ScriptableObject
{
    [Header("База данных активных умений")]
    [Tooltip("Перетащите сюда все ассеты активных умений 1-го уровня, которые могут быть предложены игроку.")]
    public List<ActiveSkillData> allFirstLevelSkills;
}