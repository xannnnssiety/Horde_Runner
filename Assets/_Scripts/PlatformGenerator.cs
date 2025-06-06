using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("Platform Settings")]
    public GameObject platformPrefab;         // ������ ��������� ��� ���������
    public Transform initialPlatform;         // ��������� ���������, ��� ����������� �� �����
    public string spawnAnchorName = "SpawnAnchor"; // ��� ��������� �������-����� �� ���������

    [Header("Player Settings")]
    public Transform playerTransform;         // Transform ������
    public float spawnTriggerDistance = 15f;  // ���������� �� ����� ������� ��������� ��� ��������� �����

    [Header("Platform Management")]
    public int maxPlatforms = 10;             // ������������ ���������� �������� �������� (��� �����������)
    private List<GameObject> activePlatforms = new List<GameObject>();

    private Transform currentPlatformEndAnchor; // ����� ������ �� ������� ���������
    private bool canSpawn = true; // ����, ����� �� �������� ����� �������� �� ���� ����

    void Start()
    {
        if (platformPrefab == null)
        {
            Debug.LogError("Platform Prefab �� �������� � PlatformGenerator!");
            enabled = false;
            return;
        }
        if (initialPlatform == null)
        {
            Debug.LogError("Initial Platform �� ��������� � PlatformGenerator!");
            enabled = false;
            return;
        }
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform �� �������� � PlatformGenerator!");
            enabled = false;
            return;
        }

        // �������� � ��������� ���������
        activePlatforms.Add(initialPlatform.gameObject);
        currentPlatformEndAnchor = FindSpawnAnchor(initialPlatform);

        if (currentPlatformEndAnchor == null)
        {
            Debug.LogError($"�� ������� ����� SpawnAnchor � ������ '{spawnAnchorName}' �� ��������� ��������� '{initialPlatform.name}'!", initialPlatform.gameObject);
            enabled = false;
            return;
        }

        // �����������: ����� ������������� ��������� �������� ������, ����� ����� �� ����� �������
        for (int i = 0; i < 3; i++) // ��������, 3 ��������� ���������
        {
            if (currentPlatformEndAnchor != null) // ��������, ��� ����� ����������
            {
                SpawnNextPlatform();
            }
            else break; // ���� ����� �� ������ �� ����������, ���������
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

        canSpawn = false; // ������������� ������������ �����

        // ������� ����� ��������� � ������� � � �������� ����� ����������
        GameObject newPlatformObj = Instantiate(platformPrefab, currentPlatformEndAnchor.position, currentPlatformEndAnchor.rotation);
        activePlatforms.Add(newPlatformObj);

        // ������� ����� �� ����� ��������� � ������ ��� �������
        currentPlatformEndAnchor = FindSpawnAnchor(newPlatformObj.transform);

        if (currentPlatformEndAnchor == null)
        {
            Debug.LogError($"�� ������� ����� SpawnAnchor � ������ '{spawnAnchorName}' �� ��������� ������� '{newPlatformObj.name}'!", newPlatformObj);
            // ����� ���������� ��������� ��� ����������� ������ ��������
            enabled = false; // ��������, ���������� ������
            return;
        }


        // ���������� ����������� �������� (�������� ������)
        if (activePlatforms.Count > maxPlatforms)
        {
            GameObject platformToDestroy = activePlatforms[0];
            activePlatforms.RemoveAt(0);
            Destroy(platformToDestroy);
        }

        // ��������� ����� ��������� ��������� ����� ��������� �������� ��� ������� �������
        // � ������ ������� ������ ����� ��������� �����, ���� ������� ����� ����������� ���������
        // ���� ���, ����� ������������ �������� ��� �������� canSpawn = true;
        canSpawn = true; // � ������ ������, ��������� �����, �.�. currentPlatformEndAnchor ���������
    }

    Transform FindSpawnAnchor(Transform platform)
    {
        // ���� �������� ������ �� �����. ����� ������� ����� ������� �����, ���� �����.
        Transform anchor = platform.Find(spawnAnchorName);
        if (anchor == null)
        {
            Debug.LogWarning($"�� ��������� '{platform.name}' �� ������ SpawnAnchor � ������ '{spawnAnchorName}'. ������� ����� ����� ���� �������� ��������.");
            // ����� �������� �����, ���� ����� �� ������ �������
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