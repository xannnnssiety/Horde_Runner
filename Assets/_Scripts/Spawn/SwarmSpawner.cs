using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwarmSpawner : MonoBehaviour
{
    [Header("Swarm Settings")]
    public GameObject projectileEnemyPrefab;
    public GameObject swarmControllerPrefab;
    public int minSwarmSize = 12;
    public int maxSwarmSize = 16;
    public float delayBetweenSpawnsInSwarm = 0.05f;

    [Header("Swarm Controller Speed")]
    public float swarmControllerSpeedFactor = 0.7f; // 70% от скорости игрока дл€ контроллера
    public float minSwarmControllerSpeed = 6f;      // ћинимальна€ скорость дл€ контроллера ро€

    [Header("Formation Shape ( учность)")]
    public int enemiesPerRow = 4;
    public float horizontalSpacing = 1.5f;
    public float verticalSpacing = 1.5f;

    [Header("Activation")]
    public float activationRadius = 25f;
    public float spawnDelay = 1.0f;
    public bool onlySpawnOncePerPlatform = true;

    private List<Transform> platformSpawnPoints = new List<Transform>();
    private Transform playerTransform;
    private PlayerMovement playerMovementScript;
    private bool playerInRange = false;
    private bool initialSpawnAttempted = false;
    private bool hasSpawned = false;
    private Coroutine activeSwarmSpawnCoroutine = null;

    void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("SwarmSpawnPoint")) { platformSpawnPoints.Add(child); }
        }
        if (platformSpawnPoints.Count == 0) { enabled = false; }
    }

    void Start()
    {
        if (!enabled) return;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerMovementScript = playerObj.GetComponent<PlayerMovement>();
            if (playerMovementScript == null) { Debug.LogError("SwarmSpawner: Ќа игроке отсутствует PlayerMovement script!"); enabled = false; return; }
        }
        else { Debug.LogError("SwarmSpawner: »грок не найден!"); enabled = false; return; }

        if (projectileEnemyPrefab == null) { Debug.LogError("SwarmSpawner: Prefab projectileEnemy не назначен!"); enabled = false; return; }
    }

    void Update()
    {
        if (!enabled || playerTransform == null || playerMovementScript == null || projectileEnemyPrefab == null || platformSpawnPoints.Count == 0) return;
        if (onlySpawnOncePerPlatform && hasSpawned) return;
        if (activeSwarmSpawnCoroutine != null) return;

        bool playerCurrentlyInRange = (Vector3.Distance(transform.position, playerTransform.position) <= activationRadius);
        if (playerCurrentlyInRange) { playerInRange = true; }
        else { playerInRange = false; initialSpawnAttempted = false; return; }

        if (playerInRange && !initialSpawnAttempted)
        {
            activeSwarmSpawnCoroutine = StartCoroutine(SpawnSwarmWithControllerCoroutine());
            initialSpawnAttempted = true;
        }
    }

    IEnumerator SpawnSwarmWithControllerCoroutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (!playerInRange || (onlySpawnOncePerPlatform && hasSpawned) || platformSpawnPoints.Count == 0 || playerTransform == null || playerMovementScript == null)
        {
            initialSpawnAttempted = false; activeSwarmSpawnCoroutine = null; yield break;
        }

        Transform chosenPlatformSpawnPoint = platformSpawnPoints[Random.Range(0, platformSpawnPoints.Count)];
        Vector3 swarmControllerSpawnPos = chosenPlatformSpawnPoint.position;

        GameObject swarmControllerGO;
        if (swarmControllerPrefab != null)
        {
            swarmControllerGO = Instantiate(swarmControllerPrefab, swarmControllerSpawnPos, Quaternion.LookRotation(playerTransform.position - swarmControllerSpawnPos));
        }
        else
        {
            swarmControllerGO = new GameObject("SwarmController_Runtime_" + Time.frameCount);
            swarmControllerGO.transform.position = swarmControllerSpawnPos;
            swarmControllerGO.transform.rotation = Quaternion.LookRotation(playerTransform.position - swarmControllerSpawnPos);
            swarmControllerGO.AddComponent<SwarmController>();
        }
        SwarmController currentSwarmController = swarmControllerGO.GetComponent<SwarmController>();
        if (currentSwarmController == null) { Destroy(swarmControllerGO); initialSpawnAttempted = false; activeSwarmSpawnCoroutine = null; yield break; }

        float playerCurrentSpeed = playerMovementScript.currentMoveSpeed;
        float controllerInitialSpeed = Mathf.Max(playerCurrentSpeed * swarmControllerSpeedFactor, minSwarmControllerSpeed);
        currentSwarmController.Initialize(playerTransform, playerMovementScript, controllerInitialSpeed, swarmControllerSpeedFactor);

        int swarmSize = Random.Range(minSwarmSize, maxSwarmSize + 1);

        for (int i = 0; i < swarmSize; i++)
        {
            if (playerTransform == null || currentSwarmController == null) { activeSwarmSpawnCoroutine = null; yield break; }

            int rowIndex = i / enemiesPerRow;
            int colIndex = i % enemiesPerRow;
            float xOffset = (colIndex - (enemiesPerRow - 1) * 0.5f) * horizontalSpacing;
            float zOffset = -rowIndex * verticalSpacing; // ќтрицательное дл€ движени€ назад от "переда" контроллера
            Vector3 localFormationOffset = new Vector3(xOffset, 0, zOffset);
            // spawnClusterRadius больше не нужен дл€ определени€ localFormationOffset,
            // так как начальный разброс теперь очень мал и задан в ProjectileEnemyAI.Initialize

            // —павним врага в начальной позиции контроллера, Initialize в ProjectileEnemyAI его разместит.
            GameObject enemyGO = Instantiate(projectileEnemyPrefab, currentSwarmController.transform.position, currentSwarmController.transform.rotation);
            ProjectileEnemyAI projEnemy = enemyGO.GetComponent<ProjectileEnemyAI>();
            if (projEnemy != null)
            {
                projEnemy.Initialize(currentSwarmController, localFormationOffset);
            }
            else
            {
                Debug.LogError("ѕрефаб врага не содержит скрипт ProjectileEnemyAI!", enemyGO); Destroy(enemyGO);
            }

            if (i < swarmSize - 1) { yield return new WaitForSeconds(delayBetweenSpawnsInSwarm); }
        }

        if (onlySpawnOncePerPlatform) { hasSpawned = true; }
        activeSwarmSpawnCoroutine = null;
    }
}