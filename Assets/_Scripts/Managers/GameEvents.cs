using System;

public static class GameEvents
{
    // Старое событие, которое просто сообщает "враг умер"
    public static event Action OnEnemyDied;

    // --- ИЗМЕНЕНИЕ ---
    // Новое событие, которое сообщает "счетчик убийств изменился" и передает новый итог.
    public static event Action<int> OnKillCountChanged;

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
}