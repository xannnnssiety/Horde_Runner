using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // ������� ��� �������� ���� ������ ���������.
    // ���� - ��� ����� (enum), �������� - ������ ������ Stat.
    private readonly Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();

    // �������, ������� ����� ����������� ��� ��������� �����.
    // ������� ��� ���������� UI.
    public event Action<StatType, float> OnStatChanged;

    

    private void Awake()
    {
        // �������������� ��� ����� �� �������� ����������.
        // ��� �������� ����� ������� � ScriptableObject � �������� ����������� ���������.
        InitializeStats();
    }

    public void InitializeAndReset()
    {
        _stats.Clear(); // �� ������ ������ ������� �������
        InitializeStats(); // ��������� ��� �������� ����������
        Debug.Log("PlayerStats ������� ��������������� � �������.");
    }


    private void InitializeStats()
    {
        _stats.Add(StatType.MaxHealth, new Stat(100)); // 100% �������� ��������
        _stats.Add(StatType.MoveSpeed, new Stat(60)); // 60 flat ������� �������� ������������
        _stats.Add(StatType.Damage, new Stat(100)); // 100% �������� �����
        _stats.Add(StatType.AreaOfEffect, new Stat(100)); // 100% �������� �������
        _stats.Add(StatType.Cooldown, new Stat(100)); // 100% ������� �����������
        _stats.Add(StatType.ProjectileSpeed, new Stat(100)); // 100% ������� ��������
        _stats.Add(StatType.Amount, new Stat(1)); // 1 flat �������� ���������� ��������
        _stats.Add(StatType.PickupRadius, new Stat(10)); // 10 flat �������� ������� �������
        _stats.Add(StatType.ExperienceGain, new Stat(100)); // 100% �������� ��������� �����
        _stats.Add(StatType.CurrencyGain, new Stat(100)); // 100% �������� ��������� ������
        _stats.Add(StatType.Armor, new Stat(0)); // 0 flat ������� ����� 
        _stats.Add(StatType.Duration, new Stat(100)); // 100% ������� ������������ ������
        _stats.Add(StatType.Luck, new Stat(0)); // 0 flat ������� ����� (����� ������ �� ���� ����� � ��������� ������)
        _stats.Add(StatType.RicochetChance, new Stat(0)); // ������� ���� 0%
        _stats.Add(StatType.RicochetCount, new Stat(0));  // ������� ���������� �������� 0
        // ... �������� ���� ��� ��������� ����� �� ������ enum StatType
    }

    /// <summary>
    /// ���������� ��������� �������� ���������� �����.
    /// </summary>
    public float GetStat(StatType type)
    {
        if (_stats.TryGetValue(type, out Stat stat))
        {
            return stat.Value;
        }
        Debug.LogWarning($"���� ���� {type} �� ������!");
        return 0;
    }

    /// <summary>
    /// ��������� ��� ������������ �� ���������� ������.
    /// </summary>
    public void ApplyPassive(PassiveSkillData passive)
    {
        foreach (var modifier in passive.modifiers)
        {
            if (_stats.TryGetValue(modifier.Stat, out Stat stat))
            {
                stat.AddModifier(modifier);
                // �������� ���� �����������, ��� ���� ���������
                OnStatChanged?.Invoke(modifier.Stat, stat.Value);
                Debug.Log($"�������� �����������: {modifier.Stat} ������� �� {stat.Value}");
            }
        }
    }

    public void ResetToDefaults()
    {
        // �������� �� ������� ����� � �������
        foreach (var statEntry in _stats)
        {
            // �������� � ���� ����� ������� �������������
            statEntry.Value.ClearModifiers();
            // �������� ����, ��� ���� ��������� (�������� � �������� ��������)
            OnStatChanged?.Invoke(statEntry.Key, statEntry.Value.Value);
        }
        Debug.Log("��� ����� ������ �������� �� ������� ��������.");
    }

    /// <summary>
    /// ������� ��� ������������ �� ���������� ������ (������� ��� ������).
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