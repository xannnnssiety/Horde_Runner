using UnityEngine;
using System.Collections.Generic;

public class OrbitalSkill : BaseSkill // Наследуем от BaseSkill
{
    [Header("Orbital Base Settings")]
    public GameObject orbitalPrefab; // Префаб нашего "лезвия"
    public float baseDamage = 8;
    public float baseRotationSpeed = 100f; // Градусов в секунду
    public float baseOrbitRadius = 3f;   // Радиус орбиты
    public int baseAmount = 1;           // Количество лезвий
    public float baseSize = 1f;          // Размер лезвий
    public float baseHitCooldown = 0.5f; // КД на удар для каждого лезвия
    public LayerMask enemyLayerMask;

    // Расчетные значения
    private int currentDamage;
    private float currentRotationSpeed;
    private float currentOrbitRadius;
    private int currentAmount;
    private float currentSize;

    // Список активных лезвий
    private List<GameObject> activeOrbitals = new List<GameObject>();

    // OnEnable/OnDisable уже есть в BaseSkill, они подпишут нас на статы.
    // Нам нужно лишь реализовать логику обновления.



    void Update()
    {
        // Вращаем этот объект, а лезвия будут вращаться вместе с ним как дочерние
        transform.Rotate(Vector3.up, currentRotationSpeed * Time.deltaTime);
    }

    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;

        var stats = PlayerStatsManager.Instance;
        float speedMult = stats.projectileSpeedMultiplier;

        // Рассчитываем текущие параметры
        currentDamage = Mathf.RoundToInt(baseDamage * (1f + stats.damageMultiplier));
        currentAmount = baseAmount + stats.amountBonus;
        currentSize = baseSize * (1f + stats.sizeMultiplier);
        currentOrbitRadius = baseOrbitRadius * (1f + stats.areaMultiplier);
        currentRotationSpeed = baseRotationSpeed * (1f + speedMult);



        // Обновляем состояние лезвий
        UpdateOrbitals();
    }

    private void UpdateOrbitals()
    {
        // 1. Если нужно больше лезвий, создаем их
        while (activeOrbitals.Count < currentAmount)
        {
            GameObject newOrbital = Instantiate(orbitalPrefab, transform);
            activeOrbitals.Add(newOrbital);
        }
        // 2. Если нужно меньше лезвий, удаляем лишние
        while (activeOrbitals.Count > currentAmount && activeOrbitals.Count > 0)
        {
            GameObject toRemove = activeOrbitals[activeOrbitals.Count - 1];
            activeOrbitals.RemoveAt(activeOrbitals.Count - 1);
            Destroy(toRemove);
        }

        // 3. Обновляем позицию, размер и параметры каждого лезвия
        for (int i = 0; i < activeOrbitals.Count; i++)
        {
            GameObject orbital = activeOrbitals[i];

            // Распределяем лезвия равномерно по кругу
            float angle = i * (360f / activeOrbitals.Count);
            Vector3 localPos = Quaternion.Euler(0, angle, 0) * Vector3.forward * currentOrbitRadius;
            orbital.transform.localPosition = localPos;

            // Обновляем размер
            orbital.transform.localScale = Vector3.one * currentSize;

            // Инициализируем/обновляем параметры на лезвии
            if (orbital.TryGetComponent<OrbitalObject>(out var orbitalLogic))
            {
                orbitalLogic.Initialize(this, currentDamage, baseHitCooldown, enemyLayerMask);
            }
        }
    }

    // При выключении скилла уничтожаем все созданные объекты
    protected override void OnDisable()
    {
        base.OnDisable(); // Важно вызвать базовый метод
        foreach (var orbital in activeOrbitals)
        {
            if (orbital != null) Destroy(orbital);
        }
        activeOrbitals.Clear();
    }
}