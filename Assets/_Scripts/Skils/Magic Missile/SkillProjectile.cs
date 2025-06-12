using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class SkillProjectile : MonoBehaviour
{
    private int damage;
    private float speed;
    private float lifetime = 5f; // ����� �����, ���� �� ����� � ����
    private Transform target;
    private LayerMask enemyLayerMask;
    private float size; // ������ ��� ���������������
    private BaseSkill ownerSkill;


    [Tooltip("������ �� VFX ��������� �� ���� �������")]
    [SerializeField] private VisualEffect projectileVFX;

    // ���� ����� ����� ������ ����� ����� �������� �������
    public void Initialize(BaseSkill owner, int damage, float speed, float size, Transform target, LayerMask enemyLayerMask, float lifetime)
    {
        this.ownerSkill = owner;
        this.damage = damage;
        this.speed = speed;
        this.target = target;
        this.enemyLayerMask = enemyLayerMask;
        this.lifetime = lifetime;
        this.size = size;

        // ��������� ������ � ������������� � ����������� �������
        transform.localScale = Vector3.one * this.size;
        if (projectileVFX != null)
        {
            // � VFX ������ ���� Exposed ���������� "Size"
            projectileVFX.SetFloat("Size", this.size);
        }

        // ���������� ������, ���� �� ������� ����� �����
        Destroy(gameObject, this.lifetime);
    }

    void Update()
    {
        // ���� ���� ������ ���, ����� �����
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            // �������� ������ ������������ ��������� ���������
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }

        // �������� � ���� � ������ ��������������
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // ������ ������������ � ������� ����. ������ ��������� �������� ��������.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        // �������� ������
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ���������, ��� ����������� � ������ (��������� ����)
        if ((enemyLayerMask.value & (1 << other.gameObject.layer)) > 0)
        {
            // �������� ������� ����
            if (other.TryGetComponent<EnemyAI>(out EnemyAI groundEnemy))
            {
                groundEnemy.TakeDamage(damage);
            }
            else if (other.TryGetComponent<ProjectileEnemyAI>(out ProjectileEnemyAI swarmEnemy))
            {
                swarmEnemy.TakeDamage(damage);
            }

            // TODO: ����� ����� �������� VFX ������ ��� ���������

            // ���������� ������ ����� ���������
            Destroy(gameObject);
        }
    }
}