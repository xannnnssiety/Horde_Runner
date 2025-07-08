using System.Collections.Generic;
using UnityEngine;

public class Perk_StackingDamageOnKill : MonoBehaviour
{
    [Header("��������� �����")]
    [Tooltip("�� ������� ����� ������������� ������� ���� �� ���� ��������.")]
    public float damageIncreasePerKill = 0.001f;

    private PlayerStats _playerStats;
    private GameManager _gameManager; // ������ �� GameManager ��� ��������� ��������� ������
    private StatModifier _currentModifier;

    void Awake()
    {
        _playerStats = GetComponentInParent<PlayerStats>();
        _gameManager = FindObjectOfType<GameManager>(); // ������� GameManager �� �����

        if (_playerStats == null || _gameManager == null)
        {
            Debug.LogError("Perk_StackingDamageOnKill �� ���� ����� PlayerStats ��� GameManager!", this);
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        // 1. ��� ��������� ����� ����� �� ��������� ����� �� ���� ����� ������ ������.
        UpdateDamageBonus(_gameManager.CurrentSaveData.totalKills);

        // 2. ������������� �� �������, ����� �������� ���������� �������� � �������� �������.
        GameEvents.OnKillCountChanged += UpdateDamageBonus;
    }

    void OnDisable()
    {
        // ������������ �� �������
        GameEvents.OnKillCountChanged -= UpdateDamageBonus;

        // ������� ��� ����������� �� ������ ��� �����������/������ �����
        if (_playerStats != null && _currentModifier != null)
        {
            RemoveCurrentModifier();
        }
    }

    /// <summary>
    /// ���� ����� ��������� ����� � ����� �� ������ ������ ����� �������.
    /// </summary>
    /// <param name="totalKills">����� ����� ���������� �������.</param>
    private void UpdateDamageBonus(int totalKills)
    {
        // ������� ������� ������ �����������, ���� �� ���
        if (_currentModifier != null)
        {
            RemoveCurrentModifier();
        }

        // ������� ����� ����������� � ���������� �������
        _currentModifier = new StatModifier(
            StatType.Damage,
            totalKills * damageIncreasePerKill,
            ModifierType.Flat,
            SkillTag.Everything
        );

        // ��������� ���
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