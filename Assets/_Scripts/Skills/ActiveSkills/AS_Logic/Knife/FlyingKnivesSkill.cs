using UnityEngine;

/// <summary>
/// Реализует логику для активного умения "Летящие ножи".
/// Наследуется от базового класса ActiveSkill.
/// </summary>
public class FlyingKnivesSkill : ActiveSkill
{
    [Header("Настройки 'Летящих ножей'")]
    public GameObject knifeProjectilePrefab;

    private Transform[] _firePoints;

    public override void Initialize(ActiveSkillData data, PlayerStats stats)
    {
        base.Initialize(data, stats);

        Transform playerRoot = playerStats.transform.parent;
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

    /// <summary>
    /// Реализация основной логики умения.
    /// </summary>
    public override void Activate()
    {
        if (knifeProjectilePrefab == null || _firePoints == null || _firePoints.Length == 0)
        {
            Debug.LogWarning("FlyingKnivesSkill: Не могу активировать, не назначен префаб снаряда или не найдены fire points.", this);
            return;
        }

        // --- ИЗМЕНЕНИЕ: Используем цикл для создания нескольких снарядов ---
        // Цикл будет выполняться столько раз, сколько указано в 'currentAmount'.
        for (int i = 0; i < currentAmount; i++)
        {
            int randomIndex = Random.Range(0, _firePoints.Length);
            Transform spawnPoint = _firePoints[randomIndex];

            GameObject knifeObject = Instantiate(knifeProjectilePrefab, spawnPoint.position, spawnPoint.rotation);

            // --- ИЗМЕНЕНИЕ: Применяем бонус к области действия (размеру) ---
            // Мы просто увеличиваем размер самого снаряда.
            knifeObject.transform.localScale *= currentAreaOfEffect;

            ProjectileMover mover = knifeObject.GetComponent<ProjectileMover>();

            if (mover != null)
            {
                mover.Initialize(currentProjectileSpeed, currentDuration);
            }
            else
            {
                Debug.LogWarning("На префабе снаряда отсутствует компонент ProjectileMover!", knifeObject);
            }
        }
    }

    /// <summary>
    /// Вспомогательный метод для поиска дочернего объекта на любой глубине вложенности.
    /// </summary>
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
}