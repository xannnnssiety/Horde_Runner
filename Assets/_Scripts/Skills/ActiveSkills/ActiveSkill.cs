using UnityEngine;

/// <summary>
/// ����������� ������� ����� ��� ���� �������� ������.
/// ������ ��������, ������������ ����� ��������� � ���������.
/// ���� ��������� ������ ���������� �� �������, ��������� � ActiveSkillData.
/// </summary>
public abstract class ActiveSkill : MonoBehaviour
{
    // --- ���������: ��������� �������� � ��������� �������� ---
    // 'public' ��������� ������ �������� (��� ActiveSkillManager) ������ ��� ��������.
    // 'private set' ��������, ��� �������� �� ����� ������ ���� �����. ��� ���������.
    public float currentCooldown { get; private set; }
    public ActiveSkillData skillData { get; private set; }

    // --- ������������ �������������� ---
    // ��� ���� �������� 'protected', ��� ��� ��� ����� ������ ����������� (���������� �������).
    protected float currentDamage;
    protected float currentAreaOfEffect;
    protected int currentAmount;
    protected float currentProjectileSpeed;
    protected float currentDuration;

    // ������ �� PlayerStats
    protected PlayerStats playerStats;
    protected float effectivenessMultiplier = 1.0f;

    /// <summary>
    /// �������������� ������, �������� ������ � � ������ ��� ������������ ��������������.
    /// ���������� �� ActiveSkillManager ��� ���������� ������.
    /// </summary>
    public virtual void Initialize(ActiveSkillData data, PlayerStats stats, float effectiveness = 1.0f)
    {
        this.skillData = data;
        this.playerStats = stats;
        this.effectivenessMultiplier = effectiveness;
        RecalculateStats();
    }

    /// <summary>
    /// ������������� ��� �������������� ������ �� ������ ������� ������ ������.
    /// ���������� ��� ������������� � ������ ���, ����� ����� ������ ��������.
    /// </summary>
    public virtual void RecalculateStats()
    {
        if (playerStats == null || skillData == null) return;

        // ������������ �������� ��������.
        currentDamage = skillData.baseDamage * (playerStats.GetStat(StatType.Damage) / 100f) * effectivenessMultiplier;
        currentAreaOfEffect = skillData.baseAreaOfEffect * (playerStats.GetStat(StatType.AreaOfEffect) / 100f) * effectivenessMultiplier;
        currentCooldown = skillData.baseCooldown * (playerStats.GetStat(StatType.Cooldown) / 100f);
        currentProjectileSpeed = skillData.baseProjectileSpeed * (playerStats.GetStat(StatType.ProjectileSpeed) / 100f) * effectivenessMultiplier;
        currentDuration = skillData.baseDuration * (playerStats.GetStat(StatType.Duration) / 100f);
        currentAmount = skillData.baseAmount + ((int)playerStats.GetStat(StatType.Amount) - 1);
    }

    /// <summary>
    /// �������� ������ ������. ���� ����� ������ ���� ���������� � ������ �������� ������.
    /// </summary>
    public abstract void Activate();
}