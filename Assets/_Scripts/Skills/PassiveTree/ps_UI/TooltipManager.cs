using UnityEngine;
using UnityEngine.UI;


public class TooltipManager : MonoBehaviour
{
    // --- �������� ---
    public static TooltipManager Instance;

    [Header("������")]
    [Tooltip("������ ���� ���������")]
    [SerializeField] private GameObject tooltipPrefab;

    private Text _titleText;
    private Text _descriptionText;
    private RectTransform _tooltipRect;

    private void Awake()
    {
        // ��������� ���������
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // ������� ��������� ������� � ������ ���
        GameObject tooltipInstance = Instantiate(tooltipPrefab, transform.parent); // ������� �� ��� �� ������, ��� � �������� (�� Canvas)
        _titleText = tooltipInstance.transform.Find("Title_Text").GetComponent<Text>();
        _descriptionText = tooltipInstance.transform.Find("Description_Text").GetComponent<Text>();
        _tooltipRect = tooltipInstance.GetComponent<RectTransform>();

        tooltipInstance.SetActive(false);
    }

    void Update()
    {
        // ���� ������ �������, �� ������� �� ��������
        if (_tooltipRect.gameObject.activeSelf)
        {
            _tooltipRect.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// ���������� ��������� � �������� �������.
    /// </summary>
    public void ShowTooltip(string title, string description)
    {
        _titleText.text = title;
        _descriptionText.text = description;
        _tooltipRect.gameObject.SetActive(true);
    }

    /// <summary>
    /// ������ ���������.
    /// </summary>
    public void HideTooltip()
    {
        _tooltipRect.gameObject.SetActive(false);
    }
}