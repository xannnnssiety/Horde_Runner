using UnityEngine;

// ���� ������ ������ ������ �� ������ �������� �������, ��������, �� ��� ��, ��� � GameManager.
public class UIWindowManager : MonoBehaviour
{
    [Header("���� ������ ��������")]
    [Tooltip("������� ��� ������ ����")]
    [SerializeField] private KeyCode passiveTreeToggleKey = KeyCode.Tab;
    [Tooltip("���������� ���� ������������ ������ ���� � ������� ��������")]
    [SerializeField] private GameObject passiveTreeWindow;

    // ���� � ������� ����� ��������� ������ ����
    // [Header("���� ���������")]
    // [SerializeField] private KeyCode inventoryToggleKey = KeyCode.I;
    // [SerializeField] private GameObject inventoryWindow;

    void Start()
    {
        // ��������, ��� ��� ����������� ���� ��������� ��� ������
        if (passiveTreeWindow != null)
        {
            passiveTreeWindow.SetActive(false);
        }
        // if (inventoryWindow != null) inventoryWindow.SetActive(false);
    }

    void Update()
    {
        // ��������� ������� ������� ��� ������� ����
        if (Input.GetKeyDown(passiveTreeToggleKey))
        {
            ToggleWindow(passiveTreeWindow);
        }

        // if (Input.GetKeyDown(inventoryToggleKey))
        // {
        //     ToggleWindow(inventoryWindow);
        // }
    }

    /// <summary>
    /// ������������� ����� ��� ��������/�������� ������ ����.
    /// </summary>
    /// <param name="windowObject">����, ������� ����� �����������.</param>
    private void ToggleWindow(GameObject windowObject)
    {
        if (windowObject == null) return;

        // ����������� ��������� ����
        bool isNowActive = !windowObject.activeSelf;
        windowObject.SetActive(isNowActive);

        // ��������� ��������.
        // ��� ������� ������. ���� � ��� ����� ����� ����, ��������,
        // ����������� ����� ������� �������, ������� ���������, ������� �� ���� �� ���� ����.
        if (isNowActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}