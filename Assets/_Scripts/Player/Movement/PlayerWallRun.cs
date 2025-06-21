using UnityEngine;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerWallRun : MonoBehaviour
{
    [Header("��������� ���� �� ������")]
    public string wallRunnableTag = "WallRunnable";
    public float wallAttractionForce = 20f;
    public float wallJumpSideForce = 12f; // �������� �������� �� ��������� ��� ����� ������ �������

    [Header("�������� ���� ��� ����")]
    [Range(1f, 90f)]
    public float maxAngleForWallRun = 45f;
    [Tooltip("���� ������� ������ ��������� �� ����� ���� �� �����")]
    public float playerTiltAngle = 15f;

    // ��������� ��������
    public bool IsWallRunning { get; private set; }
    public Vector3 WallNormal { get; private set; }
    public event Action OnJump;
    // ������
    private PlayerController _controller;

    // ���������� ����������
    private Vector3 wallRunDirection;
    private float wallJumpCooldownTimer; // ������ "����������" ����� ������
    private Coroutine resetTiltCoroutine;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    public void TickUpdate()
    {
        // ��������� ������ ����������
        if (wallJumpCooldownTimer > 0)
        {
            wallJumpCooldownTimer -= Time.deltaTime;
            // ���� �� � ���� ����������, ������ ������ �� ������.
            // �������� ����� �� ������� �� ������.
            return;
        }

        // ��������� ����� ������ ���� �� �� �����
        if (!_controller.IsGrounded)
        {
            CheckForWallAndManageState();
        }
        else if (IsWallRunning)
        {
            // ���� ������������, ���������� ���
            StopWallRun();
        }
    }

    private void CheckForWallAndManageState()
    {
        bool isWallRight = Physics.Raycast(transform.position, transform.right, out RaycastHit rightWallHit, 1f) && rightWallHit.collider.CompareTag(wallRunnableTag);
        bool isWallLeft = Physics.Raycast(transform.position, -transform.right, out RaycastHit leftWallHit, 1f) && leftWallHit.collider.CompareTag(wallRunnableTag);

        if (IsWallRunning)
        {
            // ���� �� ��� �����, ���������� ��� ��� ���������������
            // ���������, �� �� �� ����� ��� ��� �����
            if ((isWallRight && rightWallHit.normal == WallNormal) || (isWallLeft && leftWallHit.normal == WallNormal))
            {
                ContinueWallRun();
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            // ���� �� �� �����, ���������, ����� �� ������
            if (CanStartWallRun(isWallRight, isWallLeft, rightWallHit, leftWallHit))
            {
                StartWallRun(isWallRight ? rightWallHit : leftWallHit);
            }
        }
    }

    private bool CanStartWallRun(bool isWallRight, bool isWallLeft, RaycastHit rightWallHit, RaycastHit leftWallHit)
    {
        if (!isWallRight && !isWallLeft) return false;

        RaycastHit activeWallHit = isWallRight ? rightWallHit : leftWallHit;
        Vector3 wallDirection = Vector3.Cross(activeWallHit.normal, Vector3.up);
        float angle = Vector3.Angle(transform.forward, wallDirection);
        float angleReversed = Vector3.Angle(transform.forward, -wallDirection);
        float horizontalSpeed = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z).magnitude;

        return Mathf.Min(angle, angleReversed) < maxAngleForWallRun && horizontalSpeed > 2f;
    }

    private void StartWallRun(RaycastHit hit)
    {
        if (resetTiltCoroutine != null)
        {
            StopCoroutine(resetTiltCoroutine);
            resetTiltCoroutine = null;
        }

        IsWallRunning = true;
        _controller.SetState(PlayerController.PlayerState.WallRunning);

        WallNormal = hit.normal;
        wallRunDirection = Vector3.Cross(WallNormal, Vector3.up);
        if (Vector3.Dot(wallRunDirection, transform.forward) < 0)
        {
            wallRunDirection = -wallRunDirection;
        }
    }

    private void ContinueWallRun()
    {
        // --- ������ ---
        if (Input.GetButtonDown("Jump"))
        {
            
            HandleWallJump();
            return;
        }

        // --- �������� ---
        UpdateSpeed();
        Vector3 runVelocity = wallRunDirection * _controller.CurrentMoveSpeed;
        Vector3 attractionVelocity = -WallNormal * wallAttractionForce;
        _controller.PlayerVelocity = runVelocity + attractionVelocity; // Y-������������ ��������� � runVelocity, ���� wallRunDirection �������������
        _controller.PlayerVelocity = new Vector3(_controller.PlayerVelocity.x, 0, _controller.PlayerVelocity.z); // �������������� ��������� Y

        // --- ������� ---
        Quaternion lookRotation = Quaternion.LookRotation(wallRunDirection);

        // 2. ���������� ������
        // ������, ������ ��� ����� �����, ����� ����������� � ������ �������
        bool isWallOnRight = Vector3.Dot(WallNormal, transform.right) > 0;
        float tiltAngle = isWallOnRight ? -playerTiltAngle : playerTiltAngle;  // ������ � ��������. ����� ������� � ���������.

        // 3. ������� ���������� ��� �������
        Quaternion tilt = Quaternion.Euler(0, 0, tiltAngle);

        // 4. ����������� ������� � ������
        Quaternion targetRotation = lookRotation * tilt;

        // 5. ������ ��������� ��������� �������
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);


    }

    private void HandleWallJump()
    {
        // --- ����� ������ � ������ ---
        // 1. ���������� ���������� ��� �� �����
        StopWallRun();

        // 2. ������������ � ��������� �������� ������
        Vector3 sidewaysForce = WallNormal * wallJumpSideForce;
        float verticalVelocity = Mathf.Sqrt(_controller.jumpHeight * -2f * _controller.GravityValue);
        _controller.PlayerVelocity = new Vector3(sidewaysForce.x, verticalVelocity, sidewaysForce.z);

        OnJump?.Invoke();

        // 3. ��������� ������ "����������" �� ����� �������� �����
        wallJumpCooldownTimer = 0.2f; // � ������� 0.2� �� ������ �� ����� ����������� ������ ���� �� ����� �������

        _controller.Animator.SetTrigger("Jump");
    }

    private void UpdateSpeed()
    {
        float targetSpeed = _controller.maxMoveSpeed;
        float newSpeed = Mathf.MoveTowards(_controller.CurrentMoveSpeed, targetSpeed, _controller.speedChangeRate * Time.deltaTime);
        _controller.CurrentMoveSpeed = newSpeed;
    }

    private void StopWallRun()
    {
        if (!IsWallRunning) return;
        IsWallRunning = false;

        if (resetTiltCoroutine == null)
        {
            resetTiltCoroutine = StartCoroutine(ResetTilt());
        }

        // ��������� � ��������� ������, ������ ���� �� �� �� �����
        if (!_controller.IsGrounded && _controller.CurrentState == PlayerController.PlayerState.WallRunning)
        {
            _controller.SetState(PlayerController.PlayerState.InAir);
        }
    }

    private System.Collections.IEnumerator ResetTilt()
    {
        Quaternion currentRotation = transform.rotation;
        // ������� ������� - ��� ������� ������� �� Y, �� � ������� �������� �� X � Z
        Quaternion targetRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y, 0);

        float timer = 0f;
        float duration = 0.25f; // ��� ������ �����������

        while (timer < duration)
        {
            // ������ ������������� � ������������� ���������
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, timer / duration);
            timer += Time.deltaTime;
            yield return null; // ���� ���������� �����
        }

        // ����������� ������ ��������� ���������
        transform.rotation = targetRotation;
        resetTiltCoroutine = null; // ���������� ������ �� ��������
    }

}