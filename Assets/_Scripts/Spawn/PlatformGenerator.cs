using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("Platform Settings")]
    public GameObject platformPrefab;         // Префаб платформы для генерации
    public Transform initialPlatform;         // Начальная платформа, уже размещенная на сцене
    public string spawnAnchorName = "SpawnAnchor"; // Имя дочернего объекта-якоря на платформе

    [Header("Player Settings")]
    public Transform playerTransform;         // Transform игрока
    public float spawnTriggerDistance = 15f;  // Расстояние до конца текущей платформы для генерации новой

    [Header("Platform Management")]
    public int maxPlatforms = 10;             // Максимальное количество активных платформ (для оптимизации)
    private List<GameObject> activePlatforms = new List<GameObject>();

    private Transform currentPlatformEndAnchor; // Точка спавна на текущей платформе
    private bool canSpawn = true; // Флаг, чтобы не спавнить много платформ за один кадр

    void Start()
    {
        if (platformPrefab == null)
        {
            Debug.LogError("Platform Prefab не назначен в PlatformGenerator!");
            enabled = false;
            return;
        }
        if (initialPlatform == null)
        {
            Debug.LogError("Initial Platform не назначена в PlatformGenerator!");
            enabled = false;
            return;
        }
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform не назначен в PlatformGenerator!");
            enabled = false;
            return;
        }

        // Начинаем с начальной платформы
        activePlatforms.Add(initialPlatform.gameObject);
        currentPlatformEndAnchor = FindSpawnAnchor(initialPlatform);

        if (currentPlatformEndAnchor == null)
        {
            Debug.LogError($"Не удалось найти SpawnAnchor с именем '{spawnAnchorName}' на начальной платформе '{initialPlatform.name}'!", initialPlatform.gameObject);
            enabled = false;
            return;
        }

        // Опционально: сразу сгенерировать несколько платформ вперед, чтобы игрок не видел пустоту
        for (int i = 0; i < 3; i++) // Например, 3 начальные платформы
        {
            if (currentPlatformEndAnchor != null) // Проверка, что якорь существует
            {
                SpawnNextPlatform();
            }
            else break; // Если якорь не найден на предыдущей, прерываем
        }
    }

    void Update()
    {
        if (currentPlatformEndAnchor == null || playerTransform == null) return;

        float distanceToAnchor = Vector3.Distance(playerTransform.position, currentPlatformEndAnchor.position);

        if (distanceToAnchor < spawnTriggerDistance && canSpawn)
        {
            SpawnNextPlatform();
        }
    }

    void SpawnNextPlatform()
    {
        if (platformPrefab == null || currentPlatformEndAnchor == null) return;

        canSpawn = false; // Предотвращаем многократный спавн

        // Создаем новую платформу в позиции и с ротацией якоря предыдущей
        GameObject newPlatformObj = Instantiate(platformPrefab, currentPlatformEndAnchor.position, currentPlatformEndAnchor.rotation);
        activePlatforms.Add(newPlatformObj);

        // Находим якорь на новой платформе и делаем его текущим
        currentPlatformEndAnchor = FindSpawnAnchor(newPlatformObj.transform);

        if (currentPlatformEndAnchor == null)
        {
            Debug.LogError($"Не удалось найти SpawnAnchor с именем '{spawnAnchorName}' на созданном префабе '{newPlatformObj.name}'!", newPlatformObj);
            // Можно остановить генерацию или предпринять другие действия
            enabled = false; // Например, остановить скрипт
            return;
        }


        // Управление количеством платформ (удаление старых)
        if (activePlatforms.Count > maxPlatforms)
        {
            GameObject platformToDestroy = activePlatforms[0];
            activePlatforms.RemoveAt(0);
            Destroy(platformToDestroy);
        }

        // Разрешаем спавн следующей платформы после небольшой задержки или другого условия
        // В данном простом случае можно разрешить сразу, если триггер будет срабатывать корректно
        // Если нет, можно использовать корутину для задержки canSpawn = true;
        canSpawn = true; // В данном случае, разрешаем сразу, т.к. currentPlatformEndAnchor обновился
    }

    Transform FindSpawnAnchor(Transform platform)
    {
        // Ищем дочерний объект по имени. Можно сделать более сложный поиск, если нужно.
        Transform anchor = platform.Find(spawnAnchorName);
        if (anchor == null)
        {
            Debug.LogWarning($"На платформе '{platform.name}' не найден SpawnAnchor с именем '{spawnAnchorName}'. Попытка найти среди всех дочерних объектов.");
            // Более глубокий поиск, если якорь не прямой потомок
            foreach (Transform child in platform.GetComponentsInChildren<Transform>())
            {
                if (child.name == spawnAnchorName)
                {
                    return child;
                }
            }
        }
        return anchor;
    }
}