using UnityEngine;
using System.Collections;

public class KnifeSkill : BaseSkill
{
    [Header("Knife Skill Settings")]
    public GameObject knifePrefab;
    [Tooltip("Ссылка на трансформ самого игрока, чтобы знать, куда 'вперед'")]
    public Transform playerTransform; // Сюда перетащить объект Player из иерархии
    [Tooltip("Точки, из которых будут вылетать ножи")]
    public Transform[] firePoints; // Сюда перетащить FirePoint_Left и FirePoint_Right

    [Header("Base Stats")]
    public float baseDamage = 20;
    public float baseCooldown = 1.5f; // Кулдаун между залпами
    public int baseAmount = 1; // Количество ножей в одном залпе
    [Tooltip("Базовая скорость ножей, которая будет прибавляться к скорости игрока")]
    public float baseProjectileSpeed = 30f; // Это теперь 'добавочная' скорость
    [Tooltip("Во сколько раз нож быстрее текущей скорости игрока")]
    public float playerSpeedFactor = 2.0f; // Множитель скорости
    public float baseProjectileSize = 1f;
    public float lifetime = 1f; // Время жизни ножа в секундах
    public float delayBetweenShots = 0.1f; // Задержка между ножами в одном залпе
    public LayerMask enemyLayerMask;

    // Расчетные значения
    private int currentDamage;
    private float currentCooldown;
    private int currentAmount;
    private float currentProjectileSize;
    private float currentBaseSpeed; // Расчетная базовая скорость ножа
    private PlayerMovement playerMovement; // <-- Ссылка на скрипт передвижения

    void Start()
    {

        // Получаем компонент передвижения с объекта игрока
        if (playerTransform != null)
        {
            playerMovement = playerTransform.GetComponent<PlayerMovement>();
        }
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script не найден на объекте Player Transform!", this);
        }

        StartCoroutine(FireCycleCoroutine());
    }

    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;
        var stats = PlayerStatsManager.Instance;

        currentDamage = Mathf.RoundToInt(baseDamage * (1f + stats.damageMultiplier));
        currentAmount = baseAmount + stats.amountBonus;
        currentCooldown = baseCooldown / (1f + stats.cooldownMultiplier);
        currentProjectileSize = baseProjectileSize * (1f + stats.sizeMultiplier);

        // Предполагаем, что в PlayerStatsManager есть множитель скорости снарядов
        // Если нет, нужно добавить по аналогии с другими статами.
        float speedMult = stats.projectileSpeedMultiplier;
        currentBaseSpeed = baseProjectileSpeed * (1f + speedMult);
    }

    // Основной цикл, отвечающий за кулдаун между залпами
    private IEnumerator FireCycleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentCooldown);
            StartCoroutine(FireVolleyCoroutine());
        }
    }

    // Вспомогательный цикл, отвечающий за выпуск одного залпа с задержками
    private IEnumerator FireVolleyCoroutine()
    {
        if (playerTransform == null || firePoints.Length == 0 || playerMovement == null)
        {
            yield break;
        }

        for (int i = 0; i < currentAmount; i++)
        {
            Transform spawnPoint = firePoints[Random.Range(0, firePoints.Length)];

            // --- НОВАЯ ЛОГИКА РАСЧЕТА СКОРОСТИ ---
            // Скорость ножа = его базовая скорость + (скорость игрока * множитель)
            float finalSpeed = currentBaseSpeed + (playerMovement.currentMoveSpeed * playerSpeedFactor);

            GameObject knifeGO = Instantiate(knifePrefab, spawnPoint.position, playerTransform.rotation);

            if (knifeGO.TryGetComponent<LinearProjectile>(out var projectile))
            {
                // Передаем в нож уже финальную, рассчитанную скорость
                projectile.Initialize(this, currentDamage, finalSpeed, currentProjectileSize, enemyLayerMask, lifetime);
            }

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }
}