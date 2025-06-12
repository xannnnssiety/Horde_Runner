using UnityEngine;

// ���� ������ ���������� ������ ��������� �� �����, �� �� ���������� �� ��������.
public class FollowTarget : MonoBehaviour
{
    [Tooltip("����, �� ������� ����� ���������")]
    public Transform target;

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }
}