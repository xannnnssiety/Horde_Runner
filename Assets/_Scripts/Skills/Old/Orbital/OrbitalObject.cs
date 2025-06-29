using UnityEngine;

public class OrbitalObject : MonoBehaviour
{
    private int damage;
    private float cooldown; // ������������ �������, ����� �� �������� ���� ������ ����
    private LayerMask enemyLayerMask;
    private BaseSkill ownerSkill; // ������ �� ������������ ����� ��� ������ �� �����

    private float lastHitTime; // ����� ���������� �����

    public void Initialize(BaseSkill owner, int damage, float hitCooldown, LayerMask enemyLayerMask)
    {
        this.ownerSkill = owner;
        this.damage = damage;
        this.cooldown = hitCooldown;
        this.enemyLayerMask = enemyLayerMask;
        this.lastHitTime = -hitCooldown; // ����� ������ ���� ��� ����������
    }

    private void OnTriggerEnter(Collider other)
    {
        // ���������, ��� �� ������ ����� ����������� � ��� �� ����������� � ������
        if (Time.time < lastHitTime + cooldown) return;
        if ((enemyLayerMask.value & (1 << other.gameObject.layer)) == 0) return;

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
            lastHitTime = Time.time;

            // �������� �� �����. ��� ����� ����� �������� ReportDamage � BaseSkill.
            // ���� ��� ������������, ��� �� ��� �����.
            ownerSkill?.ReportDamage(damage);

            // TODO: ����� ����� �������� VFX ��� ���������
        }
    }
}