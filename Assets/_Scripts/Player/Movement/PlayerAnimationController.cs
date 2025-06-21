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

    private void UpdateAllParameters()
    {
        // --- ���������� FLOAT ���������� ---
        var horizontalVelocity = new Vector3(playerController.PlayerVelocity.x, 0, playerController.PlayerVelocity.z);
        animator.SetFloat(Speed, horizontalVelocity.magnitude, 0.1f, Time.deltaTime); // ��������� �����������

        float verticalSpeedForAnimator = playerController.IsGrounded ? 0f : playerController.PlayerVelocity.y;
        animator.SetFloat(VerticalSpeed, verticalSpeedForAnimator, 0.1f, Time.deltaTime); // � �����

        // --- ���������� BOOL ���������� ---
        animator.SetBool(IsGrounded, playerController.IsGrounded);
        animator.SetBool(IsWallSliding, playerController.IsWallSliding);

        // ��������� ������ ����� ��������������, ����� �������� NullReferenceException
        if (playerWallRun != null) animator.SetBool(IsWallRunning, playerWallRun.IsWallRunning);
        if (playerSlide != null) animator.SetBool(IsSliding, playerSlide.IsSliding);
        if (playerDash != null) animator.SetBool(IsDashing, playerDash.IsDashing);

        animator.SetBool(IsGrinding, playerController.CurrentState == PlayerController.PlayerState.Grinding);

        if (playerWallRun != null && playerWallRun.IsWallRunning)
        {
            float dot = Vector3.Dot(transform.right, playerWallRun.WallNormal);
            animator.SetBool(IsWallOnRight, dot < 0);
        }

        // --- ��������� ���������, �� ��������� � ��������� ---
        HandleLanding();
    }

    private void HandleLanding()
    {
        bool isGroundedNow = playerController.IsGrounded;
        if (!wasGroundedLastFrame && isGroundedNow)
        {
            // ���������, ��� �� �� ��������� � �������, ������� ���� ���������� � �����������
            if (playerSlide == null || !playerSlide.IsSliding)
            {
                animator.SetTrigger(LandTrigger);
            }
        }
        wasGroundedLastFrame = isGroundedNow;
    }

    // --- ������-��������� ������� ---
    private void TriggerJump()
    {
        // ���������, ��� � ��� ���� ��������, ������ ��� �������� �������
        if (animator != null)
        {
            Debug.Log("<color=cyan>ANIMATION: Jump Triggered!</color>");
            animator.SetTrigger(JumpTrigger);
        }
    }
}