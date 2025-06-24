using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ManualRootRotationHandler : MonoBehaviour
{
    private Animator animator;

    // ����� ���������� ��� ���� ��� ������������������, ��� ���������� ������ ������ ����.
    private readonly int manualRotationTagHash = Animator.StringToHash("ManualRootRotation");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // ���� ����� ���������� ������ ����, ����� �������� ��������� root motion.
    // �� ���� ��� �������� ��� ���, ��� ��� �������� �����������.
    void OnAnimatorMove()
    {
        if (animator == null) return;

        // ���������, ������� �� ������ ����� � ����� ����� �� ������� ���� (0)
        if (animator.GetCurrentAnimatorStateInfo(0).tagHash == manualRotationTagHash)
        {
            // ���� ��, �� �� ������� ��������� �������� �� �������� (deltaRotation)
            // � transform ������ �������.
            // �������� *= ��� ������������ �������� "�������� ��������".
            transform.rotation *= animator.deltaRotation;

            // �����! ����� ����� ��������� � deltaPosition.
            // ���� ���� �������� "�� �����", ����� ���� �����-��������,
            // ������� ��� ���� ������ ������� "��������" �� �����.
            // ���� ���� �������� �������� �� �����, ��� ������ ����� ����������������.
            transform.position += animator.deltaPosition;
        }
        // ���� ������������� ����� ������ �������� (��� ������ ����),
        // ���� ��� �� ����������, � �������� ����� ����������� ��� ������
        // (��������, ����� �������� Character Controller'�).
    }
}