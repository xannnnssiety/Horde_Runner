using System;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    // --- Глобальные Модификаторы Статов ---
    // Хранятся как множители. 0.1f = +10%

    [Header("Global Stat Multipliers")]
    [Tooltip("Множитель для радиуса, области действия. 0.1 = +10% Area")]
    public float areaMultiplier = 0f;

    [Tooltip("Множитель для размера снарядов/эффектов. 0.1 = +10% Size")]
    public float sizeMultiplier = 0f;

    [Tooltip("Множитель для урона. 0.1 = +10% Damage")]
    public float damageMultiplier = 0f;

    [Tooltip("Множитель для скорости перезарядки. 0.1 = +10% Cooldown Speed (уменьшает время)")]
    public float cooldownMultiplier = 0f;

    [Tooltip("Дополнительное количество снарядов/объектов.")]
    public int amountBonus = 0;

    [Tooltip("Множитель для скорости снарядов. 0.1 = +10% Speed")]
    public float projectileSpeedMultiplier = 0f;

    // ... здесь можно будет добавить duration, cooldown, amount и т.д.

    // Событие, которое оповещает все скиллы о том, что статы изменились.
    // Это КЛЮЧЕВОЙ элемент системы.
    public static event Action OnStatsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            ResetStats(); // Сбрасываем статы в начале каждого забега
        }
    }

    // Метод для сброса статов (например, в начале новой игры)
    public void ResetStats()
    {
        areaMultiplier = 0f;
        sizeMultiplier = 0f;
        damageMultiplier = 0f;
        cooldownMultiplier = 0f;
        amountBonus = 0;
        projectileSpeedMultiplier = 0f;

        // Оповещаем подписчиков о сбросе
        OnStatsChanged?.Invoke();
    }

    // --- Методы для добавления бонусов ---
    // (Их вы будете вызывать, когда игрок выбирает улучшение)

    public void AddAreaBonus(float percentage)
    {
        areaMultiplier += percentage;
        Debug.Log($"Area bonus added: {percentage * 100}%. New multiplier: {areaMultiplier}");
        OnStatsChanged?.Invoke(); // Оповещаем все скиллы!
    }

    public void AddSizeBonus(float percentage)
    {
        sizeMultiplier += percentage;
        Debug.Log($"Size bonus added: {percentage * 100}%. New multiplier: {sizeMultiplier}");
        OnStatsChanged?.Invoke();
    }

    public void AddDamageBonus(float percentage)
    {
        damageMultiplier += percentage;
        Debug.Log($"Damage bonus added: {percentage * 100}%. New multiplier: {damageMultiplier}");
        OnStatsChanged?.Invoke();
    }

    public void AddCooldownBonus(float percentage)
    {
        cooldownMultiplier += percentage;
        Debug.Log($"Cooldown bonus added: {percentage * 100}%. New multiplier: {cooldownMultiplier}");
        OnStatsChanged?.Invoke();
    }

    public void AddAmountBonus(int amount)
    {
        amountBonus += amount;
        Debug.Log($"Amount bonus added: {amount}. New bonus: {amountBonus}");
        OnStatsChanged?.Invoke();
    }

    public void AddProjectileSpeedBonus(float percentage)
    {
        projectileSpeedMultiplier += percentage;
        Debug.Log($"Projectile Speed bonus added: {percentage * 100}%. New multiplier: {projectileSpeedMultiplier}");
        OnStatsChanged?.Invoke();
    }

}