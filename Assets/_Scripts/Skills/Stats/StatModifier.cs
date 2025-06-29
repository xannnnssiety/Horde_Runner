// Атрибут [System.Serializable] очень важен. Он позволяет Unity
// отображать объекты этого класса в инспекторе внутри других классов (например, в списке).
using UnityEngine;

[System.Serializable]
public class StatModifier
{
    [Tooltip("Характеристика, которую нужно изменить")]
    public StatType Stat;

    [Tooltip("Значение модификатора")]
    public float Value;

    [Tooltip("Как именно применять модификатор (сложение, умножение и т.д.)")]
    public ModifierType Type;

    [Tooltip("На какие типы умений действует этот модификатор (можно выбрать несколько)")]
    public SkillTag AppliesToTags;

    // Конструктор для удобного создания модификаторов из кода (опционально, но полезно)
    public StatModifier(StatType stat, float value, ModifierType type, SkillTag tags)
    {
        Stat = stat;
        Value = value;
        Type = type;
        AppliesToTags = tags;
    }
}