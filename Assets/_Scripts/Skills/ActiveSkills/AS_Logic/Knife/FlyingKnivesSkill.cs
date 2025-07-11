using UnityEngine;

/// <summary>
/// ��������� ������ ��� ��������� ������ "������� ����".
/// ����������� �� �������� ������ ActiveSkill.
/// </summary>
public class FlyingKnivesSkill : ActiveSkill
{
    [Header("��������� '������� �����'")]
    public GameObject knifeProjectilePrefab;

    private Transform[] _firePoints;

    public override void Initialize(ActiveSkillData data, PlayerStats stats)
    {
        base.Initialize(data, stats);

        Transform playerRoot = playerStats.transform.parent;
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

    /// <summary>
    /// ���������� �������� ������ ������.
    /// </summary>
    public override void Activate()
    {
        if (knifeProjectilePrefab == null || _firePoints == null || _firePoints.Length == 0)
        {
            Debug.LogWarning("FlyingKnivesSkill: �� ���� ������������, �� �������� ������ ������� ��� �� ������� fire points.", this);
            return;
        }

        // --- ���������: ���������� ���� ��� �������� ���������� �������� ---
        // ���� ����� ����������� ������� ���, ������� ������� � 'currentAmount'.
        for (int i = 0; i < currentAmount; i++)
        {
            int randomIndex = Random.Range(0, _firePoints.Length);
            Transform spawnPoint = _firePoints[randomIndex];

            GameObject knifeObject = Instantiate(knifeProjectilePrefab, spawnPoint.position, spawnPoint.rotation);

            // --- ���������: ��������� ����� � ������� �������� (�������) ---
            // �� ������ ����������� ������ ������ �������.
            knifeObject.transform.localScale *= currentAreaOfEffect;

            ProjectileMover mover = knifeObject.GetComponent<ProjectileMover>();

            if (mover != null)
            {
                mover.Initialize(currentProjectileSpeed, currentDuration);
            }
            else
            {
                Debug.LogWarning("�� ������� ������� ����������� ��������� ProjectileMover!", knifeObject);
            }
        }
    }

    /// <summary>
    /// ��������������� ����� ��� ������ ��������� ������� �� ����� ������� �����������.
    /// </summary>
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
}