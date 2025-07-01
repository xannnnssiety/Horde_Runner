using UnityEngine;
using UnityEngine.EventSystems;

// �� ��������� IScrollHandler, ����� ������������� ������ ������� �������� ����
public class PassiveTree_Navigation : MonoBehaviour, IScrollHandler
{
    [Header("������")]
    [Tooltip("������, ������� �� ����� ������� � �������������� (Content)")]
    [SerializeField] private RectTransform contentRect;
    [Tooltip("������, ������� �������� ����� ��������� (��� PassiveTree)")]
    [SerializeField] private RectTransform viewportRect;

    [Header("���")]
    [Tooltip("��������� ������� ������ ��� ��������. 1 = 100%")]
    [SerializeField] private float initialScale = 0.8f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [Tooltip("����������� ������� (��������, 0.3 ��� ���������)")]
    [SerializeField] private float minScale = 0.3f;
    [Tooltip("������������ ������� (��������, 2 ��� �����������)")]
    [SerializeField] private float maxScale = 2f;

    [Header("��������������� � �������")]
    [Tooltip("��������� ������ ������ �������������� ������ �� �������")]
    [SerializeField] private float panResistance = 3f;
    [Tooltip("�������� �������� �������� ������ � �������")]
    [SerializeField] private float returnSmoothTime = 0.15f;
    [Tooltip("������ �� ����� ���� ��������� (� ��������)")]
    [SerializeField] private float boundaryPadding = 50f;

    // --- ���������� ���������� ---
    private bool _isInitialized = false; // ��������� ������ �� ������ EnableNavigation
    private bool isDragging = false;
    private Vector2 dragStartPosition;
    private Vector2 contentStartPosition;
    private bool isReturning = false;
    private Vector2 returnVelocity;

    /// <summary>
    /// ������� ����� ���������� �����, ���������� ������ ����.
    /// </summary>
    void Update()
    {
        // �� ��������� ������� ������, ���� ������ �� ����� ��������� �������������
        if (!_isInitialized) return;

        HandlePanningInput();

        if (isDragging)
        {
            ApplyPanning();
        }
        else if (isReturning)
        {
            HandleReturnToBounds();
        }
    }

    /// <summary>
    /// ���� ��������� ����� ���������� �� UI ���������, ����� ������ ���������.
    /// �� "��������" ��� ���������.
    /// </summary>
    public void EnableNavigation()
    {
        _isInitialized = true;

        // ������������� ��������� �������
        contentRect.localScale = new Vector3(initialScale, initialScale, 1f);

        // ��������� �������� ��������, ����� �������������� ������ � ����� ���������
        isReturning = true;
        Debug.Log("��������� �� ������ �������� � ��������� ���������.");
    }

    /// <summary>
    /// ���� ����� �������� ������ ���������� IScrollHandler � ���������� �������������, 
    /// ����� ���������� ������ ��������� ���� ��� ���� UI ���������.
    /// </summary>
    public void OnScroll(PointerEventData eventData)
    {
        if (!_isInitialized) return;
        HandleZoom(eventData.scrollDelta.y);
    }

    /// <summary>
    /// ������������ ������ ����.
    /// </summary>
    private void HandleZoom(float scrollDelta)
    {
        if (scrollDelta == 0) return;

        float oldZoom = contentRect.localScale.x;
        float newZoom = Mathf.Clamp(oldZoom + scrollDelta * zoomSpeed, minScale, maxScale);

        if (Mathf.Approximately(oldZoom, newZoom)) return;

        // ������ ��������� ����� �������. ��� ����� � ������ ������.
        contentRect.localScale = new Vector3(newZoom, newZoom, 1f);

        // ����� ������� ���� ����� ���������, �� ����� �� �� �� �������, � ��������� �������.
        isReturning = true;
    }

    /// <summary>
    /// ��������� ������� � ���������� ����� ������ ���� ��� ������/��������� ��������������.
    /// </summary>
    private void HandlePanningInput()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
        {
            isDragging = true;
            isReturning = false; // ��������� �������, ���� �� ���
            dragStartPosition = (Vector2)Input.mousePosition;
            contentStartPosition = contentRect.anchoredPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                isReturning = true; // ��������� �������� �� �������
            }
        }
    }

    /// <summary>
    /// ��������� �������� � ������ Content �� ����� ��������������.
    /// </summary>
    private void ApplyPanning()
    {
        Vector2 mouseDelta = (Vector2)Input.mousePosition - dragStartPosition;
        Vector2 targetPos = contentStartPosition + mouseDelta;
        Rect bounds = CalculateBounds();

        // ��������� "���������" ������������� ��� ������ �� �������
        if (targetPos.x > bounds.xMax) targetPos.x = bounds.xMax + (targetPos.x - bounds.xMax) / panResistance;
        else if (targetPos.x < bounds.xMin) targetPos.x = bounds.xMin + (targetPos.x - bounds.xMin) / panResistance;

        if (targetPos.y > bounds.yMax) targetPos.y = bounds.yMax + (targetPos.y - bounds.yMax) / panResistance;
        else if (targetPos.y < bounds.yMin) targetPos.y = bounds.yMin + (targetPos.y - bounds.yMin) / panResistance;

        contentRect.anchoredPosition = targetPos;
    }

    /// <summary>
    /// ������ ���������� ������ Content � ����������� �������.
    /// </summary>
    private void HandleReturnToBounds()
    {
        Rect bounds = CalculateBounds();
        Vector2 currentPos = contentRect.anchoredPosition;
        Vector2 clampedPos = new Vector2(
            Mathf.Clamp(currentPos.x, bounds.xMin, bounds.xMax),
            Mathf.Clamp(currentPos.y, bounds.yMin, bounds.yMax)
        );

        // ���� �� ��� ���������� ������, ������������� ��������
        if (Vector2.Distance(currentPos, clampedPos) < 0.1f)
        {
            isReturning = false;
            contentRect.anchoredPosition = clampedPos; // ������ ������ � �������� �����
        }
        else
        {
            // �����, ������ �������� � ����
            contentRect.anchoredPosition = Vector2.SmoothDamp(currentPos, clampedPos, ref returnVelocity, returnSmoothTime);
        }
    }

    /// <summary>
    /// ��������� ����������� ������� ��� ����������� ������ ������ Content.
    /// </summary>
    private Rect CalculateBounds()
    {
        Vector2 viewportSize = viewportRect.rect.size;
        Vector2 contentSize = contentRect.rect.size * contentRect.localScale.x;

        float xMinBoundary = (contentSize.x > viewportSize.x) ? (contentSize.x - viewportSize.x) / 2f + boundaryPadding : 0;
        float yMinBoundary = (contentSize.y > viewportSize.y) ? (contentSize.y - viewportSize.y) / 2f + boundaryPadding : 0;

        return new Rect(-xMinBoundary, -yMinBoundary, xMinBoundary * 2, yMinBoundary * 2);
    }
}