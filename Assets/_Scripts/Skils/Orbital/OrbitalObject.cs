using UnityEngine;

public class OrbitalObject : MonoBehaviour
{
    private int damage;
    private float cooldown; // Персональный кулдаун, чтобы не наносить урон каждый кадр
    private LayerMask enemyLayerMask;
    private BaseSkill ownerSkill; // Ссылка на родительский скилл для отчета об уроне

    private float lastHitTime; // Время последнего удара

    public void Initialize(BaseSkill owner, int damage, float hitCooldown, LayerMask enemyLayerMask)
    {
        this.ownerSkill = owner;
        this.damage = damage;
        this.cooldown = hitCooldown;
        this.enemyLayerMask = enemyLayerMask;
        this.lastHitTime = -hitCooldown; // Чтобы первый удар был мгновенным
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что не прошло время перезарядки и что мы столкнулись с врагом
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

            // Сообщаем об уроне. Нам нужно будет добавить ReportDamage в BaseSkill.
            // Пока что предполагаем, что он там будет.
            ownerSkill?.ReportDamage(damage);

            // TODO: Здесь можно добавить VFX при попадании
        }
    }
}