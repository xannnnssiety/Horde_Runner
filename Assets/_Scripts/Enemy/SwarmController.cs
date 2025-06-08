using UnityEngine;
using System.Collections.Generic;

public class SwarmController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float swarmMoveSpeed = 8f;      // Используется как базовая/фоллбэк скорость
    public float swarmRotationSpeed = 4f;

    private Transform playerTransform;
    private PlayerMovement playerMovementScript;
    private Rigidbody rb;
    private List<ProjectileEnemyAI> activeMembers = new List<ProjectileEnemyAI>();
    private bool initialized = false;

    private float currentPlayerSpeedFactor = 0.7f; // Дефолт, будет перезаписан из спавнера

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) { rb = gameObject.AddComponent<Rigidbody>(); }
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    public void Initialize(Transform targetPlayer, PlayerMovement pMovementScript, float initialSpeedForController, float speedFactorFromPlayer)
    {
        playerTransform = targetPlayer;
        playerMovementScript = pMovementScript;
        this.swarmMoveSpeed = initialSpeedForController; // Начальная скорость, рассчитанная спавнером
        this.currentPlayerSpeedFactor = speedFactorFromPlayer; // % от скорости игрока
        initialized = true;

        if (playerTransform == null || playerMovementScript == null)
        {
            Debug.LogWarning("SwarmController: Player Transform или PlayerMovement script не был передан! Рой будет двигаться с начальной скоростью: " + this.swarmMoveSpeed, gameObject);
        }
    }

    void FixedUpdate()
    {
        if (!initialized)
        {
            if (rb != null) rb.linearVelocity = Vector3.zero;
            return;
        }

        float currentActualMoveSpeed = this.swarmMoveSpeed; // По умолчанию используем скорость, заданную при спавне

        if (playerTransform != null && playerMovementScript != null)
        {
            float playerCurrentSpeed = playerMovementScript.currentMoveSpeed;
            currentActualMoveSpeed = playerCurrentSpeed * currentPlayerSpeedFactor;
            // Здесь можно добавить минимальную скорость для SwarmController, если это нужно
            // currentActualMoveSpeed = Mathf.Max(currentActualMoveSpeed, minSpeedForSwarmController);
        }
        else if (playerTransform == null) // Если игрок исчез (или не был найден изначально)
        {
            // Если игрока нет, контроллер может либо остановиться, либо продолжить движение с базовой скоростью
            // Для простоты, если игрока нет, а он был нужен, то останавливаемся
            if (rb != null) rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 directionToPlayer = (playerTransform.position - rb.position).normalized;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * swarmRotationSpeed));
        }

        rb.MovePosition(rb.position + transform.forward * currentActualMoveSpeed * Time.fixedDeltaTime);
    }

    public void RegisterMember(ProjectileEnemyAI member)
    {
        if (!activeMembers.Contains(member))
        {
            activeMembers.Add(member);
        }
    }

    public void UnregisterMember(ProjectileEnemyAI member)
    {
        if (activeMembers.Contains(member))
        {
            activeMembers.Remove(member);
        }
        if (initialized && activeMembers.Count == 0 && this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!initialized) return;
        // Можно добавить реакцию контроллера на столкновения
    }
}