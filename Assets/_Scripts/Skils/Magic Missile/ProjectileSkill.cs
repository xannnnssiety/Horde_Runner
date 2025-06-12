using UnityEngine;
using System.Collections;

public class ProjectileSkill : BaseSkill
{
    [Header("Projectile Skill Settings")]
    public GameObject projectilePrefab;
    public float baseDamage = 15;
    public float baseCooldown = 2.0f;
    public float baseProjectileSpeed = 20f;
    public int baseAmount = 1;
    public float baseProjectileSize = 1f;
    public float baseLifetime = 5f;
    public float searchRadius = 50f;
    public LayerMask enemyLayerMask;
    [Tooltip("�������� ����� ��������� � ����� �����")]
    public float delayBetweenShots = 0.08f; // <-- ������� � ��������� ��� ��������
    [Tooltip("�����, �� ������� ����� �������� �������")]
    public Transform firePoint;

    // ��������� ��������
    private int currentDamage;
    private float currentCooldown;
    private float currentProjectileSpeed;
    private int currentAmount;
    private float currentProjectileSize;

    void Start()
    {
        // ��������� �������� ����, ���������� �� ������� ����� �������
        StartCoroutine(FireCycleCoroutine());
    }

    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;
        var stats = PlayerStatsManager.Instance;

        float damageMult = stats.damageMultiplier;
        float sizeMult = stats.sizeMultiplier;
        float cooldownMult = stats.cooldownMultiplier;
        int amountBonus = stats.amountBonus;
        float speedMult = stats.projectileSpeedMultiplier;

        currentDamage = Mathf.RoundToInt(baseDamage * (1f + damageMult));
        currentCooldown = baseCooldown / (1f + cooldownMult);
        currentAmount = baseAmount + amountBonus;
        currentProjectileSize = baseProjectileSize * (1f + sizeMult);

        // --- ����������� ����� ---
        // ������ �������� �������� ��������� ��������� �����
        currentProjectileSpeed = baseProjectileSpeed * (1f + speedMult);
    }

    // ���� ���� ���� �������� �������, ������� ���� � ��������� ���� ����
    private IEnumerator FireCycleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentCooldown);

            Transform closestTarget = FindClosestEnemy();

            if (closestTarget != null)
            {
                // ��������� �������� ������ �����, ��������� �� ����
                StartCoroutine(FireVolleyCoroutine(closestTarget));
            }
        }
    }

    // ��� �������� �������� �� ������ ������ ������� ����� � ��������� ����
    private IEnumerator FireVolleyCoroutine(Transform target)
    {
        for (int i = 0; i < currentAmount; i++)
        {
            // ����� ������ ��������� ���������, ���� �� ��� ����
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                yield break; // ���� ���� ������, ���������� ����
            }

            FireProjectile(target);

            // ���� ��������� �������� ����� �������� ���������� �������
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    // ����� ��� �������� ����� ��������
    private void FireProjectile(Transform target)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;
        Quaternion initialRotation = Quaternion.LookRotation(target.position - spawnPosition);

        GameObject projectileGO = Instantiate(projectilePrefab, spawnPosition, initialRotation);

        if (projectileGO.TryGetComponent<SkillProjectile>(out var projectile))
        {
            // --- ��������� ����� ---
            // ������ �������� � 'this' ��� ������ �� �����
            projectile.Initialize(this, currentDamage, currentProjectileSpeed, currentProjectileSize, target, enemyLayerMask, baseLifetime);
        }
    }

    // ��������������� ����� ��� ������ ����� (����� �� ������������ ��������)
    private Transform FindClosestEnemy()
    {
        Collider[] allTargets = Physics.OverlapSphere(transform.position, searchRadius, enemyLayerMask);
        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        if (allTargets.Length == 0) return null;

        foreach (var targetCollider in allTargets)
        {
            if (targetCollider.TryGetComponent<EnemyAI>(out _) || targetCollider.TryGetComponent<ProjectileEnemyAI>(out _))
            {
                float distance = Vector3.Distance(transform.position, targetCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = targetCollider.transform;
                }
            }
        }
        return closestTarget;
    }
}