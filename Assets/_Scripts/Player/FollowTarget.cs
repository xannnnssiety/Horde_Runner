using UnityEngine;

// Этот скрипт заставляет объект следовать за целью, но не копировать ее вращение.
public class FollowTarget : MonoBehaviour
{
    [Tooltip("Цель, за которой нужно следовать")]
    public Transform target;

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }
}