using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerGrind : MonoBehaviour
{
    [Header("��������� �������")]
    [Tooltip("��������� ��������� �� ������ (1.5 = �� 50% �������)")]
    public float grindAccelerationMultiplier = 1.5f;
    public LayerMask grindableLayer;
    public float grindSearchRadius = 3f;

    // ������ �� ����������
    private PlayerController _controller;
    private CharacterController _characterController;

    // ��������� ���������� ���������
    private Transform currentGrindRail;
    private Vector3 grindDirection;
    private float grindCooldownTimer;
    private const float GRIND_COOLDOWN = 0.2f;
    public event Action OnJump;

    private readonly int manualRotationTagHash = Animator.StringToHash("ManualRootRotation");
    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // ������ ����������� �����, ����� ����� �� �������� � ������ ����� ����� ������ � ���
        if (grindCooldownTimer > 0)
        {
            grindCooldownTimer -= Time.deltaTime;
        }
    }

    // ���������� �� Update() �������� �����������, ����� �������� �� ����� ��� � �������
    public void CheckForGrindStart()
    {
        if (_controller.CurrentState == PlayerController.PlayerState.Grinding || grindCooldownTimer > 0f) return;

        // ���� ������ ��� ������
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 1.5f, grindableLayer))
        {
            // ������ ������ �����, ���� �� �� ����� ���������� �� �����
            if (!_controller.IsGrounded || _characterController.velocity.magnitude > 0.1f)
            {
                StartGrind(hit.transform);
            }
        }
    }

    // ���������� �� Update() �������� �����������, ����� CurrentState == PlayerState.Grinding
    public void TickUpdate()
    {
        if (currentGrindRail == null) { EndGrind(false); return; }

        if (Input.GetButtonDown("Jump"))
        {
            EndGrind(true);
            return;
        }

        HandleSpeed();
        HandleRailSwitching();

        if (currentGrindRail == null)
        {
            return; // �������. � ��������� ����� ������ �������� ��������� � ������� EndGrind.
        }

        HandleMovementOnRail();
        /*HandleGrindJump();*/


    }

    private void HandleSpeed()
    {
        // ���������� �� ������ �� ������������ ��������
        float targetSpeed = _controller.maxMoveSpeed;
        float speedChangeRate = _controller.speedChangeRate;

        float newSpeed = Mathf.MoveTowards(_controller.CurrentMoveSpeed, targetSpeed, speedChangeRate * grindAccelerationMultiplier * Time.deltaTime);
        _controller.CurrentMoveSpeed = newSpeed;
    }

    private void HandleRailSwitching()
    {
        const float lookAheadDistance = 0.5f;
        Vector3 lookAheadPoint = transform.position + grindDirection * lookAheadDistance;
        Collider currentRailCollider = currentGrindRail.GetComponent<Collider>();

        // ���� �� ����� ������ � ������� ������
        if (Vector3.Distance(lookAheadPoint, currentRailCollider.ClosestPoint(lookAheadPoint)) > lookAheadDistance)
        {
            // ���� ������ ��������� ������
            Transform nextRail = FindBestRail(currentGrindRail);
            if (nextRail != null)
            {
                SwitchToRail(nextRail);
            }
            else
            {
                EndGrind(false); // ������ �����������
            }
        }
        else // ���� �� ��� �� ������, ��������, ��� �� ����� ������ ������� (��� ��������������)
        {
            Transform bestRail = FindBestRail();
            if (bestRail != null && bestRail != currentGrindRail)
            {
                // ���� ���� ���� � ������� ����� ������, ����� �������� ������ ���������
                // ��� ��������, ���� ������ �������������, ���� ��� �����
                // SwitchToRail(bestRail); // ��� ������ ����� ���������
            }
        }
    }

    private void HandleMovementOnRail()
    {
        // ��������� � ������
        Vector3 snapToPoint = currentGrindRail.GetComponent<Collider>().ClosestPoint(transform.position);
        // ���������� Move, � �� ������ transform.position, ����� CharacterController ���� � �����������
        _characterController.Move(snapToPoint - transform.position);

        // ������������� �������� �������� ����� ������
        _controller.PlayerVelocity = grindDirection * _controller.CurrentMoveSpeed;

        bool isManualRotationActive = _controller.Animator.GetCurrentAnimatorStateInfo(0).tagHash == manualRotationTagHash;
        // ������ ������������ ��������� � ����������� ��������
        if (!isManualRotationActive)
        {
            // --- ��������� ---
            // ���������� ������������ ������ ��������. ��� ������������ �������, ����� ���������� ��������
            // ����� ������, ��� � ���� � ���� ������.
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(grindDirection), _controller.turnSmoothTime * 15f);
        }
        /*transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(grindDirection), _controller.turnSmoothTime * 15f);*/
    }

    private void HandleGrindJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            OnJump?.Invoke();
            EndGrind(true);

        }
    }

    private void StartGrind(Transform rail)
    {
        if (rail == null)
        {
            Debug.LogError("������� ������ ������ �� NULL ������!", this);
            return; // �� �������� ������, ���� ������ ���������������
        }

        currentGrindRail = rail;
        _controller.SetState(PlayerController.PlayerState.Grinding);

        // �������� ������������ ��������
        var velocity = _controller.PlayerVelocity;
        velocity.y = 0;
        _controller.PlayerVelocity = velocity;



        // ���������� ����������� �������� �� ������
        float dot = Vector3.Dot(transform.forward, currentGrindRail.forward);
        grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
    }

    private void EndGrind(bool didJump)
    {
        if (currentGrindRail == null) return;

        // ������������� �������� ������ �� ������
        _controller.PlayerVelocity = grindDirection * _controller.CurrentMoveSpeed;

        if (didJump)
        {
            // ���� ��� ������, ��������� ������������ ��������
            var velocity = _controller.PlayerVelocity;
            float jumpHeight = _controller.jumpHeight;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * _controller.GravityValue);
            _controller.PlayerVelocity = velocity;
        }

        currentGrindRail = null;
        _controller.SetState(PlayerController.PlayerState.InAir);
        
        grindCooldownTimer = GRIND_COOLDOWN;
    }

    private void SwitchToRail(Transform nextRail)
    {
        currentGrindRail = nextRail;
        // ���������� ����� �����������
        float dot = Vector3.Dot(grindDirection, currentGrindRail.forward);
        grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
    }

    private Transform FindBestRail(Transform railToIgnore = null)
    {
        var nearbyRails = Physics.OverlapSphere(transform.position, grindSearchRadius, grindableLayer);
        var bestRail = nearbyRails
            .Where(rail => rail.transform != railToIgnore) // ���������� ��, � ������� ����
            .OrderBy(rail => Vector3.Distance(transform.position, rail.ClosestPoint(transform.position)))
            .FirstOrDefault();

        return bestRail?.transform;
    }
}