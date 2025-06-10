using UnityEngine;
using System.Collections;
using System.Linq;

public class ProjectileSkill : BaseSkill
{
    [Header("Projectile Base Settings")]
    public GameObject projectilePrefab; // Сюда перетащить наш префаб
    public float baseDamage = 15;
    public float baseCooldown = 2.0f;
    public float baseProjectileSpeed = 20f;
    public int baseAmount = 1; // Кол-во снарядов за раз
    public float baseProjectileSize = 1f; // Базовый размер снаряда
    public float baseLifetime = 5f; // Базовое время жизни
    public LayerMask enemyLayerMask;
    public float searchRadius = 50f; // Радиус поиска врагов

    // Текущие, расчетные значения
    private float currentDamage;
    private float currentCooldown;
    private float currentProjectileSpeed;
    private int currentAmount;
    private float currentProjectileSize;

    [Tooltip("Точка, из которой будут вылетать снаряды. Если не указана, используется позиция этого объекта.")]
    public Transform firePoint;


    void Start()
    {
        // Запускаем бесконечный цикл стрельбы
        StartCoroutine(FireCoroutine());
    }

    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;

        // Получаем глобальные множители
        var stats = PlayerStatsManager.Instance;
        float damageMult = stats.damageMultiplier;
        float sizeMult = stats.sizeMultiplier;
        // !!! Нам понадобятся новые множители в PlayerStatsManager
        float cooldownMult = stats.cooldownMultiplier; // Предполагаем, что он есть
        int amountBonus = stats.amountBonus; // Предполагаем, что он есть

        // Рассчитываем текущие параметры
        currentDamage = baseDamage * (1f + damageMult);
        currentCooldown = baseCooldown / (1f + cooldownMult); // Кулдаун уменьшается!
        currentAmount = baseAmount + amountBonus;
        currentProjectileSize = baseProjectileSize * (1f + sizeMult);

        // Скорость и время жизни пока не меняем, но можно добавить множители и для них
        currentProjectileSpeed = baseProjectileSpeed;
    }

    private IEnumerator FireCoroutine()
    {
        while (true)
        {
            // Ждем перезарядки
            yield return new WaitForSeconds(currentCooldown);

            // Ищем цели
            Collider[] allTargets = Physics.OverlapSphere(transform.position, searchRadius, enemyLayerMask);

            if (allTargets.Length > 0)
            {
                // Находим ближайшую цель
                Transform closestTarget = null;
                float minDistance = float.MaxValue;

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

                // Если ближайшая цель найдена, выпускаем в нее все снаряды
                if (closestTarget != null)
                {
                    for (int i = 0; i < currentAmount; i++)
                    {

                        StartCoroutine(FireBurstCoroutine(currentAmount, closestTarget));
                        

                    }
                }
            }
        }
    }

    private IEnumerator FireBurstCoroutine(int amountToFire, Transform target)
    {
        // Эта корутина "запомнила" количество снарядов (amountToFire) в момент своего запуска.
        // Даже если currentAmount изменится, пока она работает, она выпустит ровно столько снарядов,
        // сколько ей сказали.
        for (int i = 0; i < amountToFire; i++)
        {
            // Проверяем, существует ли цель до сих пор, перед каждым выстрелом
            if (target != null && target.gameObject.activeInHierarchy)
            {
                FireProjectile(target);
                // Ваша задержка между выстрелами в очереди
                yield return new WaitForSeconds(0.05f);
            }
            else
            {
                // Если цель исчезла в середине очереди, просто прекращаем стрельбу
                yield break;
            }
        }
    }

    private void FireProjectile(Transform target)
    {
        if (projectilePrefab == null) return;

        // Определяем точку спавна
        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;
        // Определяем начальное вращение - пусть снаряд смотрит на цель сразу
        Quaternion initialRotation = Quaternion.LookRotation(target.position - spawnPosition);

        GameObject projectileGO = Instantiate(projectilePrefab, spawnPosition, initialRotation);

        if (projectileGO.TryGetComponent<SkillProjectile>(out SkillProjectile projectile))
        {
            int damageToDeal = Mathf.RoundToInt(currentDamage);
            // Передаем цель в снаряд, чтобы он знал, куда лететь
            projectile.Initialize(damageToDeal, currentProjectileSpeed, currentProjectileSize, target, enemyLayerMask, baseLifetime);
        }
    }
}