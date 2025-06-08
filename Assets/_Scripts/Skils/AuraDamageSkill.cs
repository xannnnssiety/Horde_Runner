using UnityEngine;
using System.Collections;
using UnityEngine.VFX; // ��������� ������������ ���� ��� VFX Graph

[RequireComponent(typeof(SphereCollider))]
public class AuraDamageSkill : BaseSkill // !!! ����������� �� BaseSkill
{
    [Header("Aura Base Settings")]
    public float baseDamage = 10;       // ������� ����
    public float baseTickRate = 1.0f;   // ������� �������
    public float baseRadius = 5.0f;     // ������� ������
    public LayerMask enemyLayerMask;

    [Header("Visuals (Optional)")]
    [Tooltip("������ �� Visual Effect Graph ��������� ��� ����")]
    [SerializeField] private VisualEffect vfxAura; // ���� ���������� ��� VFX

    // �������, ��������� ��������
    private float currentDamage;
    private float currentRadius;

    private SphereCollider auraCollider;
    private Coroutine damageCoroutine;

    void Awake() // ���������� Awake ��� ��������� �����������
    {
        auraCollider = GetComponent<SphereCollider>();
        auraCollider.isTrigger = true;
    }

    // OnEnable ������ � BaseSkill, �� �� ����� ��� ���������, ���� �����
    protected override void OnEnable()
    {
        base.OnEnable(); // �������� ������� ������ ��������
        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DamageTickCoroutine());
        }
    }

    // OnDisable ����
    protected override void OnDisable()
    {
        base.OnDisable(); // �������� ������� ������ �������
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    // !!! ������� �����, ����������� ����������
    protected override void UpdateSkillStats()
    {
        if (PlayerStatsManager.Instance == null) return;

        // 1. �������� ���������� ���������
        float areaMult = PlayerStatsManager.Instance.areaMultiplier;
        float damageMult = PlayerStatsManager.Instance.damageMultiplier;

        // 2. ������������ ������� ��������� ������
        currentRadius = baseRadius * (1f + areaMult);
        currentDamage = baseDamage * (1f + damageMult);

        // 3. ��������� ��������� � ����������� ����
        auraCollider.radius = currentRadius;

        // 4. ��������� ������!
        UpdateVisuals();

        // Debug.Log($"Aura stats updated: Radius={currentRadius}, Damage={currentDamage}");
    }

    private void UpdateVisuals()
    {
        // --- ���������� � VFX GRAPH ---
        if (vfxAura != null)
        {
            // � VFX Graph � ���������� ������ ���� "Exposed" ���������� ���� Float � ������ "AuraRadius".
            // �� ������������� �� �������� �� ����.
            vfxAura.SetFloat("AuraRadius", currentRadius);
        }

        // --- ������ ���������� � SHADER GRAPH (����� ��������) ---
        /*
        Renderer visualRenderer = GetComponentInChildren<Renderer>();
        if (visualRenderer != null)
        {
            // ������� MaterialPropertyBlock ��� �������������
            var propertyBlock = new MaterialPropertyBlock();
            visualRenderer.GetPropertyBlock(propertyBlock);
            // � ������� ������ ���� ���������� (Reference) � ������ "_AuraSize"
            propertyBlock.SetFloat("_AuraSize", currentRadius);
            visualRenderer.SetPropertyBlock(propertyBlock);
        }
        */
    }

    private IEnumerator DamageTickCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(baseTickRate); // ������� ���� �� ������, �� ����� ��������
            DealDamageInAura();
        }
    }

    private void DealDamageInAura()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentRadius, enemyLayerMask);

        int damageToDeal = Mathf.RoundToInt(currentDamage); // ���� - ����� �����

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<EnemyAI>(out EnemyAI groundEnemy))
            {
                groundEnemy.TakeDamage(damageToDeal);
                totalDamageDealt += damageToDeal;
            }
            else if (hitCollider.TryGetComponent<ProjectileEnemyAI>(out ProjectileEnemyAI swarmEnemy))
            {
                swarmEnemy.TakeDamage(damageToDeal);
                totalDamageDealt += damageToDeal;
            }
        }
    }
}