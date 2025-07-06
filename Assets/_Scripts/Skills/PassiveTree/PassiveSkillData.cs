using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPassiveSkill", menuName = "Skills/Passive Skill Data")]
public class PassiveSkillData : ScriptableObject
{
    [Header("Основная информация")]
    [Tooltip("Уникальный ID навыка. Не должен повторяться.")]
    public string skillID;

    [Tooltip("Название, которое видит игрок")]
    public string skillName;

    [Tooltip("Подробное описание, которое видит игрок")]
    [TextArea(3, 5)]
    public string description;

    [Tooltip("Иконка навыка")]
    public Sprite icon;

    [Header("Игровая логика")]
    [Tooltip("Базовая стоимость первой покупки")]
    public int baseCost = 1;

    [Tooltip("Максимальное количество раз, которое можно купить этот навык")]
    public int maxPurchaseCount = 1;

    [Header("Игровые эффекты")]
    [Tooltip("Список всех модификаторов, которые дает этот навык ЗА ОДНУ ПОКУПКУ")]
    public List<StatModifier> modifiers;

    [Header("Уникальное поведение (опционально)")]
    [Tooltip("Префаб с уникальной логикой. Будет создан только при ПЕРВОЙ покупке.")]
    public GameObject uniqueBehaviourPrefab;
}