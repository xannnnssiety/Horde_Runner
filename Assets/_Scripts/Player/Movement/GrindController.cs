using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Final : MonoBehaviour
{
    // --- ��������� ��������� (����� � ����������) ---

    [Header("������")]
    public Camera mainCamera;

    [Header("��������")]
    public float moveSpeed = 15f;
    public float turnSmoothTime = 0.1f;

    [Header("������ � ����������")]
    public float jumpHeight = 3f;
    public float gravity = -20f;
    public float coyoteTime = 0.15f;

    [Header("������")]
    public float grindSpeed = 25f;
    public LayerMask grindableLayer;
    [Tooltip("������, � ������� �������� ���� ������ ������ ����")]
    public float grindSearchRadius = 3f;


    // --- ��������� ���������� (��� ������ �������) ---

    // ���������
    private CharacterController controller;
    private Animator animator;
    private bool isGrinding = false;

    // ������
    private Vector3 playerVelocity;
    private float coyoteTimeCounter;
    private float turnSmoothVelocity;

    // ������
    private Transform currentRail;
    private Vector3 grindDirection;


    // --- �������� ������ UNITY ---

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (mainCamera == null) Debug.LogError("������ �� ���������! ���������� ���� Main Camera � ����.");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // � ����������� �� ��������� (�������� �� ��� ���), ��������� ������ ������
        if (isGrinding)
        {
            HandleGrinding();
        }
        else
        {
            HandleGroundedOrAirborne();
        }
    }


    // --- ������ �������� �������� (����� � ������) ---

    private void HandleGroundedOrAirborne()
    {
        // --- ������: ���������� � "����� ������" ---
        if (controller.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            // "���������" � �����, ����� �� ���� ������������� �� �������
            if (playerVelocity.y < 0) playerVelocity.y = -2f;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // ��������� ���������� ���������
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);


        // --- ����������: �������� � ������� ---
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            controller.Move(transform.forward * moveSpeed * Time.deltaTime);
        }

        // ��������� ��������
        animator.SetFloat("Speed", direction.magnitude);


        // --- ��������: ������ ---
        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
            coyoteTimeCounter = 0f; // ����� ������ ���� �������� ������
        }

        // --- ������� �� ������: ������������� ---
        // ���� �� �� �� �����, �� ��� ���� ���� ������, �������� ���������
        if (!controller.isGrounded)
        {
            TryToStartGrind();
        }
    }


    // --- ������ ������� ---

    private void TryToStartGrind()
    {
        // ������� ��� ����, ����� ����� ������
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, 3f, grindableLayer))
        {
            // �����! ����������� ���������
            isGrinding = true;
            currentRail = hit.transform; // ���������� ������ ������
            playerVelocity = Vector3.zero; // ��������� ����������

            // ���������� ��������� ����������� �������� �� ������
            float dot = Vector3.Dot(transform.forward, currentRail.forward);
            grindDirection = (dot >= 0) ? currentRail.forward : -currentRail.forward;
        }
    }

    private void HandleGrinding()
    {
        // 1. ���� ��������� ������ ������
        Transform bestRail = FindBestRail();

        if (bestRail == null)
        {
            // ���� ����� ����� ������ ���, ����������� ������ � ������
            EndGrind(false);
            return;
        }

        // ���� ������ ������ ���������� (�� �� �����), ��������� ��
        if (currentRail != bestRail)
        {
            currentRail = bestRail;
            // ���������� ����� ����������� ��������
            float dot = Vector3.Dot(grindDirection, currentRail.forward);
            grindDirection = (dot >= 0) ? currentRail.forward : -currentRail.forward;
        }

        // 2. ��������� � ������
        Vector3 closestPoint = currentRail.GetComponent<Collider>().ClosestPoint(transform.position);
        // ������, �� ������ ��������� � ����� �� ������. ��� ����������.
        controller.Move((closestPoint - transform.position));

        // 3. �������� ������ � ��������������
        controller.Move(grindDirection * grindSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(grindDirection), turnSmoothTime * 15f);

        // ��������� ��������
        animator.SetFloat("Speed", 1f); // �� ����� ������� ������ "�����"

        // 4. �������� �� �����������
        if (Input.GetButtonDown("Jump"))
        {
            EndGrind(true); // ����������� ������ � �������
        }
    }

    private Transform FindBestRail()
    {
        // ���� ��� ���������� ����� � ������� ������ ���������
        var nearbyRails = Physics.OverlapSphere(transform.position, grindSearchRadius, grindableLayer);

        if (nearbyRails.Length == 0) return null;

        // ������� ����� ������� ��������� �� ����
        return nearbyRails.OrderBy(rail => Vector3.Distance(transform.position, rail.ClosestPoint(transform.position)))
                          .FirstOrDefault()? // ����� ������ (����� �������) ��� null, ���� ������ ����
                          .transform;
    }

    private void EndGrind(bool didJump)
    {
        isGrinding = false;
        if (didJump)
        {
            // ���� ���������, ���� ������� �����
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }
    }
}