using UnityEngine;
using UnityEngine.UI; // ��� TMPro;

public class KillCounterUI : MonoBehaviour
{
    // ������ �� ��������� ���������
    public Text killCountText; // ��� public TMPro.TextMeshProUGUI killCountText;

    // ������� ��� ������
    public string prefixText = "Kills: ";

    void Update()
    {
        // ���������, ���������� �� ��������
        if (RunStatsManager.Instance != null)
        {
            // ��������� �����, ��������� ������ �� ���������
            killCountText.text = prefixText + RunStatsManager.Instance.totalKills;
        }
    }
}