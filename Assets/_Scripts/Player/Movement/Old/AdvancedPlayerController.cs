using UnityEngine;


// ������� �����-����������, ������� ��������� �����������
[RequireComponent(typeof(CharacterController))]
public class AdvancedPlayerController : MonoBehaviour
{
    #region Debug
    [Header("��������� (��� �������)")]
    [SerializeField] private string _currentStateName; // <<< �����
    #endregion

    #region Public Variables & Inspector Settings
    [Header("Dependencies")]
    [Tooltip("������ �� ������� ������ ��� ������� ����������� ��������.")]
    public Transform cameraTransform;


    [Header("Movement Settings")]
    [Tooltip("�������� ���� �� �����.")]
    public float moveSpeed = 8f;
    [Tooltip("���� ������.")]
    public float jumpForce = 10f;
    [Tooltip("���� ����������. ����������� �������.")]
    public float gravity = -20f;
    [Tooltip("��������� �������� ���������.")]
    public float turnSmoothTime = 0.1f;

    [Tooltip("������ �� ��������� Animator ���������.")]
    public Animator animator;

    [Header("Animation Settings")] 
    [Tooltip("��������� ����� �������� 'Speed' � ���������.")] 
    public float animationDampTime = 0.1f; 

    [Header("Air Control")]
    [Tooltip("��������� ������ ����� ����� ������ �� �������� � �������.")]
    public float airControlStrength = 4f;

    [Header("Grinding Settings")]
    [Tooltip("�������� ������������ �� �������.")]
    public float grindSpeed = 15f;
    [Tooltip("��� ��� ��������, �� ������� ����� ���������.")]
    public string grindableTag = "Grindable";

    [Header("Wall-Running Settings")]
    [Tooltip("�������� ���� �� ������.")]
    public float wallRunSpeed = 12f;
    [Tooltip("������������ ����������������� ���� �� ����� (� ��������).")]
    public float wallRunDuration = 2f;
    [Tooltip("���� ������ �� ����� (����� � � �������).")]
    public float wallJumpForce = 12f;
    [Tooltip("��� ��� ��������, �� ������� ����� ������.")]
    public string wallRunnableTag = "WallRunnable";

    #endregion


    #region State Machine
    // --- �������� ������� (State Machine) ---
    private PlayerBaseState _currentState;



    // ������� ���������� ���� ��������� ���������
    public GroundedState GroundedState { get; private set; }
    public InAirState InAirState { get; private set; }
    public GrindingState GrindingState { get; private set; }
    public WallRunningState WallRunningState { get; private set; }

    // ����� ��� ����� ���������
    public void SwitchState(PlayerBaseState newState)
    {
        _currentState?.ExitState(); // �������� ����� �� �������� ���������
        _currentState = newState;
        _currentStateName = newState.GetType().Name;
        _currentState.EnterState(); // ������ � ����� ���������
    }
    #endregion

    #region Animation Hashes 
    public readonly int SpeedHash = Animator.StringToHash("Speed");
    public readonly int JumpHash = Animator.StringToHash("Jump");
    public readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    public readonly int IsGrindingHash = Animator.StringToHash("IsGrinding");
    public readonly int IsWallRunningHash = Animator.StringToHash("IsWallRunning");
    #endregion


    #region Public Properties & Components
    // --- ���������� � ��������� ��������, ��������� ��� ���� ��������� ---
    public CharacterController Controller { get; private set; }
    public Vector3 Velocity { get; set; } // �������� ���������� ��� ���������� ������� (momentum)
    public float TurnSmoothVelocity; // ������������ ��� SmoothDampAngle
    public Vector2 InputVector { get; private set; }
    public Transform LastGrindableRail { get; set; } // ��������� ��������� ������
    public Vector3 WallNormal { get; set; } // ������� ����� ��� ���� � ������������
    #endregion


    #region Unity Lifecycle
    private void Awake()
    {

        if (animator == null) 
        {
            animator = GetComponentInChildren<Animator>();
        }

        Controller = GetComponent<CharacterController>();

        // ������������� ���������
        GroundedState = new GroundedState(this);
        InAirState = new InAirState(this);
        GrindingState = new GrindingState(this);
        WallRunningState = new WallRunningState(this);
    }

