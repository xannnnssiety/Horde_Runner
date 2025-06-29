using System.Collections.Generic;
using UnityEngine;

// Атрибут CreateAssetMenu позволяет создавать экземпляры этого объекта
// прямо в редакторе Unity через меню Create -> ...
[CreateAssetMenu(fileName = "NewPassiveSkill", menuName = "Skills/Passive Skill Data")]
public class PassiveSkillData : ScriptableObject
{
    [Header("Основная информация")]
    [Tooltip("Уникальный ID навыка. Не должен повторяться.")]
    public string skillID;

    [Tooltip("Название, которое видит игрок")]
    public string skillName;

    [Tooltip("Подробное описание, которое видит игрок")]
    [TextArea(3, 5)] // Делает текстовое поле в инспекторе больше
    public string description;

    [Tooltip("Иконка навыка")]
    public Sprite icon;

    [Header("Настройки в дереве")]
    [Tooltip("Стоимость разблокировки в очках пассивок")]
    public int cost = 1;

    [Tooltip("Тир/размер навыка в дереве")]
    public PassiveSkillTier skillTier = PassiveSkillTier.Normal;

    [Tooltip("Виртуальные координаты для отображения в UI-сетке")]
    public Vector2 gridPosition;

    [Tooltip("Список навыков, которые должны быть изучены, чтобы разблокировать этот")]
    public List<PassiveSkillData> prerequisites;

    [Header("Игровые эффекты")]
    [Tooltip("Список всех модификаторов, которые дает этот навык")]
    public List<StatModifier> modifiers;

    [Header("Уникальное поведение (опционально)")]
    [Tooltip("Префаб с уникальной логикой (например, урон за киллы)")]
    public GameObject uniqueBehaviourPrefab;
}