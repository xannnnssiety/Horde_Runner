using UnityEngine;
using UnityEngine.UI; // <-- ���������: ���������� ����������� ������������ ���� UI
using System.Text;
using System.Linq;

[RequireComponent(typeof(Text))] // <-- ���������: ������� ��������� Text, � �� TextMeshProUGUI
public class DebugStatsDisplay : MonoBehaviour
{
    [Header("������")]
    [Tooltip("������ �� ��������� PlayerStats. ������ �������������, ���� �� ����� ���� �����.")]
    [SerializeField] private PlayerStats playerStats;

    private Text _debugText; // <-- ���������: ��� ���������� ������ Text
    private StringBuilder _stringBuilder = new StringBuilder();

    void Awake()
    {
        _debugText = GetComponent<Text>(); // <-- ���������: �������� ��������� Text

        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats == null)
        {
            _debugText.text = "������: PlayerStats �� ������ �� �����!";
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
        _stringBuilder.AppendLine("--- ������� ����� ---");

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