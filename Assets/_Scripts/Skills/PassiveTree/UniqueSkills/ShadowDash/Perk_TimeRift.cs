using UnityEngine;

public class Perk_TimeRift : MonoBehaviour
{
    [Header("��������� ������� �������")]
    public GameObject timeShadowPrefab; // ������ ����� ����

    private void OnEnable()
    {
        GameEvents.OnDashStarted += HandleDashStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnDashStarted -= HandleDashStarted;
    }

    private void HandleDashStarted(Vector3 position, Quaternion rotation)
    {
        if (timeShadowPrefab != null)
        {
            Instantiate(timeShadowPrefab, position, rotation);
        }
    }
}