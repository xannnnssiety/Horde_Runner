using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(PlayerController))]
public class PlayerGrind : MonoBehaviour
{
    [Header("Настройки грайнда")]
    [Tooltip("Множитель ускорения на рельсе (1.5 = на 50% быстрее)")]
    public float grindAccelerationMultiplier = 1.5f;
    public LayerMask grindableLayer;
    public float grindSearchRadius = 3f;

    // Ссылки на компоненты
    private PlayerController _controller;
    private CharacterController _characterController;

    // Приватные переменные состояния
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
        // Таймер перезарядки нужен, чтобы игрок не прилипал к рельсе сразу после прыжка с нее
        if (grindCooldownTimer > 0)
        {
            grindCooldownTimer -= Time.deltaTime;
        }
    }

    // Вызывается из Update() главного контроллера, когда персонаж на земле или в воздухе
    public void CheckForGrindStart()
    {
        if (_controller.CurrentState == PlayerController.PlayerState.Grinding || grindCooldownTimer > 0f) return;

        // Ищем рельсу под ногами
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 1.5f, grindableLayer))
        {
            // Начать грайнд можно, если мы не стоим неподвижно на земле
            if (!_controller.IsGrounded || _characterController.velocity.magnitude > 0.1f)
            {
                StartGrind(hit.transform);
            }
        }
    }

    // Вызывается из Update() главного контроллера, когда CurrentState == PlayerState.Grinding
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
            return; // Выходим. В следующем кадре первая проверка сработает и вызовет EndGrind.
        }

        HandleMovementOnRail();
        /*HandleGrindJump();*/


    }

    private void HandleSpeed()
    {
        // Ускоряемся на рельсе до максимальной скорости
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

        // Если мы скоро сойдем с текущей рельсы
        if (Vector3.Distance(lookAheadPoint, currentRailCollider.ClosestPoint(lookAheadPoint)) > lookAheadDistance)
        {
            // Ищем лучшую следующую рельсу
            Transform nextRail = FindBestRail(currentGrindRail);
            if (nextRail != null)
            {
                SwitchToRail(nextRail);
            }
            else
            {
                EndGrind(false); // Рельсы закончились
            }
        }
        else // Если мы еще на рельсе, проверим, нет ли рядом рельсы получше (для перепрыгивания)
        {
            Transform bestRail = FindBestRail();
            if (bestRail != null && bestRail != currentGrindRail)
            {
                // Если есть ввод в сторону новой рельсы, можно добавить логику перескока
                // Для простоты, пока просто переключаемся, если она ближе
                // SwitchToRail(bestRail); // Эту логику можно усложнить
            }
        }
    }

    private void HandleMovementOnRail()
    {
        // Прилипаем к рельсе
        Vector3 snapToPoint = currentGrindRail.GetComponent<Collider>().ClosestPoint(transform.position);
        // Используем Move, а не меняем transform.position, чтобы CharacterController знал о перемещении
        _characterController.Move(snapToPoint - transform.position);

        // Устанавливаем скорость движения вдоль рельсы
        _controller.PlayerVelocity = grindDirection * _controller.CurrentMoveSpeed;

        bool isManualRotationActive = _controller.Animator.GetCurrentAnimatorStateInfo(0).tagHash == manualRotationTagHash;
        // Плавно поворачиваем персонажа в направлении движения
        if (!isManualRotationActive)
        {
            // --- ИЗМЕНЕНИЕ ---
            // Возвращаем оригинальную логику поворота. Она обеспечивает быстрый, почти мгновенный разворот
            // вдоль рельсы, как и было у тебя раньше.
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
            Debug.LogError("Попытка начать грайнд на NULL рельсе!", this);
            return; // Не начинаем грайнд, если рельса недействительна
        }

        currentGrindRail = rail;
        _controller.SetState(PlayerController.PlayerState.Grinding);

        // Обнуляем вертикальную скорость
        var velocity = _controller.PlayerVelocity;
        velocity.y = 0;
        _controller.PlayerVelocity = velocity;



        // Определяем направление движения по рельсе
        float dot = Vector3.Dot(transform.forward, currentGrindRail.forward);
        grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
    }

    private void EndGrind(bool didJump)
    {
        if (currentGrindRail == null) return;

        // Устанавливаем скорость отрыва от рельсы
        _controller.PlayerVelocity = grindDirection * _controller.CurrentMoveSpeed;

        if (didJump)
        {
            // Если был прыжок, добавляем вертикальную скорость
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
        // Определяем новое направление
        float dot = Vector3.Dot(grindDirection, currentGrindRail.forward);
        grindDirection = (dot >= 0) ? currentGrindRail.forward : -currentGrindRail.forward;
    }

    private Transform FindBestRail(Transform railToIgnore = null)
    {
        var nearbyRails = Physics.OverlapSphere(transform.position, grindSearchRadius, grindableLayer);
        var bestRail = nearbyRails
            .Where(rail => rail.transform != railToIgnore) // Игнорируем ту, с которой ищем
            .OrderBy(rail => Vector3.Distance(transform.position, rail.ClosestPoint(transform.position)))
            .FirstOrDefault();

        return bestRail?.transform;
    }
}