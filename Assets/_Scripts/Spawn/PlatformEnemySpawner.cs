using System.Collections.Generic;
using UnityEngine;

public class PlatformEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int maxEnemiesToSpawn = 3; // �������� ������, ������� ������� ��� ��������� �� ���
    public float spawnDelay = 1.5f;     // �������� ����� ������ �������� ������ ����� ��������� ��������� (�������� ��� �����)

    [Header("Player Detection")]
    public float spawnActivationRadius = 20f; // ������, � ������� ������ ���� �����, ����� ��������� ������ ��������
                                              // ���� ������ ������ ���� ������, ��� PlatformGenerator.spawnTriggerDistance,
                                              // ����� ��������� ��� ������������, ����� ����� ������ � ���� ������.

    private List<Transform> enemySpawnPoints = new List<Transform>();
    private Transform playerTransform;
    private PlayerMovement playerMovementScript; // ��� ��������� ������ �� ������ ������
    private bool playerInRange = false;
    private bool initialSpawnAttempted = false;
    private int spawnedCount = 0; // ������� ������ ��� ���������� ��� ���������

    void Awake()
    {
        // ������� ��� �������� �������, ������� �������� ������� ������
        foreach (Transform child in transform)
        {
            // ��������� �� ����� ��� ����
            if (child.name.StartsWith("EnemySpawnPoint") || child.CompareTag("EnemySpawnPoint"))
            {
                enemySpawnPoints.Add(child);
            }
        }

        if (enemySpawnPoints.Count == 0)
        {
            Debug.LogWarning("�� ��������� " + gameObject.name + " �� ������� ����� ������ ������ (��� ������ ���������� � 'EnemySpawnPoint' ��� ����� ��� 'EnemySpawnPoint').");
        }
    }

    void Start()
    {
        // ������� ������ ���� ���
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // ���������, ��� � ������ ���� ��� "Player"
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerMovementScript = playerObj.GetComponent<PlayerMovement>();
            if (playerMovementScript == null)
            {
                Debug.LogError("PlatformEnemySpawner: �� ������� ������ � ����� 'Player' ����������� ������ PlayerMovement!");
                enabled = false; return;
            }
        }
        else
        {
            Debug.LogError("PlatformEnemySpawner �� ����� ����� ������! ���������, ��� � ������ ��� 'Player'.");
            enabled = false; // ��������� �������, ���� ��� ������
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab �� �������� � PlatformEnemySpawner �� " + gameObject.name);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (playerTransform == null || enemyPrefab == null || enemySpawnPoints.Count == 0 || spawnedCount >= maxEnemiesToSpawn || playerMovementScript == null)
        {
            return; // ������ ������, ��� ��� ���������� ��������, ��� ��� ������ �� ������ ������
        }

        // ���������, ����� �� ����� � ������ ��������� ������
        if (Vector3.Distance(transform.position, playerTransform.position) <= spawnActivationRadius)
        {
            playerInRange = true;
        }
        else
        {
            playerInRange = false;
            // initialSpawnAttempted = false; // ����� ����������, ���� ����� �������� �������� ��� �����/������
            return; // ����� �� � �������, ������ �� ������ (���� �� ����� ���������� initialSpawnAttempted)
        }


        if (playerInRange && !initialSpawnAttempted)
        {
            // ��������� �������� ��� ������ � ���������
            StartCoroutine(AttemptSpawnWithDelay());
            initialSpawnAttempted = true; // ������� ������ ����� ������� (��� ��� ��������)
        }
    }

    System.Collections.IEnumerator AttemptSpawnWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (!playerInRange || playerMovementScript == null) // ������������, ���� ����� ���� ��� ������ ������ ����� �� ����� ��������
        {
            initialSpawnAttempted = false; // ��������� ����� ����������, ���� ����� ��������
            yield break;
        }

        // ������������ ����� ������ ��� ����������� (�����������)
        ShuffleSpawnPoints();

        int enemiesSpawnedThisAttempt = 0;
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            if (spawnedCount >= maxEnemiesToSpawn) break; // ��� �������� ������ ��� ���� ���������

            // �������������� ��������: �������� ������ ���� ����� ������ ���� � ��������� ������� �� ������
            // (�� �� ������� ������, ����� ���� �� �������� ����� �� ������)
            float distanceToPlayerFromSpawnPoint = Vector3.Distance(spawnPoint.position, playerTransform.position);
            // ��������, ��� ����� ������ �� ������� ������ � �� ������� ������ (���� ����� ������ ��������)
            if (distanceToPlayerFromSpawnPoint > 3f && distanceToPlayerFromSpawnPoint < spawnActivationRadius + 10f) // 3f - ���. ���������, spawnActivationRadius + 10f - ����.
            {
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    // �������� ������ playerMovementScript � ������ �� ������
                    enemyAI.Initialize(playerTransform, playerMovementScript);
                    spawnedCount++;
                    enemiesSpawnedThisAttempt++;
                    Debug.Log("��������� ���� '" + newEnemy.name + "' �� ��������� '" + gameObject.name + "' � ����� '" + spawnPoint.name + "'");
                }
                else
                {
                    Debug.LogError("�� ������� ����� '" + enemyPrefab.name + "' ����������� ������ EnemyAI!", newEnemy);
                    Destroy(newEnemy); // ���������� ������������� �����
                }
            }

            // ����������� �� ���������� ������ �� ���� ������� ������ (���� ����� �������� �� ������ �� ���)
            // if (enemiesSpawnedThisAttempt >= 1) break; 
        }

        if (enemiesSpawnedThisAttempt == 0 && spawnedCount < maxEnemiesToSpawn) // ���� �� ������� ����������, �� ����� ��� �� ���������
        {
            // ����� �������� ����, ����� ���������� ����� ��� ��������� Update, ����� ����� � �������.
            // ��� �������, ���� ����� ����������� � ����� ������ �������� ���������� ������������.
            initialSpawnAttempted = false;
            Debug.Log("�� ��������� " + gameObject.name + " �� ������� ���������� ����� ��� ������ � ���� ���. ������� ����� ���������.");
        }
    }

    void ShuffleSpawnPoints()
    {
        for (int i = 0; i < enemySpawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, enemySpawnPoints.Count);
            Transform temp = enemySpawnPoints[i];
            enemySpawnPoints[i] = enemySpawnPoints[randomIndex];
            enemySpawnPoints[randomIndex] = temp;
        }
    }
}