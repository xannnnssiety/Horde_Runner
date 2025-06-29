using UnityEngine;
using System.Collections;

public class KnifeSkill : BaseSkill
{
    [Header("Knife Skill Settings")]
    public GameObject knifePrefab;
    [Tooltip("������ �� ��������� ������ ������, ����� �����, ���� '������'")]
    public Transform playerTransform; // ���� ���������� ������ Player �� ��������
    [Tooltip("�����, �� ������� ����� �������� ����")]
    public Transform[] firePoints; // ���� ���������� FirePoint_Left � FirePoint_Right

    [Header("Base Stats")]
    public float baseDamage = 20;
    public float baseCooldown = 1.5f; // ������� ����� �������
    public int baseAmount = 1; // ���������� ����� � ����� �����
    [Tooltip("������� �������� �����, ������� ����� ������������ � �������� ������")]
    public float baseProjectileSpeed = 30f; // ��� ������ '����������' ��������
    [Tooltip("�� ������� ��� ��� ������� ������� �������� ������")]
    public float playerSpeedFactor = 2.0f; // ��������� ��������
    public float baseProjectileSize = 1f;
    public float lifetime = 1f; // ����� ����� ���� � ��������
    public float delayBetweenShots = 0.1f; // �������� ����� ������ � ����� �����
    public LayerMask enemyLayerMask;

    // ��������� ��������
    private int currentDamage;
    private float currentCooldown;
    private int currentAmount;
    private float currentProjectileSize;
    private float currentBaseSpeed; // ��������� ������� �������� ����
    private PlayerMovement playerMovement; // <-- ������ �� ������ ������������

    void Start()
    {

        // �������� ��������� ������������ � ������� ������
        if (playerTransform != null)
        {
            playerMovement = playerTransform.GetComponent<PlayerMovement>();
        }
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script �� ������ �� ������� Player Transform!", this);
        }

        StartCoroutine(FireCycleCoroutine());
    }

    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;
        var stats = PlayerStatsManager.Instance;

        currentDamage = Mathf.RoundToInt(baseDamage * (1f + stats.damageMultiplier));
        currentAmount = baseAmount + stats.amountBonus;
        currentCooldown = baseCooldown / (1f + stats.cooldownMultiplier);
        currentProjectileSize = baseProjectileSize * (1f + stats.sizeMultiplier);

        // ������������, ��� � PlayerStatsManager ���� ��������� �������� ��������
        // ���� ���, ����� �������� �� �������� � ������� �������.
        float speedMult = stats.projectileSpeedMultiplier;
        currentBaseSpeed = baseProjectileSpeed * (1f + speedMult);
    }

    // �������� ����, ���������� �� ������� ����� �������
    private IEnumerator FireCycleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentCooldown);
            StartCoroutine(FireVolleyCoroutine());
        }
    }

    // ��������������� ����, ���������� �� ������ ������ ����� � ����������
    private IEnumerator FireVolleyCoroutine()
    {
        if (playerTransform == null || firePoints.Length == 0 || playerMovement == null)
        {
            yield break;
        }

        for (int i = 0; i < currentAmount; i++)
        {
            Transform spawnPoint = firePoints[Random.Range(0, firePoints.Length)];

            // --- ����� ������ ������� �������� ---
            // �������� ���� = ��� ������� �������� + (�������� ������ * ���������)
            float finalSpeed = currentBaseSpeed + (playerMovement.currentMoveSpeed * playerSpeedFactor);

            GameObject knifeGO = Instantiate(knifePrefab, spawnPoint.position, playerTransform.rotation);

            if (knifeGO.TryGetComponent<LinearProjectile>(out var projectile))
            {
                // �������� � ��� ��� ���������, ������������ ��������
                projectile.Initialize(this, currentDamage, finalSpeed, currentProjectileSize, enemyLayerMask, lifetime);
            }

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }
}