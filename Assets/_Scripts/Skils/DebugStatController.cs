using UnityEngine;

// Этот компонент предназначен исключительно для отладки.
// Он должен находиться на том же объекте, что и PlayerStatsManager.
public class DebugStatController : MonoBehaviour
{
    [Header("Настройки теста")]
    [Tooltip("На сколько процентов увеличивать статы за одно нажатие")]
    [SerializeField] private float percentIncrement = 0.1f; // 0.1f = 10%

    [Tooltip("На сколько увеличивать количество за одно нажатие")]
    [SerializeField] private int amountIncrement = 1;

    // Ссылка на менеджер статов
    private PlayerStatsManager statsManager;

    void Awake()
    {
        // Находим PlayerStatsManager на этом же объекте
        statsManager = GetComponent<PlayerStatsManager>();
        if (statsManager == null)
        {
            Debug.LogError("DebugStatController не может найти PlayerStatsManager на этом объекте!");
            enabled = false; // Выключаем скрипт, если менеджера нет
        }
    }

    void Update()
    {
        // Проверяем, что менеджер статов существует, прежде чем его использовать
        if (statsManager == null) return;

        // --- Увеличение урона ---
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            statsManager.AddDamageBonus(percentIncrement);
        }

        // --- Увеличение области/радиуса ---
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            statsManager.AddAreaBonus(percentIncrement);
        }

        // --- Увеличение размера ---
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            statsManager.AddSizeBonus(percentIncrement);
        }

        // --- Ускорение перезарядки ---
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            statsManager.AddCooldownBonus(percentIncrement);
        }

        // --- Увеличение количества ---
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            statsManager.AddAmountBonus(amountIncrement);
        }

        // --- Кнопка для сброса всех статов ---
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("--- СТАТЫ СБРОШЕНЫ (DEBUG) ---");
            statsManager.ResetStats();
        }
    }
}