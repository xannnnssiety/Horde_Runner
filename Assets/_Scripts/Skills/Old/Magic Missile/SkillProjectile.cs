using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class SkillProjectile : MonoBehaviour
{
    private int damage;
    private float speed;
    private float lifetime = 5f; // Время жизни, если не попал в цель
    private Transform target;
    private LayerMask enemyLayerMask;
    private float size; // Размер для масштабирования
    private BaseSkill ownerSkill;


    [Tooltip("Ссылка на VFX компонент на этом объекте")]
    [SerializeField] private VisualEffect projectileVFX;

    // Этот метод будет вызван сразу после создания снаряда
    public void Initialize(BaseSkill owner, int damage, float speed, float size, Transform target, LayerMask enemyLayerMask, float lifetime)
    {
        this.ownerSkill = owner;
        this.damage = damage;
        this.speed = speed;
        this.target = target;
        this.enemyLayerMask = enemyLayerMask;
        this.lifetime = lifetime;
        this.size = size;

        // Применяем размер к трансформации и визуальному эффекту
        transform.localScale = Vector3.one * this.size;
        if (projectileVFX != null)
        {
            // У VFX должна быть Exposed переменная "Size"
            projectileVFX.SetFloat("Size", this.size);
        }

        // Уничтожаем снаряд, если он слишком долго летит
        Destroy(gameObject, this.lifetime);
    }

    void Update()
    {
        // Если цели больше нет, летим прямо
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            // Движение вперед относительно локальных координат
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }

        // Движемся к цели и плавно поворачиваемся
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Плавно поворачиваем в сторону цели. Можете настроить скорость поворота.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        // Движемся вперед
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что столкнулись с врагом (сравнивая слои)
        if ((enemyLayerMask.value & (1 << other.gameObject.layer)) > 0)
        {
            // Пытаемся нанести урон
            if (other.TryGetComponent<EnemyAI>(out EnemyAI groundEnemy))
            {
                groundEnemy.TakeDamage(damage);
            }
            else if (other.TryGetComponent<ProjectileEnemyAI>(out ProjectileEnemyAI swarmEnemy))
            {
                swarmEnemy.TakeDamage(damage);
            }

            // TODO: Здесь можно добавить VFX взрыва при попадании

            // Уничтожаем снаряд после попадания
            Destroy(gameObject);
        }
    }
}