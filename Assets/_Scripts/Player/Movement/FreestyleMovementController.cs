using UnityEngine;

// ������� �� �� ����������, ��� � � ����� ������ �������, ��� �������������.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class FreestyleMovementController : MonoBehaviour
{
    // --- ��������� ��������� ---
    // ��� ������ ������ ������ �����������. �������� ������ ��������� � ����� �� ���� ���������.
    public enum PlayerState
    {
        Grounded,      // �� �����
        InAir,         // � ������� (������ ��� �������)
        Grinding,      // �������� �� ������
        WallRunning    // ����� �� �����
    }

    [Header("��������� (��� �������)")]
    [SerializeField] private PlayerState currentState;

    [Header("�������� ����������")]
    public Transform cameraTransform;
    private CharacterController controller;
    private Animator animator;

    // --- ���� ���������� (��������� ��� �������������) ---
    [Header("��������� �������� (�� ������� �������)")]
    public float baseMoveSpeed = 5f;
    public float maxMoveSpeed = 10f;
    public float speedIncreaseRate = 1f;
    [Tooltip("��������� ���������� ��� ������ ��������")]
    public float currentMoveSpeed; // �������� ���������� ��� ����� ������ ������!

    [Header("��������� ��������")]
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("��������� ������ � ����������")]
    public float jumpHeight = 2f;
    public float gravity = -19.62f;

    // --- ����� ���������� ��� ������� ---
    [Header("��������� �������")]
    [Tooltip("����, ������� ��������� �������� ��� �������")]
    public LayerMask grindLayers;
    [Tooltip("�������� �� ����� ����������")]
    public float grindSpeed = 15f;

    [Tooltip("����, ������� ��������� ������� ��� ����")]
    public LayerMask wallRunLayers;
    [Tooltip("��� ����� ����� ������ �� �����")]
    public float maxWallRunTime = 2f;
    [Tooltip("���� ������ �� �����")]
    public float wallRunJumpForce = 5f;
    private float currentWallRunTime;
    [Tooltip("���� �������� � ������� (0 - ��� ��������, 1 - ������ ��������)")]
    [Range(0f, 20f)]
    public float airControlForce = 5f;


    // --- ���������� ���������� ---
    private Vector3 velocity; // �������� �� ���������� � ������
    private Vector3 inputDir; // ����������� ����� � ����������
    private Transform currentRail; // ������ �� ������� ������, �� ������� ��������
    private Vector3 railDirection;
    private float railProgress;
    private Vector3 wallNormal; // ����������� �� ����� (��� ������)
    private bool isWallOnRight; // ����� ������ �� ���?

    // --- �������� ---
    private readonly int animIDSpeed = Animator.StringToHash("Speed");
    private readonly int animIDJump = Animator.StringToHash("Jump");
    private readonly int animIDGrounded = Animator.StringToHash("isGrounded");
    private readonly int animIDGrinding = Animator.StringToHash("isGrinding");
    private readonly int animIDWallRunning = Animator.StringToHash("isWallRunning");

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        currentMoveSpeed = baseMoveSpeed;
        currentState = PlayerState.Grounded; // �������� �� �����
    }

    void Update()
    {
        // 1. �������� ���� ���� ��� ��� ����� �����
        HandleInput();

        // 2. �������� ������, ��������������� �������� ��������� ���������
        switch (currentState)
        {
            case PlayerState.Grounded:
                HandleGroundedState();
                break;
            case PlayerState.InAir:
                HandleInAirState();
                break;
            case PlayerState.Grinding:
                HandleGrindingState();
                break;
            case PlayerState.WallRunning:
                HandleWallRunningState();
                break;
        }

        // 3. ��������� ���������� (���� �����) � ��������� ��������
        ApplyFinalMovement();
        controller.Move(velocity * Time.deltaTime);

        // 4. ��������� ��������
        UpdateAnimator();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDir = new Vector3(horizontal, 0f, vertical).normalized;
    }

    // --- ������ ��� ������� ��������� ---

    private void HandleGroundedState()
    {
        // ��������� �������� � ������ ���������
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
            return; // �������, ����� �� ��������� ��������� ������ ����� �����
        }
        if (!controller.isGrounded)
        {
            SwitchState(PlayerState.InAir);
            return;
        }

        // ������ �������� �� ����� (����� �� ������ �������)
        if (inputDir.magnitude >= 0.1f)
        {
            // ����������� ��������
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, maxMoveSpeed, speedIncreaseRate * Time.deltaTime);

            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            velocity.x = moveDirection.x * currentMoveSpeed;
            velocity.z = moveDirection.z * currentMoveSpeed;
        }
        else
        {
            // ��������� �������� � ������������� ��������
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, baseMoveSpeed, speedIncreaseRate * Time.deltaTime);
            velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * 10f); 
            velocity.z = Mathf.Lerp(velocity.z, 0, Time.deltaTime * 10f);
        }

        // ������������� ������������ �������� ��� "����������" � �����
        if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleInAirState()
    {
        // ��������� �������� � ������ ���������
        if (controller.isGrounded)
        {
            SwitchState(PlayerState.Grounded);
            return;
        }
        if (CheckForWallRun())
        {
            // �� ��������� �� �����, ���� ����� ������ ��� � ��� ��������
            // (��� ������������� "����������" ������� � ��� �� �����)
            // ����� ��������� ������, �� ���� ������� ���.
            SwitchState(PlayerState.WallRunning);
            return;
        }
        if (CheckForGrind(out Transform rail))
        {
            StartGrinding(rail);
            return;
        }

        // --- ������ �������� � ������� ---
        if (inputDir.magnitude >= 0.1f)
        {
            // �������� ����������� �������� ������������ ������
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // ��������� ��� ����������� � �������������� ��������
            // �� �� ������ �������� ��������, � "������������" �� � ������ �������
            velocity.x += moveDirection.x * airControlForce * Time.deltaTime;
            velocity.z += moveDirection.z * airControlForce * Time.deltaTime;

            // ������������ ������������ �������������� �������� � ������, ����� �� ������� � ������
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            if (horizontalVelocity.magnitude > maxMoveSpeed)
            {
                Vector3 limitedHorizontalVel = horizontalVelocity.normalized * maxMoveSpeed;
                velocity.x = limitedHorizontalVel.x;
                velocity.z = limitedHorizontalVel.z;
            }
        }
    }

    private void HandleGrindingState()
    {
        // �������� �� ���������
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
            return;
        }

        if (!CheckForGrind(out Transform rail) || rail != currentRail)
        {
            SwitchState(PlayerState.InAir);
            // ��������� ������� ��� ����� � ������
            velocity = transform.forward * grindSpeed;
            return;
        }

        // --- �������� ������ �������� �� ������ ---
        // �������� ������ ���������� ����� CharacterController.Move, � �� ����� velocity,
        // ����� �� ��������� ����������� ������������.
        Vector3 moveVector = railDirection * grindSpeed * Time.deltaTime;
        controller.Move(moveVector);

        // ��������� ��������� � ������������ �����������, ���� ������ ���������
        // (��� ����� ������ ������ ���� ������� �� ���������� ���������)
        transform.rotation = Quaternion.LookRotation(railDirection);
    }

    private void HandleWallRunningState()
    {
        // ������ ���� �� �����
        currentWallRunTime -= Time.deltaTime;

        // ��������
        if (Input.GetButtonDown("Jump"))
        {
            WallJump();
            return;
        }

        if (currentWallRunTime <= 0 || !CheckForWallRun())
        {
            SwitchState(PlayerState.InAir);
            return;
        }

        // --- �������� ������ �������� �� ����� ---
        // ��������� ������ ����� �����. ������ ����������� � ���, ����� �� ����������.
        Vector3 wallRunDirection = transform.forward;
        velocity = wallRunDirection * currentMoveSpeed; // ���������� ������� ��������

        // ������ "��������" ���� �� �����, ����� ��� �� ��������� ��� �����.
        // ���� ������ ��� �����, ����� ������� velocity.y = 2f;
        velocity.y = -1f;
    }

    // --- ��������������� ������� ---

    private void SwitchState(PlayerState newState)
    {
        if (currentState == newState) return;

        // ����� �������� ������ ������ �� ������� ��������� ����� (OnStateExit)

        currentState = newState;

        // ����� �������� ������ ����� � ����� ��������� ����� (OnStateEnter)
        switch (currentState)
        {
            case PlayerState.WallRunning:
                currentWallRunTime = maxWallRunTime;
                velocity.y = 0; // ���������� ������������ �������� ��� ������ ���� �� �����
                transform.forward = Vector3.Cross(wallNormal, Vector3.up) * (isWallOnRight ? 1 : -1);
                break;
            case PlayerState.Grinding:
                velocity.y = 0; // ���������� ���������� �� ������
                break;
        }
    }

    private void Jump()
    {
        SwitchState(PlayerState.InAir);
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void WallJump()
    {
        SwitchState(PlayerState.InAir);

        // ����������� ������ = ����������� �� ����� + ����������� �����
        Vector3 jumpDirection = (wallNormal + Vector3.up).normalized;

        // ��������� ����. ���������� ���� ���������� jumpHeight ��� ������.
        // � ����� wallRunJumpForce ��� ���� ������������.
        velocity = jumpDirection * wallRunJumpForce;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void StartGrinding(Transform rail)
    {
        SwitchState(PlayerState.Grinding);
        currentRail = rail;
        velocity = Vector3.zero; // ���������� ��� ���������� ��������

        // --- ����� �������� ����������� ����������� ������ ---
        // �� ������������, ��� ������ - ��� ��������� ������.
        // ��� "�����������" - ��� ��� ��������� ��� Z (forward) ��� X (right)
        // � ����������� �� ����, ��� �� ������������.
        // ���������, ����� ��� �������, � ���������� ��.
        if (rail.localScale.z > rail.localScale.x)
        {
            railDirection = rail.forward;
        }
        else
        {
            railDirection = rail.right;
        }

        // --- ������� ��������� ����� �� ������ � ������������� ---
        // ��� ������� �����. ��� ��������, �� ���� ����������� �� ������
        // � ��������� �����������.

        // ����������� �������� ���������
        transform.rotation = Quaternion.LookRotation(railDirection);

        // "��������������" ��������� � ������� ����� ������
        Collider railCollider = rail.GetComponent<Collider>();
        Vector3 closestPointOnRail = railCollider.ClosestPoint(transform.position);
        transform.position = new Vector3(closestPointOnRail.x, closestPointOnRail.y + controller.height / 2, closestPointOnRail.z);
    }

    private bool CheckForWallRun()
    {
        // ��������� ������� ����� ������
        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hitRight, 1f, wallRunLayers))
        {
            wallNormal = -hitRight.normal; // ������� ������� �� �����
            isWallOnRight = true;
            return true;
        }
        // ��������� ������� ����� �����
        else if (Physics.Raycast(transform.position, -transform.right, out RaycastHit hitLeft, 1f, wallRunLayers))
        {
            wallNormal = -hitLeft.normal;
            isWallOnRight = false;
            return true;
        }
        return false;
    }

    private bool CheckForGrind(out Transform rail)
    {
        // ������� ��� ����, ����� ����� ������
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f, grindLayers))
        {
            rail = hit.transform;
            return true;
        }
        rail = null;
        return false;
    }

    private void ApplyFinalMovement()
    {
        // ��������� ����������, ���� �� �� ����� � �� �� ������/�����
        if (!controller.isGrounded && currentState != PlayerState.Grinding && currentState != PlayerState.WallRunning)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // ��������� �� �������� (�������������� + ������������) ���� ���
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        // ������������� ��������� ��� ��������� �� ������ ���������
        animator.SetBool(animIDGrounded, currentState == PlayerState.Grounded);
        animator.SetBool(animIDGrinding, currentState == PlayerState.Grinding);
        animator.SetBool(animIDWallRunning, currentState == PlayerState.WallRunning);

        // ��� ��������� Speed ���������� �������� �����, ��� �� � �����
        animator.SetFloat(animIDSpeed, inputDir.magnitude);

        // ������� ������ ������ ����� � ������ Jump(), ����� ��� �� �����
        // Speed ������ ������� �� ���������
        float speedPercent = 0f;
        if (currentState == PlayerState.Grounded)
        {
            // �� ����� �������� �������� ������� �� �����
            speedPercent = inputDir.magnitude;
        }
        else if (currentState == PlayerState.Grinding || currentState == PlayerState.WallRunning)
        {
            // �� ������� ��� ����� �������� ������ �� ������ ��������
            speedPercent = 1f;
        }

        animator.SetFloat(animIDSpeed, speedPercent);
    }

    // --- ���� ��������� ����� (�������� ��� �������������) ---
    public void AddSpeedBoost(float amount, float duration)
    {
        // ������ ����������� ��������, ��� � ������.
        // �� ������ �������� ������ � duration (�������������) �����, ���� �����������.
        currentMoveSpeed = Mathf.Min(currentMoveSpeed + amount, maxMoveSpeed * 1.5f); // �������� ����� ��������� ��������
        Debug.Log($"Speed boosted by {amount}. New current speed: {currentMoveSpeed}");
    }
}