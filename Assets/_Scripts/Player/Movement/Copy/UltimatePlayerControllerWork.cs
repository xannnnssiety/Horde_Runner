using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class UltimatePlayerControllerWork : MonoBehaviour
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

    [Tooltip("��������� ����� �������� ������ ����������� � �������")]
    public float airControlRate = 10f;
    [Tooltip("��������� ������ �������� ��������������� �� ����� (������)")]
    public float groundFriction = 10f;

    // VVVV --- ����� --- VVVV
    [Header("��������� ������� �� �����")]
    [Tooltip("���� ��� ����, �� ������� ����� �������������")]
    public LayerMask wallJumpableLayer;
    [Tooltip("��� �������� �������� �������� �� ����� ����")]
    public float wallSlideSpeed = 2f;
    [Tooltip("������ ������ �� �����. �� ��������� � 2 ���� ���� ��������.")]
    public float wallJumpHeight = 4f;
    [Tooltip("���� ������������ � ������� �� �����")]
    public float wallJumpSidewaysForce = 8f;
    [Tooltip("��������� ��� �������� ����� ����� ����������")]
    public float wallCheckDistance = 0.5f;
    // ^^^^ --- ����� ������ --- ^^^^

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
    private Vector3 inputDirection;

    // ������
    private Transform currentGrindRail;
    private Vector3 grindDirection;
    private float grindCooldownTimer;
    private const float GRIND_COOLDOWN = 0.2f;

    // VVVV --- ����� --- VVVV
    // ������ �� �����
    private bool isWallSliding;
    private Vector3 wallNormal;
    // ^^^^ --- ����� ������ --- ^^^^

    // ID ��������
    private readonly int animIDSpeed = Animator.StringToHash("Speed");
    private readonly int animIDJump = Animator.StringToHash("Jump");


    // --- �������� ������ UNITY ---

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (mainCamera == null && Camera.main != null) mainCamera = Camera.main;
        else if (mainCamera == null) Debug.LogError("������ �� ������� � �� ���������!");

        currentMoveSpeed = baseMoveSpeed;
        currentState = controller.isGrounded ? PlayerState.Grounded : PlayerState.InAir;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (grindCooldownTimer > 0)
        {
            grindCooldownTimer -= Time.deltaTime;
        }

        HandleGroundedAndCoyoteTime();
        HandleSpeed();
        HandleWallSliding(); // ��������� ��������� ���������� �� �����

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

        // VVVV --- ��������� --- VVVV
        // ���������� ����������� �� ���� ����������, ����� �������
        if (currentState != PlayerState.Grinding)
        {
            ApplyGravity();
        }

        // ��������� �� ����������� �������� (�������������� + ������������) ����� �������
        controller.Move(playerVelocity * Time.deltaTime);
        // ^^^^ --- ����� ��������� --- ^^^^

        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"Speed: {currentMoveSpeed:F2}";
        }
    }

    // --- ������ ��������� ��������� ---

    private void HandleGroundedAndCoyoteTime()
    {
        // VVVV --- ��������� --- VVVV
        // �� ������� ��������� "�� �����", ���� ��������� �������� ����� ��� ���� �� �������� �� �����.
        // ��� ������������� ���������� ���������� �� ����� ���������� �� �����.
        bool isPhysicallyGrounded = controller.isGrounded;

        if (isPhysicallyGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // ������� ��������� � �����
        }

        if (isPhysicallyGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            // ���� �� ������������, ��������� ���������
            if (currentState == PlayerState.InAir) SetState(PlayerState.Grounded);
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            // ���� �� � ������� � �� �������� �� �����/������, �� �� � ������
            if (currentState == PlayerState.Grounded && coyoteTimeCounter <= 0)
            {
                SetState(PlayerState.InAir);
            }
        }
        // ^^^^ --- ����� ��������� --- ^^^^
    }

    private void HandleSpeed()
    {
        bool shouldAccelerate = inputDirection.magnitude >= 0.1f || (currentState == PlayerState.Grinding && currentGrindRail != null);
        float targetSpeed = shouldAccelerate ? maxMoveSpeed : baseMoveSpeed;
        float currentSpeedChangeRate = speedChangeRate;

        if (currentState == PlayerState.Grinding && currentGrindRail != null)
        {
            currentSpeedChangeRate *= grindAccelerationMultiplier;
        }

        currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetSpeed, currentSpeedChangeRate * Time.deltaTime);
    }

    private void HandleGroundedMovement()
    {
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // VVVV --- ��������� --- VVVV
            // ������ Move() �� ������ ������ �������������� ��������
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            playerVelocity.x = moveDir.x * currentMoveSpeed;
            playerVelocity.z = moveDir.z * currentMoveSpeed;
            // ^^^^ --- ����� ��������� --- ^^^^

            animator.SetFloat(animIDSpeed, 1f);
        }
        else
        {
            // VVVV --- ��������� --- VVVV
            // ������� ���������
            playerVelocity.x = Mathf.Lerp(playerVelocity.x, 0, groundFriction * Time.deltaTime);
            playerVelocity.z = Mathf.Lerp(playerVelocity.z, 0, groundFriction * Time.deltaTime);
            // ^^^^ --- ����� ��������� --- ^^^^
            animator.SetFloat(animIDSpeed, 0f);
        }

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            animator.SetTrigger(animIDJump);
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            SetState(PlayerState.InAir);
            coyoteTimeCounter = 0;
        }
    }

    private void HandleAirborneMovement()
    {
        // ���� �� ������������� �� �����, ������ ��� ��������� � HandleWallSliding
        if (isWallSliding && Input.GetButtonDown("Jump"))
        {
            DoWallJump();
            return; // �������, ����� �� ��������� ������� ���������� � ���� ����
        }

        // VVVV --- ���������: ��������� �������� ���������� � ������� --- VVVV
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // �� ������ ������ ������� �������� � ������� ������ �����������
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 targetVelocity = moveDir * currentMoveSpeed;

            // Lerp ������������ ������� ���������� � ������� ��� ������ ���������
            playerVelocity.x = Mathf.Lerp(playerVelocity.x, targetVelocity.x, airControlRate * Time.deltaTime);
            playerVelocity.z = Mathf.Lerp(playerVelocity.z, targetVelocity.z, airControlRate * Time.deltaTime);
        }
        // ^^^^ --- ����� ��������� --- ^^^^
    }

    private void HandleGrinding()
    {
        if (currentGrindRail == null) { EndGrind(false); return; }

        const float lookAheadDistance = 0.5f;
        Vector3 lookAheadPoint = transform.position + grindDirection * lookAheadDistance;
        Collider currentRailCollider = currentGrindRail.GetComponent<Collider>();
        Vector3 closestPointOnRail = currentRailCollider.ClosestPoint(lookAheadPoint);

        if (Vector3.Distance(lookAheadPoint, closestPointOnRail) > lookAheadDistance)
        {
            Transform nextRail = FindBestRail(currentGrindRail);
            if (nextRail != null)
            {
                currentGrindRail = nextRail;
                float dot = Vector3.Dot(grindDirection, currentGrindRail.forward);
                grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
            }
            else
            {
                EndGrind(false);
                return;
            }
        }
        else
        {
            Transform bestRail = FindBestRail();
            if (bestRail != null && bestRail != currentGrindRail)
            {
                currentGrindRail = bestRail;
                float dot = Vector3.Dot(grindDirection, currentGrindRail.forward);
                grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
            }
        }

        Vector3 snapToPoint = currentGrindRail.GetComponent<Collider>().ClosestPoint(transform.position);
        controller.Move(snapToPoint - transform.position); // ���������� ��������� Move

        // VVVV --- ��������� --- VVVV
        // ������ �������� ��� ��������� ����� ��������
        playerVelocity = grindDirection * currentMoveSpeed;
        // ^^^^ --- ����� ��������� --- ^^^^

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(grindDirection), turnSmoothTime * 15f);
        animator.SetFloat(animIDSpeed, 1f);

        if (Input.GetButtonDown("Jump"))
        {
            EndGrind(true);
        }
    }


    // --- ������ ��� ������� � ���� ---

    // VVVV --- ����� ����� --- VVVV
    private void HandleWallSliding()
    {
        isWallSliding = false;
        // ���������� �������� ������ � ������� � ����� �� �� �� ����� � �� �� ������
        if (currentState == PlayerState.InAir && !controller.isGrounded)
        {
            // ��������� ��� ����� ������ �� ���� �������� ���������
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, wallJumpableLayer))
            {
                // ���� �� ��������� � �����
                isWallSliding = true;
                wallNormal = hit.normal; // ��������� ������� ����� (����������� "�� �����")
            }
        }

        // ���� �� ��������, ��������� �������
        if (isWallSliding)
        {
            if (playerVelocity.y < -wallSlideSpeed)
            {
                playerVelocity.y = -wallSlideSpeed;
            }
        }
    }
    // ^^^^ --- ����� ������ ������ --- ^^^^

    // VVVV --- ����� ����� --- VVVV
    private void DoWallJump()
    {
        isWallSliding = false;

        // ������������ ������������ ������
        // ���������� ���� ������� � ����� ���������� ������
        playerVelocity.y = Mathf.Sqrt(wallJumpHeight * -2f * gravity);

        // �������������� ������������ ������
        // ������� ��������� � �����������, �������� ������� �����
        Vector3 jumpDirection = wallNormal * wallJumpSidewaysForce;
        playerVelocity.x = jumpDirection.x;
        playerVelocity.z = jumpDirection.z;

        // ������������ ��������� ����� �� ����� ��� ������� ����������� �������
        transform.rotation = Quaternion.LookRotation(wallNormal);

        // ��������� �������� ������
        animator.SetTrigger(animIDJump);
    }
    // ^^^^ --- ����� ������ ������ --- ^^^^

    private Transform FindBestRail(Transform railToIgnore = null)
    {
        var nearbyRails = Physics.OverlapSphere(transform.position, grindSearchRadius, grindableLayer);
        var bestRail = nearbyRails
            .Where(rail => rail.transform != railToIgnore)
            .OrderBy(rail => Vector3.Distance(transform.position, rail.ClosestPoint(transform.position)))
            .FirstOrDefault();
        return bestRail?.transform;
    }

    private void CheckForGrindStart()
    {
        if (currentState == PlayerState.Grinding || grindCooldownTimer > 0f) return;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 1.5f, grindableLayer))
        {
            if (!controller.isGrounded || controller.velocity.magnitude > 0.1f)
            {
                StartGrind(hit);
            }
        }
    }

    private void StartGrind(RaycastHit hit)
    {
        SetState(PlayerState.Grinding);
        playerVelocity.y = 0;
        currentGrindRail = hit.transform;
        float dot = Vector3.Dot(transform.forward, currentGrindRail.forward);
        grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
    }

    private void EndGrind(bool didJump)
    {
        Transform oldRail = currentGrindRail;
        currentGrindRail = null;
        SetState(PlayerState.InAir);
        grindCooldownTimer = GRIND_COOLDOWN;

        if (oldRail == null) return;

        // VVVV --- ��������� --- VVVV
        // ��� ������ ������ �������� ��������� ��������� ������� ������� playerVelocity
        playerVelocity = grindDirection * currentMoveSpeed;
        // ^^^^ --- ����� ��������� --- ^^^^

        if (didJump)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger(animIDJump);
        }
    }

    // --- ��������������� ������ ---
    private void ApplyGravity()
    {
        // ������ ��������� ���������� � Y-������������ ��������
        playerVelocity.y += gravity * Time.deltaTime;
    }

    private void SetState(PlayerState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
        }
    }
}