    private void Start()
    {
        // ��������� ��������� - �� �����
        SwitchState(GroundedState);
    }

    private void Update()
    {
        // ��������� ����� ������ ����
        InputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // ���������� ��� ������ �������� ���������
        _currentState.UpdateState();
        UpdateAnimator();

    }

    private void UpdateAnimator()
    {
        // ��������� �������������� �������� ��� ��������� Speed
        Vector3 horizontalVelocity = new Vector3(Controller.velocity.x, 0, Controller.velocity.z);
        float speed = horizontalVelocity.magnitude;

        // ���������� SmoothDamp ��� �������� ��������� �������� � ���������
        animator.SetFloat(SpeedHash, speed, animationDampTime, Time.deltaTime);
    }

    // ���� ����� ����������, ����� CharacterController ������������ � ������ �����������
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // �������� ���������� � ������������ � ������� ��������� ��� ���������
        _currentState.HandleCollision(hit);
    }
    #endregion
}

#region State Machine Abstract Base

// ����������� ������� ����� ��� ���� ���������
public abstract class PlayerBaseState
{
    protected readonly AdvancedPlayerController Ctx; // �������� (������ �� ������� ����������)

    protected PlayerBaseState(AdvancedPlayerController context)
    {
        Ctx = context;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public virtual void HandleCollision(ControllerColliderHit hit) { } // ����������� �����, �.�. �� ���� ���������� ����� ��������� ������������
}

#endregion


#region Concrete States

// --- ���������: �� ����� (Grounded) ---
public class GroundedState : PlayerBaseState
{
    public GroundedState(AdvancedPlayerController context) : base(context) { }

    public override void EnterState()
    {
        // ��� ����������� ����� ������������ ��������, ����� �� "������������"
        var velocity = Ctx.Velocity;
        velocity.y = -2f; // ��������� ������������� �������� ��� ����������� isGrounded
        Ctx.Velocity = velocity;
        Ctx.animator.SetBool(Ctx.IsGroundedHash, true);
    }

