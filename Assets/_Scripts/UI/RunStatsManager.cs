using UnityEngine;

public class RunStatsManager : MonoBehaviour
{
    // Синглтон для легкого доступа
    public static RunStatsManager Instance { get; private set; }

    // Статистика, которую мы отслеживаем
    public int totalKills { get; private set; }

    private void Awake()
    {
        // Классическая реализация синглтона
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Инициализируем/сбрасываем счетчики в начале
        totalKills = 0;
    }

    // Метод, который вызывают враги при смерти
    public void RegisterKill()
    {
        totalKills++;
    }
}