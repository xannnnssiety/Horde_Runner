using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Словарь для хранения всех статов персонажа.
    // Ключ - тип стата (enum), значение - объект класса Stat.
    private readonly Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();

    // Событие, которое будет срабатывать при изменении стата.
    // Полезно для обновления UI.
    public event Action<StatType, float> OnStatChanged;

  

    private void Awake()
    {
        // Инициализируем все статы их базовыми значениями.
        // Эти значения можно вынести в ScriptableObject с базовыми настройками персонажа.
        InitializeStats();
    }


    private void InitializeStats()
    {
        _stats.Add(StatType.MaxHealth, new Stat(100));
        _stats.Add(StatType.MoveSpeed, new Stat(5));
        _stats.Add(StatType.Damage, new Stat(100)); // 100% базового урона
        _stats.Add(StatType.AreaOfEffect, new Stat(100)); // 100% базового радиуса
        _stats.Add(StatType.Cooldown, new Stat(100)); // 100% базовой перезарядки
        _stats.Add(StatType.ProjectileSpeed, new Stat(100)); // 100% базовой скорости
        // ... добавьте сюда все остальные статы из вашего enum StatType
    }

    /// <summary>
    /// Возвращает финальное значение указанного стата.
    /// </summary>
    public float GetStat(StatType type)
    {
        if (_stats.TryGetValue(type, out Stat stat))
        {
            return stat.Value;
        }
        Debug.LogWarning($"Стат типа {type} не найден!");
        return 0;
    }

    /// <summary>
    /// Применяет все модификаторы из пассивного навыка.
    /// </summary>
    public void ApplyPassive(PassiveSkillData passive)
    {
        foreach (var modifier in passive.modifiers)
        {
            if (_stats.TryGetValue(modifier.Stat, out Stat stat))
            {
                stat.AddModifier(modifier);
                // Сообщаем всем подписчикам, что стат изменился
                OnStatChanged?.Invoke(modifier.Stat, stat.Value);
                Debug.Log($"Применен модификатор: {modifier.Stat} изменен на {stat.Value}");
            }
        }
    }

    /// <summary>
    /// Снимает все модификаторы от пассивного навыка (полезно для сброса).
    /// </summary>
    public void RemovePassive(PassiveSkillData passive)
    {
        foreach (var modifier in passive.modifiers)
        {
            if (_stats.TryGetValue(modifier.Stat, out Stat stat))
            {
                stat.RemoveModifier(modifier);
                OnStatChanged?.Invoke(modifier.Stat, stat.Value);
            }
        }
    }
}