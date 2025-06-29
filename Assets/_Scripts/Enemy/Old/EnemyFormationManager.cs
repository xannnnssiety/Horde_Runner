using UnityEngine;
using System.Collections.Generic;

public class EnemyFormationManager : MonoBehaviour
{
    public static EnemyFormationManager Instance { get; private set; }

    [Header("Target")]
    public Transform playerTransform; // »грок, вокруг которого строитс€ формаци€

    [Header("Formation Settings")]
    public int slotsPerRow = 5;         //  оличество слотов в одном р€ду формации
    public int numberOfRows = 3;        //  оличество р€дов в формации
    public float slotSpacing = 2.5f;    // –ассто€ние между слотами в р€ду
    public float rowSpacing = 2.0f;     // –ассто€ние между р€дами
    public float formationOffsetZ = -5f; // —мещение всей формации назад от игрока (отрицательное значение)
    public float followSpeed = 5f;      // —корость, с которой центр формации следует за игроком (дл€ плавности)

    private List<FormationSlot> formationSlots = new List<FormationSlot>();
    private Vector3 currentFormationCenter; // “екущий центр формации, который плавно следует за игроком

    //  ласс дл€ представлени€ слота в формации
    public class FormationSlot
    {
        public Vector3 localOffset; // —мещение относительно центра формации
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
            // DontDestroyOnLoad(gameObject); // ≈сли менеджер должен существовать между сценами
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
            else Debug.LogError("EnemyFormationManager: Player Transform не назначен и не найден по тегу 'Player'!");
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
                float zOffset = r * -rowSpacing; // –€ды идут назад
                formationSlots.Add(new FormationSlot(new Vector3(xOffset, 0, zOffset)));
            }
        }
        Debug.Log($"»нициализировано {formationSlots.Count} слотов в формации.");
    }

    void Update()
    {
        if (playerTransform == null) return;

        // ѕлавное следование центра формации за игроком
        Vector3 targetFormationCenter = CalculateTargetFormationCenter();
        currentFormationCenter = Vector3.Lerp(currentFormationCenter, targetFormationCenter, Time.deltaTime * followSpeed);
    }

    Vector3 CalculateTargetFormationCenter()
    {
        if (playerTransform == null) return Vector3.zero;
        // ÷ентр формации смещен назад от игрока и следует за его горизонтальным положением и направлением
        Vector3 offset = playerTransform.forward * formationOffsetZ;
        return playerTransform.position + offset;
    }

    public Vector3 GetWorldPositionForSlot(FormationSlot slot)
    {
        if (playerTransform == null) return Vector3.zero;
        // ѕоворачиваем локальное смещение слота в соответствии с поворотом игрока (или формации)
        // и добавл€ем к текущему мировому центру формации.
        // ƒл€ простоты пока будем считать, что формаци€ всегда ориентирована так же, как игрок.
        Quaternion formationRotation = playerTransform.rotation; // ќриентаци€ формации = ориентаци€ игрока
        return currentFormationCenter + (formationRotation * slot.localOffset);
    }

    public FormationSlot RequestSlot(EnemyAI enemy)
    {
        foreach (FormationSlot slot in formationSlots)
        {
            if (!slot.isOccupied)
            {
                slot.assignedEnemy = enemy;
                Debug.Log($"—лот {slot.localOffset} назначен врагу {enemy.name}");
                return slot;
            }
        }
        Debug.LogWarning($"Ќет свободных слотов в формации дл€ {enemy.name}.");
        return null; // Ќет свободных слотов
    }

    public void ReleaseSlot(EnemyAI enemy)
    {
        foreach (FormationSlot slot in formationSlots)
        {
            if (slot.assignedEnemy == enemy)
            {
                slot.assignedEnemy = null;
                Debug.Log($"—лот {slot.localOffset} освобожден врагом {enemy.name}");
                return;
            }
        }
    }

    // ƒл€ отладки: отрисовка слотов формации
    void OnDrawGizmos()
    {
        if (playerTransform == null && currentFormationCenter == Vector3.zero) return; // Ќе рисовать, если нет игрока и центр не установлен

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(currentFormationCenter, 0.5f); // ÷ентр формации

        if (formationSlots.Count > 0)
        {
            foreach (FormationSlot slot in formationSlots)
            {
                Vector3 worldPos = GetWorldPositionForSlot(slot); // »спользуем текущий центр дл€ Gizmos
                Gizmos.color = slot.isOccupied ? Color.red : Color.green;
                Gizmos.DrawWireSphere(worldPos, 0.5f);
            }
        }
    }
}