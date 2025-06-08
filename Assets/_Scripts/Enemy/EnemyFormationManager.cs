using UnityEngine;
using System.Collections.Generic;

public class EnemyFormationManager : MonoBehaviour
{
    public static EnemyFormationManager Instance { get; private set; }

    [Header("Target")]
    public Transform playerTransform; // �����, ������ �������� �������� ��������

    [Header("Formation Settings")]
    public int slotsPerRow = 5;         // ���������� ������ � ����� ���� ��������
    public int numberOfRows = 3;        // ���������� ����� � ��������
    public float slotSpacing = 2.5f;    // ���������� ����� ������� � ����
    public float rowSpacing = 2.0f;     // ���������� ����� ������
    public float formationOffsetZ = -5f; // �������� ���� �������� ����� �� ������ (������������� ��������)
    public float followSpeed = 5f;      // ��������, � ������� ����� �������� ������� �� ������� (��� ���������)

    private List<FormationSlot> formationSlots = new List<FormationSlot>();
    private Vector3 currentFormationCenter; // ������� ����� ��������, ������� ������ ������� �� �������

    // ����� ��� ������������� ����� � ��������
    public class FormationSlot
    {
        public Vector3 localOffset; // �������� ������������ ������ ��������
        public EnemyAI assignedEnemy = null;
        public bool isOccupied => assignedEnemy != null;

        public FormationSlot(Vector3 offset)
        {
            localOffset = offset;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // ���� �������� ������ ������������ ����� �������
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFormation();
    }

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
            else Debug.LogError("EnemyFormationManager: Player Transform �� �������� � �� ������ �� ���� 'Player'!");
        }
        if (playerTransform != null)
        {
            currentFormationCenter = CalculateTargetFormationCenter();
        }
    }

    void InitializeFormation()
    {
        formationSlots.Clear();
        float formationWidth = (slotsPerRow - 1) * slotSpacing;

        for (int r = 0; r < numberOfRows; r++)
        {
            for (int c = 0; c < slotsPerRow; c++)
            {
                float xOffset = (c * slotSpacing) - (formationWidth / 2f);
                float zOffset = r * -rowSpacing; // ���� ���� �����
                formationSlots.Add(new FormationSlot(new Vector3(xOffset, 0, zOffset)));
            }
        }
        Debug.Log($"���������������� {formationSlots.Count} ������ � ��������.");
    }

    void Update()
    {
        if (playerTransform == null) return;

        // ������� ���������� ������ �������� �� �������
        Vector3 targetFormationCenter = CalculateTargetFormationCenter();
        currentFormationCenter = Vector3.Lerp(currentFormationCenter, targetFormationCenter, Time.deltaTime * followSpeed);
    }

    Vector3 CalculateTargetFormationCenter()
    {
        if (playerTransform == null) return Vector3.zero;
        // ����� �������� ������ ����� �� ������ � ������� �� ��� �������������� ���������� � ������������
        Vector3 offset = playerTransform.forward * formationOffsetZ;
        return playerTransform.position + offset;
    }

    public Vector3 GetWorldPositionForSlot(FormationSlot slot)
    {
        if (playerTransform == null) return Vector3.zero;
        // ������������ ��������� �������� ����� � ������������ � ��������� ������ (��� ��������)
        // � ��������� � �������� �������� ������ ��������.
        // ��� �������� ���� ����� �������, ��� �������� ������ ������������� ��� ��, ��� �����.
        Quaternion formationRotation = playerTransform.rotation; // ���������� �������� = ���������� ������
        return currentFormationCenter + (formationRotation * slot.localOffset);
    }

    public FormationSlot RequestSlot(EnemyAI enemy)
    {
        foreach (FormationSlot slot in formationSlots)
        {
            if (!slot.isOccupied)
            {
                slot.assignedEnemy = enemy;
                Debug.Log($"���� {slot.localOffset} �������� ����� {enemy.name}");
                return slot;
            }
        }
        Debug.LogWarning($"��� ��������� ������ � �������� ��� {enemy.name}.");
        return null; // ��� ��������� ������
    }

    public void ReleaseSlot(EnemyAI enemy)
    {
        foreach (FormationSlot slot in formationSlots)
        {
            if (slot.assignedEnemy == enemy)
            {
                slot.assignedEnemy = null;
                Debug.Log($"���� {slot.localOffset} ���������� ������ {enemy.name}");
                return;
            }
        }
    }

    // ��� �������: ��������� ������ ��������
    void OnDrawGizmos()
    {
        if (playerTransform == null && currentFormationCenter == Vector3.zero) return; // �� ��������, ���� ��� ������ � ����� �� ����������

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(currentFormationCenter, 0.5f); // ����� ��������

        if (formationSlots.Count > 0)
        {
            foreach (FormationSlot slot in formationSlots)
            {
                Vector3 worldPos = GetWorldPositionForSlot(slot); // ���������� ������� ����� ��� Gizmos
                Gizmos.color = slot.isOccupied ? Color.red : Color.green;
                Gizmos.DrawWireSphere(worldPos, 0.5f);
            }
        }
    }
}