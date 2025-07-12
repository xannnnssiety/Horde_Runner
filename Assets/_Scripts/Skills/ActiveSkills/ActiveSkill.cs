using UnityEngine;

/// <summary>
/// Абстрактный базовый класс для всех активных умений.
/// Служит шаблоном, определяющим общую структуру и поведение.
/// Этот компонент должен находиться на префабе, указанном в ActiveSkillData.
/// </summary>
public abstract class ActiveSkill : MonoBehaviour
{
    // --- ИЗМЕНЕНИЕ: Публичные свойства с приватным сеттером ---
    // 'public' позволяет другим скриптам (как ActiveSkillManager) читать эти значения.
    // 'private set' означает, что изменять их может только этот класс. Это безопасно.
    public float currentCooldown { get; private set; }
    public ActiveSkillData skillData { get; private set; }

    // --- Рассчитанные характеристики ---
    // Эти поля остаются 'protected', так как они нужны только наследникам (конкретным умениям).
    protected float currentDamage;
    protected float currentAreaOfEffect;
    protected int currentAmount;
    protected float currentProjectileSpeed;
    protected float currentDuration;

    // Ссылка на PlayerStats
    protected PlayerStats playerStats;
    protected float effectivenessMultiplier = 1.0f;

    /// <summary>
    /// Инициализирует умение, получает ссылки и в первый раз рассчитывает характеристики.
    /// Вызывается из ActiveSkillManager при добавлении умения.
    /// </summary>
    public virtual void Initialize(ActiveSkillData data, PlayerStats stats, float effectiveness = 1.0f)
    {
        this.skillData = data;
        this.playerStats = stats;
        this.effectivenessMultiplier = effectiveness;
        RecalculateStats();
    }

    /// <summary>
    /// Пересчитывает все характеристики умения на основе текущих статов игрока.
    /// Вызывается при инициализации и каждый раз, когда статы игрока меняются.
    /// </summary>
    public virtual void RecalculateStats()
    {
        if (playerStats == null || skillData == null) return;

        // Рассчитываем итоговые значения.
        currentDamage = skillData.baseDamage * (playerStats.GetStat(StatType.Damage) / 100f) * effectivenessMultiplier;
        currentAreaOfEffect = skillData.baseAreaOfEffect * (playerStats.GetStat(StatType.AreaOfEffect) / 100f) * effectivenessMultiplier;
        currentCooldown = skillData.baseCooldown * (playerStats.GetStat(StatType.Cooldown) / 100f);
        currentProjectileSpeed = skillData.baseProjectileSpeed * (playerStats.GetStat(StatType.ProjectileSpeed) / 100f) * effectivenessMultiplier;
        currentDuration = skillData.baseDuration * (playerStats.GetStat(StatType.Duration) / 100f);
        currentAmount = skillData.baseAmount + ((int)playerStats.GetStat(StatType.Amount) - 1);
    }

    /// <summary>
    /// Основная логика умения. Этот метод ДОЛЖЕН быть реализован в каждом дочернем классе.
    /// </summary>
    public abstract void Activate();
}