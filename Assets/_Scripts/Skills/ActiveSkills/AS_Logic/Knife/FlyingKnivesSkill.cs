using UnityEngine;
using System.Collections; // ���������� ��� ������������� ������� (IEnumerator)

/// <summary>
/// ��������� ������ ��� ��������� ������ "������� ����".
/// ����������� �� �������� ������ ActiveSkill.
/// </summary>
public class FlyingKnivesSkill : ActiveSkill
{
    [Header("��������� '������� �����'")]
    public GameObject knifeProjectilePrefab;

    [Header("��������� ������ ����")]
    public float searchRadius = 7f;
    public float searchAngle = 75f;

    // --- ����� ������ �������� ---
    [Header("��������� �����")]
    [Tooltip("�������� ����� ���������� � ����� ����� (� ��������).")]
    public float volleyDelay = 0.05f;

    private Transform[] _firePoints;
    private bool _isShadowClone = false; // ����, ����� �����, �������� �� ���� ��������� ������

    public override void Initialize(ActiveSkillData data, PlayerStats stats, float effectiveness = 1.0f)
    {
        base.Initialize(data, stats, effectiveness);
        Transform playerRoot = this.playerStats.transform.parent;
        Transform firePointL = FindDeepChild(playerRoot, "FirePointKnife_Left");
        Transform firePointR = FindDeepChild(playerRoot, "FirePointKnife_Right");

        if (firePointL != null && firePointR != null)
        {
            _firePoints = new Transform[] { firePointL, firePointR };
        }
        else
        {
            Debug.LogError("FlyingKnivesSkill: �� ������� ����� �������� ������� FirePointKnife_Left ��� FirePointKnife_Right �� ������� ������!", this);
        }
    }

    public void SetFirePointSource(Transform source)
    {
        _isShadowClone = true; // ��������, ��� �� - ����
        // ���� fire points �� ���������� ��������� (�� ����)
        Transform firePointL = FindDeepChild(source, "FirePointKnife_Left");
        Transform firePointR = FindDeepChild(source, "FirePointKnife_Right");
        if (firePointL != null && firePointR != null)
        {
            _firePoints = new Transform[] { firePointL, firePointR };
        }
    }

    /// <summary>
    /// ����� Activate ������ �� ������� ���� ��������, � ��������� �������� ��� �� ����������������� �������.
    /// </summary>
    public override void Activate()
    {
        // ���� fire points ��� �� ����������� (������, �� - �������� �����, � �� ����)
        if (_firePoints == null)
        {
            // ��������� ����� �� �������� ������ ���� ���
            Transform playerRoot = playerStats.transform.parent;
            Transform firePointL = FindDeepChild(playerRoot, "FirePointKnife_Left");
            Transform firePointR = FindDeepChild(playerRoot, "FirePointKnife_Right");
            if (firePointL != null && firePointR != null)
            {
                _firePoints = new Transform[] { firePointL, firePointR };
            }
        }

        if (knifeProjectilePrefab == null || _firePoints == null || _firePoints.Length == 0) return;

        Transform nearestEnemy = FindNearestEnemyInCone();
        StartCoroutine(FireVolley(nearestEnemy));
    }

    /// <summary>
    /// ��������, ������� ��������� ���� ���� ����� � ��������� ����� ����.
    /// </summary>
    private IEnumerator FireVolley(Transform target)
    {
        for (int i = 0; i < currentAmount; i++)
        {
            int randomIndex = Random.Range(0, _firePoints.Length);
            Transform spawnPoint = _firePoints[randomIndex];

            Vector3 targetDirection;
            if (target != null)
            {
                targetDirection = (target.position - spawnPoint.position).normalized;
            }
            else
            {
                targetDirection = spawnPoint.forward;
            }

            Quaternion projectileRotation = Quaternion.LookRotation(targetDirection);
            GameObject knifeObject = Instantiate(knifeProjectilePrefab, spawnPoint.position, projectileRotation);

            knifeObject.transform.localScale *= currentAreaOfEffect;

            ProjectileMover mover = knifeObject.GetComponent<ProjectileMover>();
            if (mover != null)
            {
                mover.Initialize(currentProjectileSpeed, currentDuration);
            }

            // --- �������� ��������� ---
            // ������ ����� ����� ��������� ��������� �����.
            yield return new WaitForSeconds(volleyDelay);
        }

        if (_isShadowClone)
        {
            Destroy(gameObject);
        }

    }

    /// <summary>
    /// ��������������� ����� ��� ������ ����. ������� �� Activate ��� ������� ����.
    /// </summary>
    private Transform FindNearestEnemyInCone()
    {
        Collider[] hits = Physics.OverlapSphere(playerStats.transform.position, searchRadius);
        Transform nearestEnemy = null;
        float minDistance = float.MaxValue;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 directionToEnemy = (hit.transform.position - playerStats.transform.position).normalized;
                if (Vector3.Angle(playerStats.transform.forward, directionToEnemy) < searchAngle / 2)
                {
                    float distance = Vector3.Distance(playerStats.transform.position, hit.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestEnemy = hit.transform;
                    }
                }
            }
        }
        return nearestEnemy;
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform result = FindDeepChild(child, childName);
            if (result != null) return result;
        }
        return null;
    }
}