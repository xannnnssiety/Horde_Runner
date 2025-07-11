using UnityEngine;

/// <summary>
/// ��������� ��������� � �������� ����� �������� �������.
/// �������� ���� �������������� ����� ��� �������������.
/// </summary>
public class ProjectileMover : MonoBehaviour
{
    private float _speed;
    private float _lifetime;

    /// <summary>
    /// �������������� ������, ��������� ��� �������� � ����� �����.
    /// ���������� �������, ������� ��� �������.
    /// </summary>
    public void Initialize(float speed, float lifetime)
    {
        this._speed = speed;
        this._lifetime = lifetime;

        // ���������� ������ �� ��������� ����������� ������� �����
        Destroy(gameObject, this._lifetime);
    }

    void Update()
    {
        // ���� �������� �� ���� ����������� (��������, ��� ������), ������ �� ������
        if (_speed <= 0) return;

        // ����� ������ ������������ ������ ���������� �����������
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }
}