    public override void UpdateState()
    {
        // 1. �������� �� ������� � ������ ���������
        // ������
        if (Input.GetButtonDown("Jump"))
        {
            var velocity = Ctx.Velocity;
            velocity.y = Ctx.jumpForce;
            Ctx.Velocity = velocity;
            Ctx.animator.SetTrigger(Ctx.JumpHash);
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // ���� � ������
        if (!Ctx.Controller.isGrounded)
        {
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // 2. ������ ��������
        Vector2 input = Ctx.InputVector;
        if (input.sqrMagnitude > 0.01f) // ���� ���� ����
        {
            // ������ ����������� �������� ������������ ������
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + Ctx.cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // ������� ���������
            float angle = Mathf.SmoothDampAngle(Ctx.transform.eulerAngles.y, targetAngle, ref Ctx.TurnSmoothVelocity, Ctx.turnSmoothTime);
            Ctx.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // ���������� ��������
            Vector3 horizontalVelocity = moveDir.normalized * Ctx.moveSpeed;
            Ctx.Velocity = new Vector3(horizontalVelocity.x, Ctx.Velocity.y, horizontalVelocity.z);
        }
        else // ���� ����� ���, ���������������
        {
            Ctx.Velocity = new Vector3(0, Ctx.Velocity.y, 0);
        }

        // ���������� ��������
        Ctx.Controller.Move(Ctx.Velocity * Time.deltaTime);
    }

    public override void ExitState() 
    {
        Ctx.animator.SetBool(Ctx.IsGroundedHash, false);
    }

}


// --- ���������: � ������� (InAir) ---
public class InAirState : PlayerBaseState
{
    public InAirState(AdvancedPlayerController context) : base(context) { }

    public override void EnterState() { }

    public override void UpdateState()
    {
        // 1. �������� �� �����������
        if (Ctx.Controller.isGrounded && Ctx.Velocity.y < 0)
        {
            Ctx.SwitchState(Ctx.GroundedState);
            return;
        }

        // 2. ������ �������� � �������
        // ���������� ����������
        var velocity = Ctx.Velocity;
        velocity.y += Ctx.gravity * Time.deltaTime;

        // �������� � ������� (Air Control)
        Vector2 input = Ctx.InputVector;
        if (input.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + Ctx.cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // ������ ��������� �������� � ������� �������, � �� �������� ��
            velocity += moveDir.normalized * Ctx.airControlStrength * Time.deltaTime;
        }

        Ctx.Velocity = velocity;

        // ���������� ��������
        Ctx.Controller.Move(Ctx.Velocity * Time.deltaTime);
    }

    public override void HandleCollision(ControllerColliderHit hit)
    {
        // �������� �� ������������ � �������
        if (hit.gameObject.CompareTag(Ctx.grindableTag))
        {
            Ctx.LastGrindableRail = hit.transform;
            Ctx.SwitchState(Ctx.GrindingState);
        }
        // �������� �� ������������ �� ������
        else if (hit.gameObject.CompareTag(Ctx.wallRunnableTag))
        {
            // ��������, ��� �� ��������� � �����, � �� � ��� ��� �������
            // ������� - ��� ������, ���������������� �����������. ��� ���� �� ����� ��������������.
            if (Mathf.Abs(hit.normal.y) < 0.1f)
            {
                Ctx.WallNormal = hit.normal;
                Ctx.SwitchState(Ctx.WallRunningState);
            }
        }
    }

    public override void ExitState() { }
}


// --- ���������: ������ (Grinding) ---
// --- ���������: ������ (Grinding) ---
// --- ���������: ������ (Grinding) ---
// --- ���������: ������ (Grinding) ---
public class GrindingState : PlayerBaseState
{
    public GrindingState(AdvancedPlayerController context) : base(context) { }

    private Vector3 _grindDirection; // ����������� �������� ����� ������
    private Vector3 _railAxis;       // ���, ����� ������� �������� ������

    public override void EnterState()
    {
        Ctx.animator.SetBool(Ctx.IsGrindingHash, true);

        // --- ����� ������ ����������� ��� ������ ---
        Transform rail = Ctx.LastGrindableRail;
        var railCollider = rail.GetComponent<Collider>();
        if (railCollider == null)
        {
            Debug.LogError("Grindable object is missing a Collider!", rail.gameObject);
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // ���������� ����� ������� ��� ����������. ��� � ����� ������������ ������.
        Vector3 size = railCollider.bounds.size;
        if (size.x > size.y && size.x > size.z)
        {
            _railAxis = rail.right; // ����� ������� ��� - X
        }
        else if (size.y > size.x && size.y > size.z)
        {
            _railAxis = rail.up; // ����� ������� ��� - Y
        }
        else
        {
            _railAxis = rail.forward; // ����� ������� ��� - Z
        }

        // ����������, � ����� ������� ��������� ����� ���� ���, �� ������ �������� ������
        float dot = Vector3.Dot(Ctx.Velocity, _railAxis);
        _grindDirection = (dot >= 0) ? _railAxis : -_railAxis;

        // ���������� ������������ ��������
        var velocity = Ctx.Velocity;
        velocity.y = 0;
        Ctx.Velocity = velocity;

        // ������������� ������������� � ������ ���
        SnapToRail();
    }

    public override void UpdateState()
    {
        if (Input.GetButtonDown("Jump"))
        {
            var velocity = _grindDirection * Ctx.grindSpeed; // ��������� ������� �� �������
            velocity.y = Ctx.jumpForce;
            Ctx.Velocity = velocity;
            Ctx.animator.SetTrigger(Ctx.JumpHash);
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        SnapToRail();

        // ���������, �� ����������� �� ������
        if (!Physics.Raycast(Ctx.transform.position, Vector3.down, 1.5f))
        {
            // ������������� ������� ����� ��������
            Ctx.Velocity = _grindDirection * Ctx.grindSpeed;
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // ��������
        Vector3 movement = _grindDirection * Ctx.grindSpeed * Time.deltaTime;
        Ctx.Controller.Move(movement);

        // �������
        Quaternion targetRotation = Quaternion.LookRotation(_grindDirection, Vector3.up);
        Ctx.transform.rotation = Quaternion.Slerp(Ctx.transform.rotation, targetRotation, Time.deltaTime * 15f);
    }

    private void SnapToRail()
    {
        Transform rail = Ctx.LastGrindableRail;
        if (rail == null) return;

        // �������� ��������� ������ �� ��� ������
        Vector3 playerToRailCenter = Ctx.transform.position - rail.position;
        float projection = Vector3.Dot(playerToRailCenter, _railAxis);
        Vector3 pointOnRailLine = rail.position + _railAxis * projection;

        // ���������������� �� �������� ����������
        float railTopY = rail.GetComponent<Collider>().bounds.max.y;
        Vector3 targetPosition = new Vector3(pointOnRailLine.x, railTopY, pointOnRailLine.z);

        // ������ �������������
        Ctx.transform.position = Vector3.Lerp(Ctx.transform.position, targetPosition, Time.deltaTime * 20f);
    }

    public override void ExitState()
    {
        Ctx.animator.SetBool(Ctx.IsGrindingHash, false);
    }
}


// --- ���������: ��� �� ����� (Wall-Running) ---
public class WallRunningState : PlayerBaseState
{
    private float _wallRunTimer;

    public WallRunningState(AdvancedPlayerController context) : base(context) { }

    public override void EnterState()
    {
        Ctx.animator.SetBool(Ctx.IsWallRunningHash, true);
        _wallRunTimer = Ctx.wallRunDuration;

        // ���������� ��� ����������� ����������� ���� �� �����.
        // WallNormal - ������ �� ����� � ���.
        // Vector3.up - ������ �����.
        // Vector3.Cross - ��������� ������������, ���� ������, ���������������� ���� ������.
        // ��� � ����� ������������ �������� ����� �����.
        Vector3 wallForward = Vector3.Cross(Ctx.WallNormal, Vector3.up);

        // ����������, � ����� ������� ������ (������ ��� ����� ����� �����)
        // ���������� � ������������ ������� ������ (������)
        if (Vector3.Dot(Ctx.cameraTransform.forward, wallForward) < 0)
        {
            wallForward = -wallForward; // ���� ������� � ��������������� �������, �����������
        }

        // ������������� �������� ��� ���� �� �����
        var velocity = wallForward * Ctx.wallRunSpeed;
        velocity.y = 0; // ���������� ������������ ��������, ����� �� ������ �����
        Ctx.Velocity = velocity;

        // ������������ ���������
        Ctx.transform.rotation = Quaternion.LookRotation(wallForward);
    }

    public override void UpdateState()
    {
        _wallRunTimer -= Time.deltaTime;

        // 1. �������� �� �������
        // ������ �����, ��� �� ������ �� �������� ����� (���������� ��������)
        if (_wallRunTimer <= 0)
        {
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // ������ �� �����
        if (Input.GetButtonDown("Jump"))
        {
            Ctx.animator.SetTrigger(Ctx.JumpHash);
            // ������ ���������� � ����������� �� ����� � �����
            Vector3 jumpDirection = (Ctx.WallNormal + Vector3.up).normalized;
            Ctx.Velocity = jumpDirection * Ctx.wallJumpForce; // ��������� �������� �������� ��� ������� ������������
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // 2. ������ ��������
        // ��������� ����, ����������� � �����, � ��������������
        Vector3 antiGravity = -Ctx.gravity * 0.5f * Time.deltaTime * Vector3.up;
        Ctx.Controller.Move((Ctx.Velocity + antiGravity) * Time.deltaTime);

        // ��������� ���������, ���� �� ����� �����, ����� �� ������ �� �������
        // ��� ����� ������� ��� � ������� �����
        if (!Physics.Raycast(Ctx.transform.position, -Ctx.WallNormal, 1.0f))
        {
            Ctx.SwitchState(Ctx.InAirState);
        }
    }

    public override void ExitState() 
    {
        Ctx.animator.SetBool(Ctx.IsWallRunningHash, false);
    }
}


#endregion