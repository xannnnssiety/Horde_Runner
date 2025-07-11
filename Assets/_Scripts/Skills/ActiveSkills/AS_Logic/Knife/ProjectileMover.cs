using UnityEngine;

/// <summary>
/// ”правл€ет движением и временем жизни простого снар€да.
/// ѕолучает свои характеристики извне при инициализации.
/// </summary>
public class ProjectileMover : MonoBehaviour
{
    private float _speed;
    private float _lifetime;

    /// <summary>
    /// »нициализирует снар€д, передава€ ему скорость и врем€ жизни.
    /// ¬ызываетс€ умением, которое его создало.
    /// </summary>
    public void Initialize(float speed, float lifetime)
    {
        this._speed = speed;
        this._lifetime = lifetime;

        // ”ничтожаем объект по истечении переданного времени жизни
        Destroy(gameObject, this._lifetime);
    }

    void Update()
    {
        // ≈сли скорость не была установлена (например, при ошибке), ничего не делаем
        if (_speed <= 0) return;

        // Ћетим вперед относительно своего локального направлени€
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }
}