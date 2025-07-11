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

    /// <summary>
    /// �������������� ������, �������� ������ � � ������ ��� ������������ ��������������.
    /// ���������� �� ActiveSkillManager ��� ���������� ������.
    /// </summary>
    public virtual void Initialize(ActiveSkillData data, PlayerStats stats)
    {
        this.skillData = data;
        this.playerStats = stats;
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
        currentDamage = skillData.baseDamage * (playerStats.GetStat(StatType.Damage) / 100f);
        currentAreaOfEffect = skillData.baseAreaOfEffect * (playerStats.GetStat(StatType.AreaOfEffect) / 100f);
        currentCooldown = skillData.baseCooldown * (playerStats.GetStat(StatType.Cooldown) / 100f);
        currentProjectileSpeed = skillData.baseProjectileSpeed * (playerStats.GetStat(StatType.ProjectileSpeed) / 100f);
        currentDuration = skillData.baseDuration * (playerStats.GetStat(StatType.Duration) / 100f);
        currentAmount = skillData.baseAmount + ((int)playerStats.GetStat(StatType.Amount) - 1);
    }

    /// <summary>
    /// �������� ������ ������. ���� ����� ������ ���� ���������� � ������ �������� ������.
    /// </summary>
    public abstract void Activate();
}