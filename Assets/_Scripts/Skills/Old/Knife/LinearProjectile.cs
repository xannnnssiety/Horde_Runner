using UnityEngine;

public class LinearProjectile : MonoBehaviour
{
    private int damage;
    private float speed;
    private LayerMask enemyLayerMask;
    private BaseSkill ownerSkill;

    private bool hasHit = false; // ��������������, ����� ��� ������� ���� ������ ���� ���

    // ������������� �� ��������� ������� ������
    public void Initialize(BaseSkill owner, int damage, float speed, float size, LayerMask enemyLayerMask, float lifetime)
    {
        this.ownerSkill = owner;
        this.damage = damage;
        this.speed = speed;
        this.enemyLayerMask = enemyLayerMask;

        transform.localScale = Vector3.one * size;

        // ���������� ������ ����� ��������� ����� �����
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // ������ ����� ������ �� ��������� ��� Z
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ���� ��� ������, ������ �� ������
        if (hasHit) return;

        // ���������, ��� ����������� � ������
        if ((enemyLayerMask.value & (1 << other.gameObject.layer)) > 0)
        {
            hasHit = true; // ����� ������ ����, ��� ������
            bool damageDealt = false;

            if (other.TryGetComponent<EnemyAI>(out EnemyAI groundEnemy))
            {
                groundEnemy.TakeDamage(damage);
                damageDealt = true;
            }
            else if (other.TryGetComponent<ProjectileEnemyAI>(out ProjectileEnemyAI swarmEnemy))
            {
                swarmEnemy.TakeDamage(damage);
                damageDealt = true;
            }

            if (damageDealt)
            {
                ownerSkill?.ReportDamage(damage);
            }

            // ������������ ����� ��� �������� � ������
            Destroy(gameObject);
        }
    }
}