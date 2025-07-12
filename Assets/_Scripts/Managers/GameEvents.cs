using System;
using UnityEngine;

public static class GameEvents
{
    // Старое событие, которое просто сообщает "враг умер"
    public static event Action OnEnemyDied;
    public static event Action<int> OnKillCountChanged;

    [Tooltip("Событие, которое срабатывает, когда игрок использует активное умение.")]
    public static event Action<ActiveSkillData> OnPlayerAbilityUsed;

    public static event Action<Vector3, Quaternion> OnDashStarted;
    public static void ReportEnemyDied()
    {
        OnEnemyDied?.Invoke();
    }

    // --- ИЗМЕНЕНИЕ ---
    // Метод для вызова нового события
    public static void ReportKillCountChanged(int newTotalKills)
    {
        OnKillCountChanged?.Invoke(newTotalKills);
    }

    /// <summary>
    /// Этот метод должен вызываться ActiveSkillManager'ом в момент использования умения.
    /// Он передает данные об использованном умении всем подписчикам.
    /// </summary>
    /// <param name="skillData">Данные использованного умения.</param>
    public static void ReportPlayerAbilityUsed(ActiveSkillData skillData)
    {
        OnPlayerAbilityUsed?.Invoke(skillData);
    }

    public static void ReportDashStarted(Vector3 startPosition, Quaternion startRotation)
    {
        OnDashStarted?.Invoke(startPosition, startRotation);
    }

}