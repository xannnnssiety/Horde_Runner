using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] обязательно, чтобы этот класс отображался в инспекторе
[System.Serializable]
public class PrerequisiteGroup
{
    // Определяем, как должны быть выполнены требования ВНУТРИ этой группы
    public enum GroupLogicType { AND, OR }

    [Tooltip("AND: нужно изучить ВСЕ навыки в этой группе. OR: нужно изучить ХОТЯ БЫ ОДИН навык в этой группе.")]
    public GroupLogicType logicType = GroupLogicType.AND;

    [Tooltip("Список навыков, к которым применяется логика этой группы.")]
    public List<PassiveSkillData> requiredSkills;
}