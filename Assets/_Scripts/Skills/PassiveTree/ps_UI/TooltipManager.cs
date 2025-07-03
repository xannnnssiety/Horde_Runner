using UnityEngine;
using UnityEngine.UI;


public class TooltipManager : MonoBehaviour
{
    // --- Синглтон ---
    public static TooltipManager Instance;

    [Header("Ссылки")]
    [Tooltip("Префаб окна подсказки")]
    [SerializeField] private GameObject tooltipPrefab;

    private Text _titleText;
    private Text _descriptionText;
    private RectTransform _tooltipRect;

    private void Awake()
    {
        // Настройка синглтона
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Создаем экземпляр тултипа и прячем его
        GameObject tooltipInstance = Instantiate(tooltipPrefab, transform.parent); // Создаем на том же уровне, что и менеджер (на Canvas)
        _titleText = tooltipInstance.transform.Find("Title_Text").GetComponent<Text>();
        _descriptionText = tooltipInstance.transform.Find("Description_Text").GetComponent<Text>();
        _tooltipRect = tooltipInstance.GetComponent<RectTransform>();

        tooltipInstance.SetActive(false);
    }

    void Update()
    {
        // Если тултип активен, он следует за курсором
        if (_tooltipRect.gameObject.activeSelf)
        {
            _tooltipRect.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// Показывает подсказку с заданным текстом.
    /// </summary>
    public void ShowTooltip(string title, string description)
    {
        _titleText.text = title;
        _descriptionText.text = description;
        _tooltipRect.gameObject.SetActive(true);
    }

    /// <summary>
    /// Прячет подсказку.
    /// </summary>
    public void HideTooltip()
    {
        _tooltipRect.gameObject.SetActive(false);
    }
}