using UnityEngine;
using UnityEngine.UI; // ����������� ���, ���� ������� ������� UI Text
// using TMPro; // ����������� ���, ���� ������� TextMeshPro - Text, � ��������������� UnityEngine.UI

public class SpeedDisplayUI : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement; // ������ �� ������ PlayerMovement
    public Text speedTextLabel;           // ������ �� ��������� UI Text
    // public TextMeshProUGUI speedTextLabelTMP; // ����������� ��� ������ ������ �������, ���� ����������� TextMeshPro

    [Header("Settings")]
    public string prefixText = "Speed: "; // �����, ������� ����� ������������ ����� ��������� ��������
    public string formatString = "F1";    // ������ ��� ����������� ����� (F1 = 1 ���� ����� �������, F2 = 2 ����� � �.�.)

    void Start()
    {
        // ������� ����� PlayerMovement, ���� �� �� �������� � ����������
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("SpeedDisplayUI: PlayerMovement ������ �� ������ �� ����� � �� ��������!");
                enabled = false; // ��������� ������, ���� ��� ������
                return;
            }
        }

        // ��������, �������� �� ��������� ���������
#if UNITY_EDITOR // ���� ���� ����� �������� ������ � ���������
        if (speedTextLabel == null
            // && speedTextLabelTMP == null // ���������������� ��� �����, ���� ����������� TextMeshPro
            )
        {
            Debug.LogError("SpeedDisplayUI: ��������� ��������� (Text ��� TextMeshProUGUI) �� �������� � ����������!");
            enabled = false; // ��������� ������
            return;
        }
#endif
    }

    void Update()
    {
        if (playerMovement == null) return; // �������������� �������� �� ������, ���� ����� ���������

        // �������� ������� ��������
        float currentSpeed = playerMovement.currentMoveSpeed;

        // ��������� �����
        if (speedTextLabel != null)
        {
            speedTextLabel.text = prefixText + currentSpeed.ToString(formatString) + " km/h";
        }
        /* // ���������������� ���� ����, ���� ����������� TextMeshPro, � ��������������� ���� ����
        else if (speedTextLabelTMP != null)
        {
            speedTextLabelTMP.text = prefixText + currentSpeed.ToString(formatString);
        }
        */
    }
}