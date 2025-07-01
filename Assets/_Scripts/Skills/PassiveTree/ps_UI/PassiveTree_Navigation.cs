using UnityEngine;
using UnityEngine.EventSystems;

// Мы реализуем IScrollHandler, чтобы автоматически ловить события колесика мыши
public class PassiveTree_Navigation : MonoBehaviour, IScrollHandler
{
    [Header("Ссылки")]
    [Tooltip("Объект, который мы будем двигать и масштабировать (Content)")]
    [SerializeField] private RectTransform contentRect;
    [Tooltip("Объект, который является окном просмотра (сам PassiveTree)")]
    [SerializeField] private RectTransform viewportRect;

    [Header("Зум")]
    [Tooltip("Начальный масштаб дерева при открытии. 1 = 100%")]
    [SerializeField] private float initialScale = 0.8f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [Tooltip("Минимальный масштаб (например, 0.3 для отдаления)")]
    [SerializeField] private float minScale = 0.3f;
    [Tooltip("Максимальный масштаб (например, 2 для приближения)")]
    [SerializeField] private float maxScale = 2f;

    [Header("Панорамирование и Границы")]
    [Tooltip("Насколько сильно панель сопротивляется выходу за границы")]
    [SerializeField] private float panResistance = 3f;
    [Tooltip("Скорость плавного возврата панели в границы")]
    [SerializeField] private float returnSmoothTime = 0.15f;
    [Tooltip("Отступ от краев окна просмотра (в пикселях)")]
    [SerializeField] private float boundaryPadding = 50f;

    // --- Внутренние переменные ---
    private bool _isInitialized = false; // Блокирует работу до вызова EnableNavigation
    private bool isDragging = false;
    private Vector2 dragStartPosition;
    private Vector2 contentStartPosition;
    private bool isReturning = false;
    private Vector2 returnVelocity;

    /// <summary>
    /// Главный метод жизненного цикла, вызываемый каждый кадр.
    /// </summary>
    void Update()
    {
        // Не выполняем никакой логики, пока дерево не будет полностью сгенерировано
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
    /// Этот публичный метод вызывается из UI Менеджера, когда дерево построено.
    /// Он "включает" всю навигацию.
    /// </summary>
    public void EnableNavigation()
    {
        _isInitialized = true;

        // Устанавливаем начальный масштаб
        contentRect.localScale = new Vector3(initialScale, initialScale, 1f);

        // Запускаем механизм возврата, чтобы отцентрировать дерево с новым масштабом
        isReturning = true;
        Debug.Log("Навигация по дереву включена с начальным масштабом.");
    }

    /// <summary>
    /// Этот метод является частью интерфейса IScrollHandler и вызывается автоматически, 
    /// когда происходит скролл колесиком мыши над этим UI элементом.
    /// </summary>
    public void OnScroll(PointerEventData eventData)
    {
        if (!_isInitialized) return;
        HandleZoom(eventData.scrollDelta.y);
    }

    /// <summary>
    /// Обрабатывает логику зума.
    /// </summary>
    private void HandleZoom(float scrollDelta)
    {
        if (scrollDelta == 0) return;

        float oldZoom = contentRect.localScale.x;
        float newZoom = Mathf.Clamp(oldZoom + scrollDelta * zoomSpeed, minScale, maxScale);

        if (Mathf.Approximately(oldZoom, newZoom)) return;

        // Просто применяем новый масштаб. Зум будет к центру экрана.
        contentRect.localScale = new Vector3(newZoom, newZoom, 1f);

        // После каждого зума нужно проверить, не вышли ли мы за границы, и запустить возврат.
        isReturning = true;
    }

    /// <summary>
    /// Проверяет нажатие и отпускание левой кнопки мыши для начала/окончания перетаскивания.
    /// </summary>
    private void HandlePanningInput()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
        {
            isDragging = true;
            isReturning = false; // Прерываем возврат, если он был
            dragStartPosition = (Vector2)Input.mousePosition;
            contentStartPosition = contentRect.anchoredPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                isReturning = true; // Запускаем проверку на возврат
            }
        }
    }

    /// <summary>
    /// Применяет смещение к панели Content во время перетаскивания.
    /// </summary>
    private void ApplyPanning()
    {
        Vector2 mouseDelta = (Vector2)Input.mousePosition - dragStartPosition;
        Vector2 targetPos = contentStartPosition + mouseDelta;
        Rect bounds = CalculateBounds();

        // Применяем "резиновое" сопротивление при выходе за границы
        if (targetPos.x > bounds.xMax) targetPos.x = bounds.xMax + (targetPos.x - bounds.xMax) / panResistance;
        else if (targetPos.x < bounds.xMin) targetPos.x = bounds.xMin + (targetPos.x - bounds.xMin) / panResistance;

        if (targetPos.y > bounds.yMax) targetPos.y = bounds.yMax + (targetPos.y - bounds.yMax) / panResistance;
        else if (targetPos.y < bounds.yMin) targetPos.y = bounds.yMin + (targetPos.y - bounds.yMin) / panResistance;

        contentRect.anchoredPosition = targetPos;
    }

    /// <summary>
    /// Плавно возвращает панель Content в разрешенные границы.
    /// </summary>
    private void HandleReturnToBounds()
    {
        Rect bounds = CalculateBounds();
        Vector2 currentPos = contentRect.anchoredPosition;
        Vector2 clampedPos = new Vector2(
            Mathf.Clamp(currentPos.x, bounds.xMin, bounds.xMax),
            Mathf.Clamp(currentPos.y, bounds.yMin, bounds.yMax)
        );

        // Если мы уже достаточно близко, останавливаем движение
        if (Vector2.Distance(currentPos, clampedPos) < 0.1f)
        {
            isReturning = false;
            contentRect.anchoredPosition = clampedPos; // Жестко ставим в конечную точку
        }
        else
        {
            // Иначе, плавно движемся к цели
            contentRect.anchoredPosition = Vector2.SmoothDamp(currentPos, clampedPos, ref returnVelocity, returnSmoothTime);
        }
    }

    /// <summary>
    /// Вычисляет разрешенные границы для перемещения центра панели Content.
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