using UnityEngine;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimationController : MonoBehaviour
{
    // --- ������ �� ���������� � ������ ---
    // �� ������� �� ����������, �� ������ �� ����������, ����� ����� ���� ����� ��������,
    // �� �� �������� �������.
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public PlayerWallRun playerWallRun;
    [HideInInspector] public PlayerDash playerDash;
    [HideInInspector] public PlayerSlide playerSlide;
    [HideInInspector] public PlayerGroundedMovement playerGroundedMovement;
    [HideInInspector] public PlayerWallMovement playerWallMovement;
    // �������� ������ ������, ���� ��� ����� ����� �������

    // --- ���������� ���������� ---
    private bool wasGroundedLastFrame;

   

    // --- ���� ���������� ��������� ---
    // ��� ����� ������� ������ ������ � �����������.
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int IsWallRunning = Animator.StringToHash("IsWallRunning");
    private static readonly int IsWallSliding = Animator.StringToHash("IsWallSliding");
    private static readonly int IsSliding = Animator.StringToHash("IsSliding");
    private static readonly int IsGrinding = Animator.StringToHash("IsGrinding");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int IsWallOnRight = Animator.StringToHash("IsWallOnRight");
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int LandTrigger = Animator.StringToHash("Land");
    private static readonly int LandFlairTrigger = Animator.StringToHash("LandFlair"); 
    private static readonly int FlairIndex = Animator.StringToHash("FlairIndex");
    private static readonly int JumpFlairTrigger = Animator.StringToHash("JumpFlair");
    private static readonly int JumpFlairIndex = Animator.StringToHash("JumpFlairIndex");

    // --- ������ ���������� ����� UNITY ---

    void Awake()
    {
        // 1. �������� ��� ������ � ��������� ������ �� ���
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("FATAL ERROR: PlayerAnimationController �� ����� PlayerController �� �������! ����������.", this);
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("FATAL ERROR: PlayerController �� ���� ������������ Animator! ��������� PlayerController.Awake(). ����������.", this);
            enabled = false;
            return;
        }

        // �������� ������, ��� ����������� ��� �������� � ��� ������ � Update
        playerWallRun = GetComponent<PlayerWallRun>();
        playerDash = GetComponent<PlayerDash>();
        playerSlide = GetComponent<PlayerSlide>();
        playerGroundedMovement = GetComponent<PlayerGroundedMovement>();
        playerWallMovement = GetComponent<PlayerWallMovement>();
    }

    void OnEnable()
    {
        // 2. ������������� �� �������. ��������� ������ ������ ����� ���������.
        Debug.Log("PlayerAnimationController: �������� �� �������...");

        if (playerGroundedMovement != null) playerGroundedMovement.OnJump += TriggerJump;
        else Debug.LogWarning("�� ������� ����������� �� ������: PlayerGroundedMovement �� ������.");

        if (playerWallMovement != null) playerWallMovement.OnJump += TriggerJump;
        else Debug.LogWarning("�� ������� ����������� �� ������: PlayerWallMovement �� ������.");

        if (playerWallRun != null) playerWallRun.OnJump += TriggerJump;
        else Debug.LogWarning("�� ������� ����������� �� ������: PlayerWallRun �� ������.");

        if (playerSlide != null) playerSlide.OnJump += TriggerJump;
        else Debug.LogWarning("�� ������� ����������� �� ������: PlayerSlide �� ������.");

        if (playerSlide != null) playerSlide.OnSlideEnd += OnSlideFinished;
        else Debug.LogWarning("�� ������� ����������� �� ��������� ������: PlayerSlide �� ������.");

        // �������� ����� �������� �� OnDash, ���� ����������.
    }

    void OnDisable()
    {
        // 3. ������ ������������, ����� �������� ������ � ������ ������.
        Debug.Log("PlayerAnimationController: ������� �� �������...");

        if (playerGroundedMovement != null) playerGroundedMovement.OnJump -= TriggerJump;
        if (playerWallMovement != null) playerWallMovement.OnJump -= TriggerJump;
        if (playerWallRun != null) playerWallRun.OnJump -= TriggerJump;
        if (playerSlide != null) playerSlide.OnJump -= TriggerJump;
        if (playerSlide != null) playerSlide.OnSlideEnd -= OnSlideFinished;
    }

    void Start()
    {
        // 4. �������������� ���������, ����� �������� "���������" � ������ �����.
        if (animator != null)
        {
            UpdateAllParameters();
            wasGroundedLastFrame = playerController.IsGrounded;
        }
    }



    // 5. ���������� LateUpdate, ����� �������������� �������� ��������� ������ �� ����.
    void LateUpdate()
    {
        if (animator != null && playerController != null)
        {
            UpdateAllParameters();
        }
    }


    // --- �������� ������ ---


    private void OnSlideFinished()
    {
        // ����� ����� �������������, �� ���� �������� ������ �����������
        Debug.Log("<color=yellow>SLIDE ENDED, triggering landing logic.</color>");
        
        PlayLandingAnimation();
    }

    private void UpdateAllParameters()
    {
        // --- ���������� FLOAT ���������� ---
        // ��������� �������������� �������� ��� �������� ����/������
        var horizontalVelocity = new Vector3(playerController.PlayerVelocity.x, 0, playerController.PlayerVelocity.z);
        animator.SetFloat(Speed, horizontalVelocity.magnitude, 0.1f, Time.deltaTime);

        // ��������� ������������ �������� ��� �������� ������/�������,
        // ������, ��� �� ����� ��� ����� ���� ��� ���������.
        float verticalSpeedForAnimator = playerController.IsGrounded ? 0f : playerController.PlayerVelocity.y;
        animator.SetFloat(VerticalSpeed, verticalSpeedForAnimator, 0.1f, Time.deltaTime);

        // --- ���������� BOOL ���������� � ������������ ---

        // 1. ������� �������� "�����" ������ � ������� �������� ���������� �� �������.
        bool isGrinding = playerController.CurrentState == PlayerController.PlayerState.Grinding;
        bool isWallRunning = playerWallRun != null && playerWallRun.IsWallRunning;
        bool isWallSliding = playerController.IsWallSliding;
        bool isSliding = playerSlide != null && playerSlide.IsSliding;
        bool isDashing = playerDash != null && playerDash.IsDashing;

        // 2. ������������� ����� ��� ���� "������" ��������� � ��������.
        animator.SetBool(IsGrinding, isGrinding);
        animator.SetBool(IsWallRunning, isWallRunning);
        animator.SetBool(IsWallSliding, isWallSliding);
        animator.SetBool(IsSliding, isSliding);
        animator.SetBool(IsDashing, isDashing);

        // 3. ������ ��������� � ������������� ���� IsGrounded � ������ �����������.
        // �������� ��������� "�� �����" ��� ���������, ������ ���� �� ��������� �� �����
        // � ��� ���� �� ��������� ������ ��� ������ (������� ���� ����� �������� �����).
        bool isGroundedForAnimator = playerController.IsGrounded && !isGrinding && !isSliding;

 

        animator.SetBool(IsGrounded, isGroundedForAnimator);

        // 4. ������������ ����������� ������ ��� ���� �� ����� (����������� �������).
        if (isWallRunning)
        {
            // ��������� ������������ ����������, � ����� ������� �� ��������� ��������� ������� �����.
            float dot = Vector3.Dot(transform.right, playerWallRun.WallNormal);
            // ������� "�������" �� �����. ���� ��� ������ �� ���, dot ����� �������������.
            // ���� �� �����, ����� IsWallOnRight ���� true, ����� ����� ������, ������� ������ ���� `dot > 0`.
            // ���� �������� ���� �� ������ ����� "����������" (�������� ����������� �����), �� ����� ������������ `dot < 0`.
            // ������� `dot < 0` ��� � ������� ���.
            animator.SetBool(IsWallOnRight, dot < 0);
        }

        // 5. �������� ���������� ��� ���������, �� ��������� � ��������� (��������, �����������).
        HandleLanding();

        
    }



    private void HandleLanding()
    {
        bool isGroundedNow = playerController.IsGrounded;
        if (!wasGroundedLastFrame && isGroundedNow)
        {
            // ������ ���� ����� ������ �������� ���� ����� ������
            PlayLandingAnimation();
        }
        wasGroundedLastFrame = isGroundedNow;
    }



    private void PlayLandingAnimation()
    {
        // ���������, ��� �� �� � ������� (�� ������ ������, ����� �������� ������������)
        if (playerSlide != null && playerSlide.IsSliding) return;

        // ���������, ��� �� �� �����, � ��, ��������, ��������� � ������
        if (playerController.CurrentState != PlayerController.PlayerState.Grounded) return;

        // ������, ����� �������� ����������� ������
        float randomChance = UnityEngine.Random.Range(0f, 1f);
        if (randomChance <= 0.5f)
        {
            Debug.Log("<color=magenta>FLAIR ANIMATION TRIGGERED!</color>");
            int randomIndex = UnityEngine.Random.Range(0, 5); // �������� ��������: max (�� ������������).
            animator.SetFloat(FlairIndex, randomIndex);
            animator.SetTrigger(LandFlairTrigger);
            // �� ������ �� ��������� ����������, ��� ��� ��� ����� ���������� ������
        }
        else
        {
            Debug.Log("<color=green>NORMAL LANDING TRIGGERED!</color>");
            animator.SetTrigger(LandTrigger);
        }
    }

    // --- ������-��������� ������� ---
    private void TriggerJump()
    {
        if (animator == null) return; // �������� �� ������ ������

        // --- ����� ������ � ������ ---
        // ���������� ��������� ����� �� 0.0 �� 1.0
        float randomChance = UnityEngine.Random.Range(0f, 1f);

        if (randomChance <= 0.75f) // 75% ���� �� "��������" ������
        {
            Debug.Log("<color=cyan>FLAIR JUMP TRIGGERED!</color>");

            // �������� ��������� �������� �� ����
            int randomIndex = UnityEngine.Random.Range(0, 3); // ������ 0 ��� 1

            // ������������� ������ ��� Blend Tree
            animator.SetFloat(JumpFlairIndex, randomIndex);

            // ������� ������� ��� ��������� ������
            animator.SetTrigger(JumpFlairTrigger);
        }
        else // 25% ���� �� ������� ������
        {
            Debug.Log("<color=blue>NORMAL JUMP TRIGGERED!</color>");

            // ������� ������� ��� �������� ������
            animator.SetTrigger(JumpTrigger);
        }
    }
}