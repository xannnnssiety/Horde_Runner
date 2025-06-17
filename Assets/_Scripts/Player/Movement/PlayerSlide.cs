using UnityEngine;


[RequireComponent(typeof(PlayerController))]
public class PlayerSlide : MonoBehaviour
{
    [Header("��������� �������")]
    [Tooltip("�������� �� ����� ������� �� �����")]
    public float slideSpeed = 10f;
    [Tooltip("��� ����� ������ ������ � ��������")]
    public float slideDuration = 0.7f;
    [Tooltip("����� ����������� ������� � ��������")]
    public float slideCooldown = 1f;
    [Tooltip("��������� ���������� ��� ��������� � ������� (����� ������� ������)")]
    public float diveGravity = -200f;

    // ��������� ��������, ����� ���������� � ������ ������ ����� � ����� ���������
    public bool IsSliding { get; private set; }

    // ������
    private PlayerController _controller;
    private CharacterController _characterController;

    // ���������� ����������
    private float slideTimer;
    private float cooldownTimer;
    private bool isDiving; // ����, ������������, ��� �� ������������ ������ � ������� � ������

    // ID ��������
    private readonly int animIDSlide = Animator.StringToHash("Slide");

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    public void TickUpdate()
    {
        // --- ����� ������ ��� ���������� ������� ---
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // ��������� ����, ������ ���� �� �� � ������ ������ ���������,
        // ����� �� ������� � ������� ������
        if (!IsSliding && cooldownTimer <= 0 && _controller.CurrentState != PlayerController.PlayerState.Grinding)
        {
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                StartSlide();
            }
        }

        UpdateSlideState();
    }

    private void StartSlide()
    {
        slideTimer = slideDuration;

        if (_controller.IsGrounded)
        {
            // ���� �� �� �����, ����� �������� ���������
            BeginGroundSlide();
        }
        else
        {
            // ���� �� � �������, �������� "�����������"
            isDiving = true;
        }
    }

private void UpdateSlideState()
{
        if (isDiving && !_controller.IsGrounded)
        {
            // ���� �����������
        }
        else if (isDiving && _controller.IsGrounded)
        {
            isDiving = false; // <-- ���������� ��������
            BeginGroundSlide();
        }

        // ���� �� � ��������� ���������� �� �����
        if (IsSliding)
    {
        // --- ����� �������� �� ������ ---
        if (Input.GetButtonDown("Jump"))
        {
            // �������� ������ ������ �� �����������
            float jumpHeight = _controller.jumpHeight;
            
            // ������������ ������������ �������� ������
            float jumpVelocityY = Mathf.Sqrt(jumpHeight * -2f * _controller.GravityValue);

            // �������� ������� �������������� �������� ������
            Vector3 currentHorizontalVelocity = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z);
            
            // ����������� �������������� �������� �� ������ � ������������ �� ������
            _controller.PlayerVelocity = currentHorizontalVelocity + Vector3.up * jumpVelocityY;
            
            // ���������� ����������� �����
            EndSlide();
            
            // �������� �����������, ��� �� ������ � �������
            _controller.SetState(PlayerController.PlayerState.InAir);
            
            // ��������� �������� ������
            _controller.Animator.SetTrigger("Jump"); // ��������������, ��� � ��� ���� ������� Jump

            return; // ������� �� ������, ����� �� ������������ ��������� ������ ������
        }

        slideTimer -= Time.deltaTime;
        
        if (slideTimer <= 0)
        {
            EndSlide();
        }
        else
        {
            Vector3 slideVelocity = transform.forward * slideSpeed;
            _controller.PlayerVelocity = new Vector3(slideVelocity.x, _controller.PlayerVelocity.y, slideVelocity.z);
        }
    }
}

    private void BeginGroundSlide()
    {
        IsSliding = true;
        _controller.Animator.SetBool(animIDSlide, true); // ���������� Bool, � �� Trigger

        // ��������, ����������� �������� ������ ����������, ����� �������� "���������"
        // ��������: _characterController.height = 0.8f;
    }

    private void EndSlide()
    {
        // ���������, ������������� �� �� ���� � ��������� ����������, ����� �� ��������� ������� ��������
        if (IsSliding)
        {
            cooldownTimer = slideCooldown; 
        }

        IsSliding = false;
        _controller.Animator.SetBool("Slide", false); // ����� ������������ animIDSlide

        // ���������� ������ ����������, ���� ������
        // ��������: _characterController.height = 1.8f;
    }

    // ��������� �����, ������� ����� �������� ����������, ����� ��������� ����������
    public float GetCurrentGravity()
    {
        return isDiving ? diveGravity : _controller.GravityValue;
    }
}