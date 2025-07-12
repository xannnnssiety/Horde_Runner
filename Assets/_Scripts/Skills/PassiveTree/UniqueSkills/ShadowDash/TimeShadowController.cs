using UnityEngine;

public class TimeShadowController : MonoBehaviour
{
    [Header("��������� ����")]
    public float lifetime = 1.5f;
    public float effectiveness = 0.4f;

    private PlayerStats _realPlayerStats;

    void Start()
    {
        _realPlayerStats = FindObjectOfType<PlayerStats>();
        if (_realPlayerStats == null)
        {
            Destroy(gameObject);
            return;
        }

        GameEvents.OnPlayerAbilityUsed += HandlePlayerAbilityUsed;
        Destroy(gameObject, lifetime);
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerAbilityUsed -= HandlePlayerAbilityUsed;
    }

    private void HandlePlayerAbilityUsed(ActiveSkillData skillData)
    {
        GameObject skillObject = Instantiate(skillData.skillLogicPrefab, transform);
        ActiveSkill skillInstance = skillObject.GetComponent<ActiveSkill>();

        if (skillInstance != null)
        {
            skillInstance.Initialize(skillData, _realPlayerStats, effectiveness);

            // --- ���������: ���������, �������� �� ��� ����� ������� ����� ---
            // ���� ��, "�����������" ��� ������������ fire points ����.
            if (skillInstance is FlyingKnivesSkill knivesSkill)
            {
                knivesSkill.SetFirePointSource(this.transform);
            }

            skillInstance.Activate();
        }

        // --- ���������: ������� ������ Destroy(skillObject, 0.1f); ---
        // ������ ������ ������ ���� ����������� � ����� �����������.
    }
}