using System.Collections.Generic;
using UnityEngine;

// Этот атрибут позволит нам создать экземпляр этого хранилища прямо в редакторе
[CreateAssetMenu(fileName = "PassiveSkillTree", menuName = "Skills/Passive Skill Tree")]
public class PassiveSkillTree : ScriptableObject
{
    // Просто список, в который мы будем перетаскивать все наши ассеты пассивных навыков
    public List<PassiveSkillData> allSkills;
}