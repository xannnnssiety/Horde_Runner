using UnityEngine;
using System.Collections.Generic; // Оставляем на всякий случай, но список больше не используется
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileMover : MonoBehaviour
{
    public float ricochetSearchRadius = 20f;

    private Rigidbody _rigidbody;
    private float _damage;
    private float _initialSpeed;
    private int _ricochetsLeft;
    private bool _isRicocheting = false;

    // --- ИЗМЕНЕНИЕ: Список пораженных целей больше не нужен ---
    // private List<Transform> _hitTargets = new List<Transform>();

    public void Initialize(Vector3 direction, float speed, float lifetime, float damage, float ricochetChance, int ricochetCount)
    {
        _rigidbody = GetComponent<Rigidbody>();
        _initialSpeed = speed;
        _rigidbody.linearVelocity = direction * _initialSpeed;
        this._damage = damage;
        this._ricochetsLeft = ricochetCount;

        if (Random.Range(0f, 100f) < ricochetChance)
        {
            _isRicocheting = true;
        }

        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // --- ИЗМЕНЕНИЕ: Убрана проверка на уже пораженные цели ---
        if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleCollision(collision.transform);
        }
    }

    private void HandleCollision(Transform target)
    {
/*        if (target.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            enemyHealth.TakeDamage(_damage);
        }*/
        // --- ИЗМЕНЕНИЕ: Больше не добавляем цель в список ---
        // _hitTargets.Add(target);

        if (_isRicocheting && _ricochetsLeft > 0)
        {
            Transform nextTarget = FindNextTarget(target);
            if (nextTarget != null)
            {
                _ricochetsLeft--;
                Vector3 newDirection = (nextTarget.position - transform.position).normalized;
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.linearVelocity = newDirection * _initialSpeed;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Transform FindNextTarget(Transform currentTarget)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ricochetSearchRadius);

        // --- КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ ---
        // 1. Находим всех валидных врагов для рикошета и помещаем их в массив.
        Collider[] validTargets = hits
            .Where(hit => hit.transform != this.transform && hit.transform != currentTarget && hit.CompareTag("Enemy"))
            .ToArray();

        // 2. Если валидных целей нет, возвращаем null, чтобы снаряд уничтожился.
        if (validTargets.Length == 0)
        {
            return null;
        }

        // 3. Выбираем случайный индекс из массива валидных целей.
        int randomIndex = Random.Range(0, validTargets.Length);

        // 4. Возвращаем transform случайной цели.
        return validTargets[randomIndex].transform;
    }
}