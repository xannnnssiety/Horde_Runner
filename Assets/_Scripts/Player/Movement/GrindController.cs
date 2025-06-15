using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Final : MonoBehaviour
{
    // --- ПУБЛИЧНЫЕ НАСТРОЙКИ (видны в инспекторе) ---

    [Header("Ссылки")]
    public Camera mainCamera;

    [Header("Движение")]
    public float moveSpeed = 15f;
    public float turnSmoothTime = 0.1f;

    [Header("Прыжок и Гравитация")]
    public float jumpHeight = 3f;
    public float gravity = -20f;
    public float coyoteTime = 0.15f;

    [Header("Грайнд")]
    public float grindSpeed = 25f;
    public LayerMask grindableLayer;
    [Tooltip("Радиус, в котором персонаж ищет рельсы вокруг себя")]
    public float grindSearchRadius = 3f;


    // --- ПРИВАТНЫЕ ПЕРЕМЕННЫЕ (для работы скрипта) ---

    // Состояние
    private CharacterController controller;
    private Animator animator;
    private bool isGrinding = false;

    // Физика
    private Vector3 playerVelocity;
    private float coyoteTimeCounter;
    private float turnSmoothVelocity;

    // Грайнд
    private Transform currentRail;
    private Vector3 grindDirection;


    // --- ОСНОВНЫЕ МЕТОДЫ UNITY ---

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (mainCamera == null) Debug.LogError("Камера не назначена! Перетащите вашу Main Camera в слот.");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // В зависимости от состояния (скользим мы или нет), выполняем разную логику
        if (isGrinding)
        {
            HandleGrinding();
        }
        else
        {
            HandleGroundedOrAirborne();
        }
    }


    // --- ЛОГИКА ОБЫЧНОГО ДВИЖЕНИЯ (ЗЕМЛЯ И ВОЗДУХ) ---

    private void HandleGroundedOrAirborne()
    {
        // --- Физика: Гравитация и "Время Койота" ---
        if (controller.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            // "Прижимаем" к земле, чтобы не было подпрыгиваний на склонах
            if (playerVelocity.y < 0) playerVelocity.y = -2f;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Применяем гравитацию постоянно
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);


        // --- Управление: Движение и Поворот ---
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            controller.Move(transform.forward * moveSpeed * Time.deltaTime);
        }

        // Обновляем аниматор
        animator.SetFloat("Speed", direction.magnitude);


        // --- Действия: Прыжок ---
        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
            coyoteTimeCounter = 0f; // Чтобы нельзя было прыгнуть дважды
        }

        // --- Переход на грайнд: АВТОМАТИЧЕСКИ ---
        // Если мы не на земле, но под нами есть рельса, начинаем скользить
        if (!controller.isGrounded)
        {
            TryToStartGrind();
        }
    }


    // --- ЛОГИКА ГРАЙНДА ---

    private void TryToStartGrind()
    {
        // Пускаем луч вниз, чтобы найти рельсу
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, 3f, grindableLayer))
        {
            // Нашли! Переключаем состояние
            isGrinding = true;
            currentRail = hit.transform; // Запоминаем первую рельсу
            playerVelocity = Vector3.zero; // Выключаем гравитацию

            // Определяем начальное направление движения по рельсе
            float dot = Vector3.Dot(transform.forward, currentRail.forward);
            grindDirection = (dot >= 0) ? currentRail.forward : -currentRail.forward;
        }
    }

    private void HandleGrinding()
    {
        // 1. ИЩЕМ БЛИЖАЙШУЮ РЕЛЬСУ ВОКРУГ
        Transform bestRail = FindBestRail();

        if (bestRail == null)
        {
            // Если рельс рядом больше нет, заканчиваем грайнд и падаем
            EndGrind(false);
            return;
        }

        // Если лучшая рельса изменилась (мы на стыке), обновляем ее
        if (currentRail != bestRail)
        {
            currentRail = bestRail;
            // Определяем новое направление движения
            float dot = Vector3.Dot(grindDirection, currentRail.forward);
            grindDirection = (dot >= 0) ? currentRail.forward : -currentRail.forward;
        }

        // 2. ПРИЛИПАЕМ К РЕЛЬСЕ
        Vector3 closestPoint = currentRail.GetComponent<Collider>().ClosestPoint(transform.position);
        // Плавно, но быстро двигаемся к точке на рельсе. Без телепортов.
        controller.Move((closestPoint - transform.position));

        // 3. ДВИЖЕМСЯ ВПЕРЕД И ПОВОРАЧИВАЕМСЯ
        controller.Move(grindDirection * grindSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(grindDirection), turnSmoothTime * 15f);

        // Обновляем аниматор
        animator.SetFloat("Speed", 1f); // Во время грайнда всегда "бежим"

        // 4. ПРОВЕРКА НА СПРЫГИВАНИЕ
        if (Input.GetButtonDown("Jump"))
        {
            EndGrind(true); // Заканчиваем грайнд с прыжком
        }
    }

    private Transform FindBestRail()
    {
        // Ищем ВСЕ коллайдеры рельс в радиусе вокруг персонажа
        var nearbyRails = Physics.OverlapSphere(transform.position, grindSearchRadius, grindableLayer);

        if (nearbyRails.Length == 0) return null;

        // Находим самый близкий коллайдер из всех
        return nearbyRails.OrderBy(rail => Vector3.Distance(transform.position, rail.ClosestPoint(transform.position)))
                          .FirstOrDefault()? // Берем первый (самый близкий) или null, если список пуст
                          .transform;
    }

    private void EndGrind(bool didJump)
    {
        isGrinding = false;
        if (didJump)
        {
            // Если спрыгнули, даем импульс вверх
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }
    }
}