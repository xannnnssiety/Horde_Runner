using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileMover : MonoBehaviour
{
    public float ricochetSearchRadius = 20f;

    private Rigidbody _rigidbody;
    private float _initialSpeed;
    private float _damage;
    private int _ricochetsLeft;
    private bool _isRicocheting = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 direction, float speed, float lifetime, float damage, float ricochetChance, int ricochetCount)
    {
        _initialSpeed = speed;
        _damage = damage;
        _ricochetsLeft = ricochetCount;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        _rigidbody.linearVelocity = direction * _initialSpeed;

        if (Random.Range(0f, 100f) < ricochetChance)
        {
            _isRicocheting = true;
        }

        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleCollision(collision.transform);
        }
        else if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    private void HandleCollision(Transform target)
    {
        /*
        if (target.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            enemyHealth.TakeDamage(_damage);
        }
        */

        if (_isRicocheting && _ricochetsLeft > 0)
        {
            Transform nextTarget = FindNextTarget(target);
            if (nextTarget != null)
            {
                _ricochetsLeft--;
                Vector3 newDirection = (nextTarget.position - transform.position).normalized;

                if (newDirection.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(newDirection);
                }

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

        Collider[] validTargets = hits
            .Where(hit => hit.transform != this.transform && hit.transform != currentTarget && hit.CompareTag("Enemy"))
            .ToArray();

        if (validTargets.Length == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, validTargets.Length);
        return validTargets[randomIndex].transform;
    }
}