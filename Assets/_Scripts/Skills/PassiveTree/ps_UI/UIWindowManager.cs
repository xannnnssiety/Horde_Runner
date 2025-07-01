using UnityEngine;

// Этот скрипт должен висеть на всегда активном объекте, например, на том же, где и GameManager.
public class UIWindowManager : MonoBehaviour
{
    [Header("Окно дерева пассивок")]
    [Tooltip("Клавиша для вызова окна")]
    [SerializeField] private KeyCode passiveTreeToggleKey = KeyCode.Tab;
    [Tooltip("Перетащите сюда родительский объект окна с деревом пассивок")]
    [SerializeField] private GameObject passiveTreeWindow;

    // Сюда в будущем можно добавлять другие окна
    // [Header("Окно инвентаря")]
    // [SerializeField] private KeyCode inventoryToggleKey = KeyCode.I;
    // [SerializeField] private GameObject inventoryWindow;

    void Start()
    {
        // Убедимся, что все управляемые окна выключены при старте
        if (passiveTreeWindow != null)
        {
            passiveTreeWindow.SetActive(false);
        }
        // if (inventoryWindow != null) inventoryWindow.SetActive(false);
    }

    void Update()
    {
        // Проверяем нажатие клавиши для каждого окна
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
    /// Универсальный метод для открытия/закрытия любого окна.
    /// </summary>
    /// <param name="windowObject">Окно, которое нужно переключить.</param>
    private void ToggleWindow(GameObject windowObject)
    {
        if (windowObject == null) return;

        // Инвертируем состояние окна
        bool isNowActive = !windowObject.activeSelf;
        windowObject.SetActive(isNowActive);

        // Управляем курсором.
        // Это простая логика. Если у вас будет много окон, возможно,
        // понадобится более сложная система, которая проверяет, открыто ли ХОТЯ БЫ ОДНО окно.
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