using System.Collections.Generic;
using UnityEngine;

public class PlatformEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int maxEnemiesToSpawn = 3; // Максимум врагов, которых спавнит ЭТА платформа за раз
    public float spawnDelay = 1.5f;     // Задержка перед первой попыткой спавна после активации платформы (уменьшил для теста)

    [Header("Player Detection")]
    public float spawnActivationRadius = 20f; // Радиус, в котором должен быть игрок, чтобы платформа начала спавнить
                                              // Этот радиус должен быть меньше, чем PlatformGenerator.spawnTriggerDistance,
                                              // чтобы платформа уже существовала, когда игрок входит в этот радиус.

    private List<Transform> enemySpawnPoints = new List<Transform>();
    private Transform playerTransform;
    private PlayerMovement playerMovementScript; // Для получения ссылки на скрипт игрока
    private bool playerInRange = false;
    private bool initialSpawnAttempted = false;
    private int spawnedCount = 0; // Сколько врагов уже заспавнила эта платформа

    void Awake()
    {
        // Находим все дочерние объекты, которые являются точками спавна
        foreach (Transform child in transform)
        {
            // Проверяем по имени или тегу
            if (child.name.StartsWith("EnemySpawnPoint") || child.CompareTag("EnemySpawnPoint"))
            {
                enemySpawnPoints.Add(child);
            }
        }

        if (enemySpawnPoints.Count == 0)
        {
            Debug.LogWarning("На платформе " + gameObject.name + " не найдено точек спавна врагов (имя должно начинаться с 'EnemySpawnPoint' или иметь тег 'EnemySpawnPoint').");
        }
    }

    void Start()
    {
        // Находим игрока один раз
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // Убедитесь, что у игрока есть тег "Player"
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerMovementScript = playerObj.GetComponent<PlayerMovement>();
            if (playerMovementScript == null)
            {
                Debug.LogError("PlatformEnemySpawner: На объекте игрока с тегом 'Player' отсутствует скрипт PlayerMovement!");
                enabled = false; return;
            }
        }
        else
        {
            Debug.LogError("PlatformEnemySpawner не может найти игрока! Убедитесь, что у игрока тег 'Player'.");
            enabled = false; // Отключаем спавнер, если нет игрока
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab не назначен в PlatformEnemySpawner на " + gameObject.name);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (playerTransform == null || enemyPrefab == null || enemySpawnPoints.Count == 0 || spawnedCount >= maxEnemiesToSpawn || playerMovementScript == null)
        {
            return; // Нечего делать, или уже заспавнили максимум, или нет ссылки на скрипт игрока
        }

        // Проверяем, вошел ли игрок в радиус активации спавна
        if (Vector3.Distance(transform.position, playerTransform.position) <= spawnActivationRadius)
        {
            playerInRange = true;
        }
        else
        {
            playerInRange = false;
            // initialSpawnAttempted = false; // Можно сбрасывать, если нужно повторно спавнить при входе/выходе
            return; // Игрок не в радиусе, ничего не делаем (если не нужно сбрасывать initialSpawnAttempted)
        }


        if (playerInRange && !initialSpawnAttempted)
        {
            // Запускаем корутину для спавна с задержкой
            StartCoroutine(AttemptSpawnWithDelay());
            initialSpawnAttempted = true; // Попытка спавна будет сделана (или уже делается)
        }
    }

    System.Collections.IEnumerator AttemptSpawnWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (!playerInRange || playerMovementScript == null) // Перепроверка, если игрок ушел или скрипт игрока исчез за время задержки
        {
            initialSpawnAttempted = false; // Позволить снова попытаться, если игрок вернется
            yield break;
        }

        // Перемешиваем точки спавна для случайности (опционально)
        ShuffleSpawnPoints();

        int enemiesSpawnedThisAttempt = 0;
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            if (spawnedCount >= maxEnemiesToSpawn) break; // Уже достигли лимита для этой платформы

            // Дополнительная проверка: спавнить только если точка спавна тоже в некотором радиусе от игрока
            // (но не слишком близко, чтобы враг не появился прямо на игроке)
            float distanceToPlayerFromSpawnPoint = Vector3.Distance(spawnPoint.position, playerTransform.position);
            // Убедимся, что точка спавна не слишком близко и не слишком далеко (если игрок быстро движется)
            if (distanceToPlayerFromSpawnPoint > 3f && distanceToPlayerFromSpawnPoint < spawnActivationRadius + 10f) // 3f - мин. дистанция, spawnActivationRadius + 10f - макс.
            {
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    // Передаем только playerMovementScript и ссылку на игрока
                    enemyAI.Initialize(playerTransform, playerMovementScript);
                    spawnedCount++;
                    enemiesSpawnedThisAttempt++;
                    Debug.Log("Заспавнен враг '" + newEnemy.name + "' на платформе '" + gameObject.name + "' в точке '" + spawnPoint.name + "'");
                }
                else
                {
                    Debug.LogError("На префабе врага '" + enemyPrefab.name + "' отсутствует скрипт EnemyAI!", newEnemy);
                    Destroy(newEnemy); // Уничтожаем некорректного врага
                }
            }

            // Ограничение на количество врагов за одну попытку спавна (если нужно спавнить по одному за раз)
            // if (enemiesSpawnedThisAttempt >= 1) break; 
        }

        if (enemiesSpawnedThisAttempt == 0 && spawnedCount < maxEnemiesToSpawn) // Если не удалось заспавнить, но лимит еще не достигнут
        {
            // Можно сбросить флаг, чтобы попытаться снова при следующем Update, когда игрок в радиусе.
            // Это полезно, если игрок маневрирует и точки спавна временно становятся недоступными.
            initialSpawnAttempted = false;
            Debug.Log("На платформе " + gameObject.name + " не найдено подходящих точек для спавна в этот раз. Попытка будет повторена.");
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