using UnityEngine;
using UnityEngine.UI; // Обязательно добавьте эту строку для работы с TextMeshPro

/// <summary>
/// Временный скрипт для тестирования системы подсчета убийств.
/// При нажатии ЛКМ симулирует убийство врага.
/// Обновляет текстовое поле с текущим количеством убийств.
/// </summary>
public class TempKillCounter : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [Tooltip("Перетащите сюда текстовый объект TextMeshPro из вашей сцены")]
    public Text killCountText;

    private void OnEnable()
    {
        // Подписываемся на событие изменения счетчика, чтобы обновлять текст
        GameEvents.OnKillCountChanged += UpdateKillText;
    }

    private void OnDisable()
    {
        // Обязательно отписываемся, чтобы избежать ошибок
        GameEvents.OnKillCountChanged -= UpdateKillText;
    }

    void Update()
    {
        // Проверяем, была ли нажата левая кнопка мыши в этом кадре
        if (Input.GetMouseButtonDown(0))
        {
            // Симулируем убийство врага, вызывая глобальное событие.
            // GameManager услышит это событие и увеличит счетчик.
            Debug.Log("LMB Clicked! Simulating an enemy kill.");
            GameEvents.ReportEnemyDied();
        }
    }

    /// <summary>
    /// Этот метод вызывается событием OnKillCountChanged из GameEvents.
    /// </summary>
    /// <param name="newTotalKills">Новое общее количество убийств.</param>
    private void UpdateKillText(int newTotalKills)
    {
        // Проверяем, не забыли ли мы присвоить текстовое поле в инспекторе
        if (killCountText != null)
        {
            // Обновляем текст на экране
            killCountText.text = $"Total Kills: {newTotalKills}";
        }
    }
}