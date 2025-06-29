using UnityEngine;
using System.Collections.Generic;

public class OrbitalSkill : BaseSkill // ��������� �� BaseSkill
{
    [Header("Orbital Base Settings")]
    public GameObject orbitalPrefab; // ������ ������ "������"
    public float baseDamage = 8;
    public float baseRotationSpeed = 100f; // �������� � �������
    public float baseOrbitRadius = 3f;   // ������ ������
    public int baseAmount = 1;           // ���������� ������
    public float baseSize = 1f;          // ������ ������
    public float baseHitCooldown = 0.5f; // �� �� ���� ��� ������� ������
    public LayerMask enemyLayerMask;

    // ��������� ��������
    private int currentDamage;
    private float currentRotationSpeed;
    private float currentOrbitRadius;
    private int currentAmount;
    private float currentSize;

    // ������ �������� ������
    private List<GameObject> activeOrbitals = new List<GameObject>();

    // OnEnable/OnDisable ��� ���� � BaseSkill, ��� �������� ��� �� �����.
    // ��� ����� ���� ����������� ������ ����������.



    void Update()
    {
        // ������� ���� ������, � ������ ����� ��������� ������ � ��� ��� ��������
        transform.Rotate(Vector3.up, currentRotationSpeed * Time.deltaTime);
    }

    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;

        var stats = PlayerStatsManager.Instance;
        float speedMult = stats.projectileSpeedMultiplier;

        // ������������ ������� ���������
        currentDamage = Mathf.RoundToInt(baseDamage * (1f + stats.damageMultiplier));
        currentAmount = baseAmount + stats.amountBonus;
        currentSize = baseSize * (1f + stats.sizeMultiplier);
        currentOrbitRadius = baseOrbitRadius * (1f + stats.areaMultiplier);
        currentRotationSpeed = baseRotationSpeed * (1f + speedMult);



        // ��������� ��������� ������
        UpdateOrbitals();
    }

    private void UpdateOrbitals()
    {
        // 1. ���� ����� ������ ������, ������� ��
        while (activeOrbitals.Count < currentAmount)
        {
            GameObject newOrbital = Instantiate(orbitalPrefab, transform);
            activeOrbitals.Add(newOrbital);
        }
        // 2. ���� ����� ������ ������, ������� ������
        while (activeOrbitals.Count > currentAmount && activeOrbitals.Count > 0)
        {
            GameObject toRemove = activeOrbitals[activeOrbitals.Count - 1];
            activeOrbitals.RemoveAt(activeOrbitals.Count - 1);
            Destroy(toRemove);
        }

        // 3. ��������� �������, ������ � ��������� ������� ������
        for (int i = 0; i < activeOrbitals.Count; i++)
        {
            GameObject orbital = activeOrbitals[i];

            // ������������ ������ ���������� �� �����
            float angle = i * (360f / activeOrbitals.Count);
            Vector3 localPos = Quaternion.Euler(0, angle, 0) * Vector3.forward * currentOrbitRadius;
            orbital.transform.localPosition = localPos;

            // ��������� ������
            orbital.transform.localScale = Vector3.one * currentSize;

            // ��������������/��������� ��������� �� ������
            if (orbital.TryGetComponent<OrbitalObject>(out var orbitalLogic))
            {
                orbitalLogic.Initialize(this, currentDamage, baseHitCooldown, enemyLayerMask);
            }
        }
    }

    // ��� ���������� ������ ���������� ��� ��������� �������
    protected override void OnDisable()
    {
        base.OnDisable(); // ����� ������� ������� �����
        foreach (var orbital in activeOrbitals)
        {
            if (orbital != null) Destroy(orbital);
        }
        activeOrbitals.Clear();
    }
}