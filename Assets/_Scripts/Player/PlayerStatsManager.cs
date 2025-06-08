using System;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    // --- ���������� ������������ ������ ---
    // �������� ��� ���������. 0.1f = +10%

    [Header("Global Stat Multipliers")]
    [Tooltip("��������� ��� �������, ������� ��������. 0.1 = +10% Area")]
    public float areaMultiplier = 0f;

    [Tooltip("��������� ��� ������� ��������/��������. 0.1 = +10% Size")]
    public float sizeMultiplier = 0f;

    [Tooltip("��������� ��� �����. 0.1 = +10% Damage")]
    public float damageMultiplier = 0f;

    // ... ����� ����� ����� �������� duration, cooldown, amount � �.�.

    // �������, ������� ��������� ��� ������ � ���, ��� ����� ����������.
    // ��� �������� ������� �������.
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
            ResetStats(); // ���������� ����� � ������ ������� ������
        }
    }

    // ����� ��� ������ ������ (��������, � ������ ����� ����)
    public void ResetStats()
    {
        areaMultiplier = 0f;
        sizeMultiplier = 0f;
        damageMultiplier = 0f;

        // ��������� ����������� � ������
        OnStatsChanged?.Invoke();
    }

    // --- ������ ��� ���������� ������� ---
    // (�� �� ������ ��������, ����� ����� �������� ���������)

    public void AddAreaBonus(float percentage)
    {
        areaMultiplier += percentage;
        Debug.Log($"Area bonus added: {percentage * 100}%. New multiplier: {areaMultiplier}");
        OnStatsChanged?.Invoke(); // ��������� ��� ������!
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
}