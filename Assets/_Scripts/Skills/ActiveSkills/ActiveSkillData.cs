using UnityEngine;

// Атрибут для создания ассетов этого типа через меню Unity (Assets -> Create -> Skills -> Active Skill)
[CreateAssetMenu(fileName = "NewActiveSkill", menuName = "Skills/Active Skill")]
public class ActiveSkillData : ScriptableObject
{
    [Header("Основная информация")]
    [Tooltip("Уникальный ID умения. Используется для сохранения и идентификации.")]
    public string skillID;

    [Tooltip("Название умения, которое видит игрок.")]
    public string skillName;

    [Tooltip("Подробное описание умения для UI.")]
    [TextArea(3, 5)]
    public string description;

    [Tooltip("Иконка умения.")]
    public Sprite icon;

    [Header("Игровая логика и связь")]
    [Tooltip("Теги, определяющие тип умения. Позволяет пассивным перкам влиять на него.")]
    public SkillTag tags;

    [Tooltip("Префаб, содержащий логику умения (скрипт, наследуемый от ActiveSkill).")]
    public GameObject skillLogicPrefab;

    [Header("Базовые характеристики")]
    [Tooltip("Базовый урон умения.")]
    public float baseDamage = 10f;

    [Tooltip("Базовая перезарядка в секундах.")]
    public float baseCooldown = 5f;

    [Tooltip("Базовый радиус/область действия.")]
    public float baseAreaOfEffect = 1f;

    [Tooltip("Базовое количество снарядов/ударов за раз.")]
    public int baseAmount = 1;

    [Tooltip("Базовая скорость полета для снарядов.")]
    public float baseProjectileSpeed = 10f;

    [Tooltip("Базовая длительность эффектов (например, для аур или DoT).")]
    public float baseDuration = 3f;

    [Header("Система уровней")]
    [Tooltip("Ссылка на ScriptableObject следующего уровня этого умения. Оставьте пустым для максимального уровня.")]
    public ActiveSkillData nextLevelSkill;

    [Tooltip("Ссылка на ScriptableObject ультимативной версии умения. Оставьте пустым, если ее нет.")]
    public ActiveSkillData ultimateVersionSkill;
}