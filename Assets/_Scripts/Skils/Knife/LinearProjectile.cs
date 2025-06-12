using UnityEngine;

public class LinearProjectile : MonoBehaviour
{
    private int damage;
    private float speed;
    private LayerMask enemyLayerMask;
    private BaseSkill ownerSkill;

    private bool hasHit = false; // Предохранитель, чтобы нож наносил урон только один раз

    // Инициализация из основного скрипта скилла
    public void Initialize(BaseSkill owner, int damage, float speed, float size, LayerMask enemyLayerMask, float lifetime)
    {
        this.ownerSkill = owner;
        this.damage = damage;
        this.speed = speed;
        this.enemyLayerMask = enemyLayerMask;

        transform.localScale = Vector3.one * size;

        // Уничтожаем объект через указанное время жизни
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Просто летим вперед по локальной оси Z
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Если уже попали, ничего не делаем
        if (hasHit) return;

        // Проверяем, что столкнулись с врагом
        if ((enemyLayerMask.value & (1 << other.gameObject.layer)) > 0)
        {
            hasHit = true; // Сразу ставим флаг, что попали
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

            // Уничтожаемся сразу при контакте с врагом
            Destroy(gameObject);
        }
    }
}