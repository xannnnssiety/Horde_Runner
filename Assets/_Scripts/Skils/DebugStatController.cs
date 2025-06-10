using UnityEngine;

// ���� ��������� ������������ ������������� ��� �������.
// �� ������ ���������� �� ��� �� �������, ��� � PlayerStatsManager.
public class DebugStatController : MonoBehaviour
{
    [Header("��������� �����")]
    [Tooltip("�� ������� ��������� ����������� ����� �� ���� �������")]
    [SerializeField] private float percentIncrement = 0.1f; // 0.1f = 10%

    [Tooltip("�� ������� ����������� ���������� �� ���� �������")]
    [SerializeField] private int amountIncrement = 1;

    // ������ �� �������� ������
    private PlayerStatsManager statsManager;

    void Awake()
    {
        // ������� PlayerStatsManager �� ���� �� �������
        statsManager = GetComponent<PlayerStatsManager>();
        if (statsManager == null)
        {
            Debug.LogError("DebugStatController �� ����� ����� PlayerStatsManager �� ���� �������!");
            enabled = false; // ��������� ������, ���� ��������� ���
        }
    }

    void Update()
    {
        // ���������, ��� �������� ������ ����������, ������ ��� ��� ������������
        if (statsManager == null) return;

        // --- ���������� ����� ---
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            statsManager.AddDamageBonus(percentIncrement);
        }

        // --- ���������� �������/������� ---
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            statsManager.AddAreaBonus(percentIncrement);
        }

        // --- ���������� ������� ---
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            statsManager.AddSizeBonus(percentIncrement);
        }

        // --- ��������� ����������� ---
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            statsManager.AddCooldownBonus(percentIncrement);
        }

        // --- ���������� ���������� ---
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            statsManager.AddAmountBonus(amountIncrement);
        }

        // --- ������ ��� ������ ���� ������ ---
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("--- ����� �������� (DEBUG) ---");
            statsManager.ResetStats();
        }
    }
}