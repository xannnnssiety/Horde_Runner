using UnityEngine;
using UnityEngine.UI; // <-- ИЗМЕНЕНИЕ: Используем стандартное пространство имен UI
using System.Text;
using System.Linq;

[RequireComponent(typeof(Text))] // <-- ИЗМЕНЕНИЕ: Требуем компонент Text, а не TextMeshProUGUI
public class DebugStatsDisplay : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Ссылка на компонент PlayerStats. Найдет автоматически, если на сцене один игрок.")]
    [SerializeField] private PlayerStats playerStats;

    private Text _debugText; // <-- ИЗМЕНЕНИЕ: Тип переменной теперь Text
    private StringBuilder _stringBuilder = new StringBuilder();

    void Awake()
    {
        _debugText = GetComponent<Text>(); // <-- ИЗМЕНЕНИЕ: Получаем компонент Text

        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats == null)
        {
            _debugText.text = "ОШИБКА: PlayerStats не найден на сцене!";
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatChanged += OnStatChanged_UpdateDisplay;
            UpdateFullDisplay();
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatChanged -= OnStatChanged_UpdateDisplay;
        }
    }

    private void OnStatChanged_UpdateDisplay(StatType type, float newValue)
    {
        UpdateFullDisplay();
    }

    private void Update()
    {
        UpdateFullDisplay();
    }

    private void UpdateFullDisplay()
    {
        _stringBuilder.Clear();
        _stringBuilder.AppendLine("--- ТЕКУЩИЕ БАФФЫ ---");

        var statTypes = (StatType[])System.Enum.GetValues(typeof(StatType));
        System.Array.Sort(statTypes, (x, y) => x.ToString().CompareTo(y.ToString()));

        foreach (StatType type in statTypes)
        {
            float value = playerStats.GetStat(type);
            _stringBuilder.AppendLine($"{type}: {value:F2}");
        }

        _debugText.text = _stringBuilder.ToString();
    }
}