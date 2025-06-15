using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class UltimatePlayerController : MonoBehaviour
{
    // --- ������������ ��������� ---
    private enum PlayerState
    {
        Grounded,
        InAir,
        Grinding
    }

    // --- ��������� ��������� (����� � ����������) ---

    [Header("������")]
    public Camera mainCamera;
    [Tooltip("���������� ���� ��������� ������� (Legacy) ��� ����������� ��������")]
    public Text speedDisplayText;

    [Header("��������� ��������")]
    public float baseMoveSpeed = 5f;
    public float maxMoveSpeed = 10f;
    [Tooltip("��� ������ �������� ��������/���������� �������� �� �����")]
    public float speedChangeRate = 2f;
    public float turnSmoothTime = 0.1f;

    [Header("��������� ������ � ����������")]
    public float jumpHeight = 2f;
    public float gravity = -19.62f;
    public float coyoteTime = 0.15f;

    [Header("��������� �������")]
    [Tooltip("��������� ��������� �� ������ (1.5 = �� 50% �������)")]
    public float grindAccelerationMultiplier = 1.5f;
    public LayerMask grindableLayer;
    public float grindSearchRadius = 3f;

    [Header("��������� (��� �������)")]
    [SerializeField] private PlayerState currentState;


    // --- ��������� ���������� ---

    // ����������
    private CharacterController controller;
    private Animator animator;

    // ���������
    private Vector3 playerVelocity;
    private float coyoteTimeCounter;
    private float turnSmoothVelocity;
    private float currentMoveSpeed;
    private Vector3 inputDirection; // ������� ����������� �����, ����� ��� ���� �������� �����

    // ������
    private Transform currentGrindRail;
    private Vector3 grindDirection;
    private float grindCooldownTimer;
    private const float GRIND_COOLDOWN = 0.2f;

    // ID ��������
    private readonly int animIDSpeed = Animator.StringToHash("Speed");
    private readonly int animIDJump = Animator.StringToHash("Jump");


    // --- �������� ������ UNITY ---

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main;
        }
        else if (mainCamera == null)
        {
            Debug.LogError("������ �� ������� � �� ���������!");
        }

        currentMoveSpeed = baseMoveSpeed;
        currentState = controller.isGrounded ? PlayerState.Grounded : PlayerState.InAir;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. �������� ���� �� ������
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. ��������� �������
        if (grindCooldownTimer > 0)
        {
            grindCooldownTimer -= Time.deltaTime;
        }

        if (controller.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 3. ��������� �������� (������ �������� ����)
        HandleSpeed();

        // 4. ������������� ���������
        switch (currentState)
        {
            case PlayerState.Grounded:
                HandleGroundedMovement();
                CheckForGrindStart();
                break;
            case PlayerState.InAir:
                HandleAirborneMovement();
                CheckForGrindStart();
                break;
            case PlayerState.Grinding:
                HandleGrinding();
                break;
        }

        // 5. ��������� ���������� (����� �������)
        if (currentState != PlayerState.Grinding)
        {
            ApplyGravity();
        }

        // 6. ����� �������� �� �����
        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"Speed: {currentMoveSpeed:F2}";
        }
    }


    // --- ������ ��������� ��������� ---

    private void HandleSpeed()
    {
        // ���������� ������� �������� ���������
        float currentSpeedChangeRate = speedChangeRate;
        if (currentState == PlayerState.Grinding)
        {
            currentSpeedChangeRate *= grindAccelerationMultiplier;
        }

        // ���� ���� ���� ��� �� ��������, �������� ��������
        if (inputDirection.magnitude >= 0.1f || currentState == PlayerState.Grinding)
        {
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, maxMoveSpeed, currentSpeedChangeRate * Time.deltaTime);
        }
        else // ���� ��� ����� � �� �� �� ������, ���������� ��������
        {
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, baseMoveSpeed, speedChangeRate * Time.deltaTime);
        }
    }

    private void HandleGroundedMovement()
    {
        // �������� � ������� ����������� ������ ���� ���� ����
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            controller.Move(transform.forward * currentMoveSpeed * Time.deltaTime);
            animator.SetFloat(animIDSpeed, 1f);
        }
        else
        {
            animator.SetFloat(animIDSpeed, 0f);
        }

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            animator.SetTrigger(animIDJump);
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            SetState(PlayerState.InAir);
            coyoteTimeCounter = 0;
        }

        if (coyoteTimeCounter <= 0f && !controller.isGrounded)
        {
            SetState(PlayerState.InAir);
        }
    }

    private void HandleAirborneMovement()
    {
        // ���� ���� ����, ��������� ������� ��������� ����������
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        // � ������� �������� ���������� ������ ������ � ������� ���������
        // �� ���������� ����������� ���������, � �� ������, ��� ����� �������������� ������
        controller.Move(transform.forward * currentMoveSpeed * Time.deltaTime);

        if (controller.isGrounded)
        {
            SetState(PlayerState.Grounded);
        }
    }

    private void HandleGrinding()
    {
        Transform bestRail = FindBestRail();

        if (bestRail == null)
        {
            EndGrind(false);
            return;
        }

        if (currentGrindRail != bestRail)
        {
            currentGrindRail = bestRail;
            // ���������� ����� ����������� �������� �� �����
            float dot = Vector3.Dot(grindDirection, currentGrindRail.forward);
            grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
        }

        // ��������� � ������
        Vector3 closestPoint = currentGrindRail.GetComponent<Collider>().ClosestPoint(transform.position);
        controller.Move((closestPoint - transform.position));

        // ��������� � ������� ��������� (������� ������ ���������� � HandleSpeed)
        controller.Move(grindDirection * currentMoveSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(grindDirection), turnSmoothTime * 15f);
        animator.SetFloat(animIDSpeed, 1f);

        if (Input.GetButtonDown("Jump"))
        {
            EndGrind(true);
        }
    }


    // --- ������ ��� ������� ---

    private Transform FindBestRail()
    {
        var nearbyRails = Physics.OverlapSphere(transform.position, grindSearchRadius, grindableLayer);
        if (nearbyRails.Length == 0) return null;

        return nearbyRails.OrderBy(rail => Vector3.Distance(transform.position, rail.ClosestPoint(transform.position)))
                          .FirstOrDefault()?
                          .transform;
    }

    private void CheckForGrindStart()
    {
        if (currentState == PlayerState.Grinding || grindCooldownTimer > 0f) return;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 1.5f, grindableLayer))
        {
            // �������� ������ ���� ������ �� ������, ��� �������� �� ��� � �������
            if (!controller.isGrounded || controller.velocity.magnitude > 0.1f)
            {
                StartGrind(hit);
            }
        }
    }

    private void StartGrind(RaycastHit hit)
    {
        SetState(PlayerState.Grinding);
        playerVelocity.y = 0; // �������� ������ ������������ �������� (����������)
        currentGrindRail = hit.transform;

        // ���������� ��������� ����������� ����������
        float dot = Vector3.Dot(transform.forward, currentGrindRail.forward);
        grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
    }

    private void EndGrind(bool didJump)
    {
        SetState(PlayerState.InAir);
        currentGrindRail = null;
        grindCooldownTimer = GRIND_COOLDOWN;

        if (didJump)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger(animIDJump);
        }

        // ����� �� ������ � ������, �� ����� ��������� �������.
        // �� ������ playerVelocity, ����� �� �������������� ������ ���������� ��������.
        playerVelocity = grindDirection * currentMoveSpeed;
    }


    // --- ��������������� ������ ---

    private void ApplyGravity()
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, playerVelocity.y, 0) * Time.deltaTime);
    }

    private void SetState(PlayerState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
        }
    }
}