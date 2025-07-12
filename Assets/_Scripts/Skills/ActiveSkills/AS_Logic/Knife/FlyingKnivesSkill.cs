using UnityEngine;
using System.Collections; // Необходимо для использования корутин (IEnumerator)

/// <summary>
/// Реализует логику для активного умения "Летящие ножи".
/// Наследуется от базового класса ActiveSkill.
/// </summary>
public class FlyingKnivesSkill : ActiveSkill
{
    [Header("Настройки 'Летящих ножей'")]
    public GameObject knifeProjectilePrefab;

    [Header("Настройки поиска цели")]
    public float searchRadius = 7f;
    public float searchAngle = 75f;

    // --- НОВЫЙ РАЗДЕЛ НАСТРОЕК ---
    [Header("Настройки залпа")]
    [Tooltip("Задержка между выстрелами в одном залпе (в секундах).")]
    public float volleyDelay = 0.05f;

    private Transform[] _firePoints;
    private bool _isShadowClone = false; // Флаг, чтобы знать, является ли этот экземпляр клоном

    public override void Initialize(ActiveSkillData data, PlayerStats stats, float effectiveness = 1.0f)
    {
        base.Initialize(data, stats, effectiveness);
        Transform playerRoot = this.playerStats.transform.parent;
        Transform firePointL = FindDeepChild(playerRoot, "FirePointKnife_Left");
        Transform firePointR = FindDeepChild(playerRoot, "FirePointKnife_Right");

        if (firePointL != null && firePointR != null)
        {
            _firePoints = new Transform[] { firePointL, firePointR };
        }
        else
        {
            Debug.LogError("FlyingKnivesSkill: Не удалось найти дочерние объекты FirePointKnife_Left или FirePointKnife_Right на объекте игрока!", this);
        }
    }

    public void SetFirePointSource(Transform source)
    {
        _isShadowClone = true; // Помечаем, что мы - клон
        // Ищем fire points на переданном источнике (на тени)
        Transform firePointL = FindDeepChild(source, "FirePointKnife_Left");
        Transform firePointR = FindDeepChild(source, "FirePointKnife_Right");
        if (firePointL != null && firePointR != null)
        {
            _firePoints = new Transform[] { firePointL, firePointR };
        }
    }

    /// <summary>
    /// Метод Activate теперь не создает ножи напрямую, а запускает корутину для их последовательного выпуска.
    /// </summary>
    public override void Activate()
    {
        // Если fire points еще не установлены (значит, мы - реальный скилл, а не клон)
        if (_firePoints == null)
        {
            // Выполняем поиск на реальном игроке один раз
            Transform playerRoot = playerStats.transform.parent;
            Transform firePointL = FindDeepChild(playerRoot, "FirePointKnife_Left");
            Transform firePointR = FindDeepChild(playerRoot, "FirePointKnife_Right");
            if (firePointL != null && firePointR != null)
            {
                _firePoints = new Transform[] { firePointL, firePointR };
            }
        }

        if (knifeProjectilePrefab == null || _firePoints == null || _firePoints.Length == 0) return;

        Transform nearestEnemy = FindNearestEnemyInCone();
        StartCoroutine(FireVolley(nearestEnemy));
    }

    /// <summary>
    /// Корутина, которая выпускает весь залп ножей с задержкой между ними.
    /// </summary>
    private IEnumerator FireVolley(Transform target)
    {
        for (int i = 0; i < currentAmount; i++)
        {
            int randomIndex = Random.Range(0, _firePoints.Length);
            Transform spawnPoint = _firePoints[randomIndex];

            Vector3 targetDirection;
            if (target != null)
            {
                targetDirection = (target.position - spawnPoint.position).normalized;
            }
            else
            {
                targetDirection = spawnPoint.forward;
            }

            Quaternion projectileRotation = Quaternion.LookRotation(targetDirection);
            GameObject knifeObject = Instantiate(knifeProjectilePrefab, spawnPoint.position, projectileRotation);

            knifeObject.transform.localScale *= currentAreaOfEffect;

            ProjectileMover mover = knifeObject.GetComponent<ProjectileMover>();
            if (mover != null)
            {
                mover.Initialize(currentProjectileSpeed, currentDuration);
            }

            // --- КЛЮЧЕВОЕ ИЗМЕНЕНИЕ ---
            // Делаем паузу перед следующей итерацией цикла.
            yield return new WaitForSeconds(volleyDelay);
        }

        if (_isShadowClone)
        {
            Destroy(gameObject);
        }

    }

    /// <summary>
    /// Вспомогательный метод для поиска цели. Вынесен из Activate для чистоты кода.
    /// </summary>
    private Transform FindNearestEnemyInCone()
    {
        Collider[] hits = Physics.OverlapSphere(playerStats.transform.position, searchRadius);
        Transform nearestEnemy = null;
        float minDistance = float.MaxValue;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 directionToEnemy = (hit.transform.position - playerStats.transform.position).normalized;
                if (Vector3.Angle(playerStats.transform.forward, directionToEnemy) < searchAngle / 2)
                {
                    float distance = Vector3.Distance(playerStats.transform.position, hit.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestEnemy = hit.transform;
                    }
                }
            }
        }
        return nearestEnemy;
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform result = FindDeepChild(child, childName);
            if (result != null) return result;
        }
        return null;
    }
}