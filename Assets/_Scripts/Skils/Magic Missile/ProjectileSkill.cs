using UnityEngine;
using System.Collections;
using System.Linq;

public class ProjectileSkill : BaseSkill
{
    [Header("Projectile Base Settings")]
    public GameObject projectilePrefab; // ���� ���������� ��� ������
    public float baseDamage = 15;
    public float baseCooldown = 2.0f;
    public float baseProjectileSpeed = 20f;
    public int baseAmount = 1; // ���-�� �������� �� ���
    public float baseProjectileSize = 1f; // ������� ������ �������
    public float baseLifetime = 5f; // ������� ����� �����
    public LayerMask enemyLayerMask;
    public float searchRadius = 50f; // ������ ������ ������

    // �������, ��������� ��������
    private float currentDamage;
    private float currentCooldown;
    private float currentProjectileSpeed;
    private int currentAmount;
    private float currentProjectileSize;

    [Tooltip("�����, �� ������� ����� �������� �������. ���� �� �������, ������������ ������� ����� �������.")]
    public Transform firePoint;


    void Start()
    {
        // ��������� ����������� ���� ��������
        StartCoroutine(FireCoroutine());
    }

    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;

        // �������� ���������� ���������
        var stats = PlayerStatsManager.Instance;
        float damageMult = stats.damageMultiplier;
        float sizeMult = stats.sizeMultiplier;
        // !!! ��� ����������� ����� ��������� � PlayerStatsManager
        float cooldownMult = stats.cooldownMultiplier; // ������������, ��� �� ����
        int amountBonus = stats.amountBonus; // ������������, ��� �� ����

        // ������������ ������� ���������
        currentDamage = baseDamage * (1f + damageMult);
        currentCooldown = baseCooldown / (1f + cooldownMult); // ������� �����������!
        currentAmount = baseAmount + amountBonus;
        currentProjectileSize = baseProjectileSize * (1f + sizeMult);

        // �������� � ����� ����� ���� �� ������, �� ����� �������� ��������� � ��� ���
        currentProjectileSpeed = baseProjectileSpeed;
    }

    private IEnumerator FireCoroutine()
    {
        while (true)
        {
            // ���� �����������
            yield return new WaitForSeconds(currentCooldown);

            // ���� ����
            Collider[] allTargets = Physics.OverlapSphere(transform.position, searchRadius, enemyLayerMask);

            if (allTargets.Length > 0)
            {
                // ������� ��������� ����
                Transform closestTarget = null;
                float minDistance = float.MaxValue;

                foreach (var targetCollider in allTargets)
                {
                    if (targetCollider.TryGetComponent<EnemyAI>(out _) || targetCollider.TryGetComponent<ProjectileEnemyAI>(out _))
                    {
                        float distance = Vector3.Distance(transform.position, targetCollider.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestTarget = targetCollider.transform;
                        }
                    }
                }

                // ���� ��������� ���� �������, ��������� � ��� ��� �������
                if (closestTarget != null)
                {
                    for (int i = 0; i < currentAmount; i++)
                    {

                        StartCoroutine(FireBurstCoroutine(currentAmount, closestTarget));
                        

                    }
                }
            }
        }
    }

    private IEnumerator FireBurstCoroutine(int amountToFire, Transform target)
    {
        // ��� �������� "���������" ���������� �������� (amountToFire) � ������ ������ �������.
        // ���� ���� currentAmount ���������, ���� ��� ��������, ��� �������� ����� ������� ��������,
        // ������� �� �������.
        for (int i = 0; i < amountToFire; i++)
        {
            // ���������, ���������� �� ���� �� ��� ���, ����� ������ ���������
            if (target != null && target.gameObject.activeInHierarchy)
            {
                FireProjectile(target);
                // ���� �������� ����� ���������� � �������
                yield return new WaitForSeconds(0.05f);
            }
            else
            {
                // ���� ���� ������� � �������� �������, ������ ���������� ��������
                yield break;
            }
        }
    }

    private void FireProjectile(Transform target)
    {
        if (projectilePrefab == null) return;

        // ���������� ����� ������
        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;
        // ���������� ��������� �������� - ����� ������ ������� �� ���� �����
        Quaternion initialRotation = Quaternion.LookRotation(target.position - spawnPosition);

        GameObject projectileGO = Instantiate(projectilePrefab, spawnPosition, initialRotation);

        if (projectileGO.TryGetComponent<SkillProjectile>(out SkillProjectile projectile))
        {
            int damageToDeal = Mathf.RoundToInt(currentDamage);
            // �������� ���� � ������, ����� �� ����, ���� ������
            projectile.Initialize(damageToDeal, currentProjectileSpeed, currentProjectileSize, target, enemyLayerMask, baseLifetime);
        }
    }
}