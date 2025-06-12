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
    [Tooltip("Задержка между снарядами в одном залпе")]
    public float delayBetweenShots = 0.08f; // <-- Добавил в инспектор для удобства
    [Tooltip("Точка, из которой будут вылетать снаряды")]
    public Transform firePoint;

    // Расчетные значения
    private int currentDamage;
    private float currentCooldown;
    private float currentProjectileSpeed;
    private int currentAmount;
    private float currentProjectileSize;

    void Start()
    {
        // Запускаем основной цикл, отвечающий за кулдаун между ЗАЛПАМИ
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

        // --- ИСПРАВЛЕНИЕ ЗДЕСЬ ---
        // Теперь скорость снарядов корректно учитывает бонус
        currentProjectileSpeed = baseProjectileSpeed * (1f + speedMult);
    }

    // Этот цикл ждет основной кулдаун, находит цель и запускает один залп
    private IEnumerator FireCycleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentCooldown);

            Transform closestTarget = FindClosestEnemy();

            if (closestTarget != null)
            {
                // Запускаем корутину одного залпа, передавая ей цель
                StartCoroutine(FireVolleyCoroutine(closestTarget));
            }
        }
    }

    // Эта корутина отвечает за выпуск одного полного залпа в выбранную цель
    private IEnumerator FireVolleyCoroutine(Transform target)
    {
        for (int i = 0; i < currentAmount; i++)
        {
            // Перед каждым выстрелом проверяем, жива ли еще цель
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                yield break; // Если цель умерла, прекращаем залп
            }

            FireProjectile(target);

            // Ждем небольшую задержку перед запуском следующего снаряда
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    // Метод для выстрела одним снарядом
    private void FireProjectile(Transform target)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;
        Quaternion initialRotation = Quaternion.LookRotation(target.position - spawnPosition);

        GameObject projectileGO = Instantiate(projectilePrefab, spawnPosition, initialRotation);

        if (projectileGO.TryGetComponent<SkillProjectile>(out var projectile))
        {
            // --- ИЗМЕНЕНИЕ ЗДЕСЬ ---
            // Теперь передаем и 'this' для отчета об уроне
            projectile.Initialize(this, currentDamage, currentProjectileSpeed, currentProjectileSize, target, enemyLayerMask, baseLifetime);
        }
    }

    // Вспомогательный метод для поиска врага (чтобы не загромождать корутину)
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