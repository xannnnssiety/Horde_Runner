using UnityEngine;
using UnityEngine.UI; // или TMPro;

public class KillCounterUI : MonoBehaviour
{
    // Ссылка на текстовый компонент
    public Text killCountText; // или public TMPro.TextMeshProUGUI killCountText;

    // Префикс для текста
    public string prefixText = "Kills: ";

    void Update()
    {
        // Проверяем, существует ли менеджер
        if (RunStatsManager.Instance != null)
        {
            // Обновляем текст, используя данные из менеджера
            killCountText.text = prefixText + RunStatsManager.Instance.totalKills;
        }
    }
}