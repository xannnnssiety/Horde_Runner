using UnityEngine;
using System.Collections;
using UnityEngine.VFX; // Добавляем пространство имен для VFX Graph

[RequireComponent(typeof(SphereCollider))]
public class AuraDamageSkill : BaseSkill // !!! Наследуемся от BaseSkill
{
    [Header("Aura Base Settings")]
    public float baseDamage = 10;       // Базовый урон
    public float baseTickRate = 1.0f;   // Базовая частота
    public float baseRadius = 5.0f;     // Базовый радиус
    public LayerMask enemyLayerMask;

    [Header("Visuals (Optional)")]
    [Tooltip("Ссылка на Visual Effect Graph компонент для ауры")]
    [SerializeField] private VisualEffect vfxAura; // Сюда перетащите ваш VFX

    // Текущие, расчетные значения
    private float currentDamage;
    private float currentRadius;

    private SphereCollider auraCollider;
    private Coroutine damageCoroutine;

    void Awake() // Используем Awake для получения компонентов
    {
        auraCollider = GetComponent<SphereCollider>();
        auraCollider.isTrigger = true;
    }

    // OnEnable теперь в BaseSkill, но мы можем его расширить, если нужно
    protected override void OnEnable()
    {
        base.OnEnable(); // Вызываем базовую логику подписки
        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DamageTickCoroutine());
        }
    }

    // OnDisable тоже
    protected override void OnDisable()
    {
        base.OnDisable(); // Вызываем базовую логику отписки
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    // !!! ГЛАВНЫЙ МЕТОД, РЕАЛИЗУЮЩИЙ АБСТРАКЦИЮ
    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;

        // 1. Получаем глобальные множители
        float areaMult = PlayerStatsManager.Instance.areaMultiplier;
        float damageMult = PlayerStatsManager.Instance.damageMultiplier;

        // 2. Рассчитываем текущие параметры скилла
        currentRadius = baseRadius * (1f + areaMult);
        currentDamage = baseDamage * (1f + damageMult);

        // 3. Применяем параметры к компонентам игры
        auraCollider.radius = currentRadius;

        // 4. ОБНОВЛЯЕМ ВИЗУАЛ!
        UpdateVisuals();

        // Debug.Log($"Aura stats updated: Radius={currentRadius}, Damage={currentDamage}");
    }

    private void UpdateVisuals()
    {
        // --- ИНТЕГРАЦИЯ С VFX GRAPH ---
        if (vfxAura != null)
        {
            // У VFX Graph в инспекторе должна быть "Exposed" переменная типа Float с именем "AuraRadius".
            // Мы устанавливаем ее значение из кода.
            vfxAura.SetFloat("AuraRadius", currentRadius);
        }

        // --- ПРИМЕР ИНТЕГРАЦИИ С SHADER GRAPH (через материал) ---
        /*
        Renderer visualRenderer = GetComponentInChildren<Renderer>();
        if (visualRenderer != null)
        {
            // Создаем MaterialPropertyBlock для эффективности
            var propertyBlock = new MaterialPropertyBlock();
            visualRenderer.GetPropertyBlock(propertyBlock);
            // У шейдера должна быть переменная (Reference) с именем "_AuraSize"
            propertyBlock.SetFloat("_AuraSize", currentRadius);
            visualRenderer.SetPropertyBlock(propertyBlock);
        }
        */
    }

    private IEnumerator DamageTickCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(baseTickRate); // Тикрейт пока не меняем, но можно добавить
            DealDamageInAura();
        }
    }

    private void DealDamageInAura()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentRadius, enemyLayerMask);

        int damageToDeal = Mathf.RoundToInt(currentDamage); // Урон - целое число

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<EnemyAI>(out EnemyAI groundEnemy))
            {
                groundEnemy.TakeDamage(damageToDeal);
                totalDamageDealt += damageToDeal;
            }
            else if (hitCollider.TryGetComponent<ProjectileEnemyAI>(out ProjectileEnemyAI swarmEnemy))
            {
                swarmEnemy.TakeDamage(damageToDeal);
                totalDamageDealt += damageToDeal;
            }
        }
    }
}