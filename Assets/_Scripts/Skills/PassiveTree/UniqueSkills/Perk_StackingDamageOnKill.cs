using System.Collections.Generic;
using UnityEngine;

public class Perk_StackingDamageOnKill : MonoBehaviour
{
    [Header("Настройки перка")]
    [Tooltip("На сколько будет увеличиваться плоский урон за одно убийство.")]
    public float damageIncreasePerKill = 0.001f;

    private PlayerStats _playerStats;
    private GameManager _gameManager; // Ссылка на GameManager для получения начальных данных
    private StatModifier _currentModifier;

    void Awake()
    {
        _playerStats = GetComponentInParent<PlayerStats>();
        _gameManager = FindObjectOfType<GameManager>(); // Находим GameManager на сцене

        if (_playerStats == null || _gameManager == null)
        {
            Debug.LogError("Perk_StackingDamageOnKill не смог найти PlayerStats или GameManager!", this);
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        // 1. При активации перка сразу же применяем бонус за ВСЕХ ранее убитых врагов.
        UpdateDamageBonus(_gameManager.CurrentSaveData.totalKills);

        // 2. Подписываемся на событие, чтобы получать обновления счетчика в реальном времени.
        GameEvents.OnKillCountChanged += UpdateDamageBonus;
    }

    void OnDisable()
    {
        // Отписываемся от события
        GameEvents.OnKillCountChanged -= UpdateDamageBonus;

        // Удаляем наш модификатор со статов при деактивации/сбросе перка
        if (_playerStats != null && _currentModifier != null)
        {
            RemoveCurrentModifier();
        }
    }

    /// <summary>
    /// Этот метод обновляет бонус к урону на основе общего числа убийств.
    /// </summary>
    /// <param name="totalKills">Новое общее количество убийств.</param>
    private void UpdateDamageBonus(int totalKills)
    {
        // Сначала удаляем старый модификатор, если он был
        if (_currentModifier != null)
        {
            RemoveCurrentModifier();
        }

        // Создаем новый модификатор с актуальным бонусом
        _currentModifier = new StatModifier(
            StatType.Damage,
            totalKills * damageIncreasePerKill,
            ModifierType.Flat,
            SkillTag.Everything
        );

        // Применяем его
        ApplyCurrentModifier();
    }

    private void ApplyCurrentModifier()
    {
        var tempPassive = new PassiveSkillData { modifiers = new List<StatModifier> { _currentModifier } };
        _playerStats.ApplyPassive(tempPassive);
    }

    private void RemoveCurrentModifier()
    {
        var tempPassive = new PassiveSkillData { modifiers = new List<StatModifier> { _currentModifier } };
        _playerStats.RemovePassive(tempPassive);
    }





}