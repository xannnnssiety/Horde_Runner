using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // —ловарь дл€ хранени€ всех статов персонажа.
    //  люч - тип стата (enum), значение - объект класса Stat.
    private readonly Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();

    // —обытие, которое будет срабатывать при изменении стата.
    // ѕолезно дл€ обновлени€ UI.
    public event Action<StatType, float> OnStatChanged;

    

    private void Awake()
    {
        // »нициализируем все статы их базовыми значени€ми.
        // Ёти значени€ можно вынести в ScriptableObject с базовыми настройками персонажа.
        InitializeStats();
    }

    public void InitializeAndReset()
    {
        _stats.Clear(); // Ќа вс€кий случай очищаем словарь
        InitializeStats(); // «аполн€ем его базовыми значени€ми
        Debug.Log("PlayerStats успешно инициализирован и сброшен.");
    }


    private void InitializeStats()
    {
        _stats.Add(StatType.MaxHealth, new Stat(100)); // 100% базового здоровь€
        _stats.Add(StatType.MoveSpeed, new Stat(60)); // 60 flat базовой скорости передвижени€
        _stats.Add(StatType.Damage, new Stat(100)); // 100% базового урона
        _stats.Add(StatType.AreaOfEffect, new Stat(100)); // 100% базового радиуса
        _stats.Add(StatType.Cooldown, new Stat(100)); // 100% базовой перезар€дки
        _stats.Add(StatType.ProjectileSpeed, new Stat(100)); // 100% базовой скорости
        _stats.Add(StatType.Amount, new Stat(1)); // 1 flat базового количества снар€дов
        _stats.Add(StatType.PickupRadius, new Stat(10)); // 10 flat базового радиуса подбора
        _stats.Add(StatType.ExperienceGain, new Stat(100)); // 100% базового получени€ опыта
        _stats.Add(StatType.CurrencyGain, new Stat(100)); // 100% базового получени€ валюты
        _stats.Add(StatType.Armor, new Stat(0)); // 0 flat базовой брони 
        _stats.Add(StatType.Duration, new Stat(100)); // 100% базовой длительности умений
        _stats.Add(StatType.Luck, new Stat(0)); // 0 flat базовой удачи (может вли€ть на шанс крита и выпадение наград)
        _stats.Add(StatType.RicochetChance, new Stat(0)); // Ѕазовый шанс 0%
        _stats.Add(StatType.RicochetCount, new Stat(0));  // Ѕазовое количество отскоков 0
        // ... добавьте сюда все остальные статы из вашего enum StatType
    }

    /// <summary>
    /// ¬озвращает финальное значение указанного стата.
    /// </summary>
    public float GetStat(StatType type)
    {
        if (_stats.TryGetValue(type, out Stat stat))
        {
            return stat.Value;
        }
        Debug.LogWarning($"—тат типа {type} не найден!");
        return 0;
    }

    /// <summary>
    /// ѕримен€ет все модификаторы из пассивного навыка.
    /// </summary>
    public void ApplyPassive(PassiveSkillData passive)
    {
        foreach (var modifier in passive.modifiers)
        {
            if (_stats.TryGetValue(modifier.Stat, out Stat stat))
            {
                stat.AddModifier(modifier);
                // —ообщаем всем подписчикам, что стат изменилс€
                OnStatChanged?.Invoke(modifier.Stat, stat.Value);
                Debug.Log($"ѕрименен модификатор: {modifier.Stat} изменен на {stat.Value}");
            }
        }
    }

    public void ResetToDefaults()
    {
        // ѕроходим по каждому стату в словаре
        foreach (var statEntry in _stats)
        {
            // ¬ызываем у него метод очистки модификаторов
            statEntry.Value.ClearModifiers();
            // —ообщаем всем, что стат изменилс€ (вернулс€ к базовому значению)
            OnStatChanged?.Invoke(statEntry.Key, statEntry.Value.Value);
        }
        Debug.Log("¬се статы игрока сброшены до базовых значений.");
    }

    /// <summary>
    /// —нимает все модификаторы от пассивного навыка (полезно дл€ сброса).
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