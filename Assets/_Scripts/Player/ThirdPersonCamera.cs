using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The target to follow
    public Vector3 offset = new Vector3(0f, 1.5f, 0f); // Offset from the target position

    [Header("Camera Control")]
    public float rotationSpeed = 5f; // Speed of camera rotation    
    public float minYAngle = -30f; // Minimum vertical angle    
    public float maxYAngle = 70f; // Maximum vertical angle

    [Header("Zoom Control")]
    public float initialDistance = 5f; // Initial distance from the target  
    public float minDistance = 1.5f; // Minimum distance from the target
    public float maxDistance = 10f; // Maximum distance from the target
    public float zoomSpeed = 5f; // Speed of zooming

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f; // Smoothing time for position
    public float rotationSmoothTime = 0.05f; // Smoothing time for rotation

    [Header("Collision Handling")]
    public LayerMask collisionLayers; // Layer mask for collision detection
    public float collisionOffset = 0.2f; // Offset to prevent camera from clipping through objects

    [Header("Wall Run Settings")]
    [Tooltip("Угол наклона камеры при беге по стене")]
    public float wallRunTiltAngle = 15f;
    [Tooltip("Скорость наклона камеры")]
    public float tiltSmoothTime = 0.2f;

    private float currentX = 0f; // Current horizontal angle
    private float currentY = 0f; // Current vertical angle
    private float currentDistance; // Current distance from the target
    private Vector3 currentPositionVelocity; // Velocity for position smoothing
    private Quaternion currentRotation; // Current rotation of the camera
    private Quaternion targetRotation; // Target rotation of the camera
    private float currentTilt;
    private float tiltVelocity;

    private PlayerWallRun playerWallRun;

    void Start()
    {
        if (!target)
        {
            Debug.LogError("Target not set for ThirdPersonCamera. Please assign a target in the inspector.");
            enabled = false;
            return;
        }

        if (target != null)
        {
            playerWallRun = target.GetComponent<PlayerWallRun>();
            if (playerWallRun == null)
            {
                Debug.LogWarning("Камера не нашла компонент PlayerWallRun на цели!", this);
            }
        }

        currentDistance = initialDistance;
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentX = angles.x;

        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }



    void LateUpdate()
    {
        if (!target) return;

        HandleInput();
        CalculateCameraTransform();

    }



    void HandleInput()
    {
        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

    }



    void CalculateCameraTransform()
    {
        // 1. Определяем точку, на которую смотрим
        Vector3 targetFocusPoint = target.position + offset;

        // 2. Рассчитываем желаемую позицию БЕЗ учета наклона (пока что)
        Quaternion desiredRotationWithoutTilt = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = targetFocusPoint - (desiredRotationWithoutTilt * Vector3.forward * currentDistance);

        // 3. Обработка столкновений (остается без изменений)
        RaycastHit hit;
        Vector3 directionToCamera = desiredPosition - targetFocusPoint;
        if (Physics.Raycast(targetFocusPoint, directionToCamera.normalized, out hit, directionToCamera.magnitude, collisionLayers))
        {
            desiredPosition = hit.point + hit.normal * collisionOffset;
        }

        // 4. Плавное перемещение позиции камеры (остается без изменений)
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentPositionVelocity, positionSmoothTime);

        // --- ОБНОВЛЕННАЯ ЛОГИКА ПОВОРОТА И НАКЛОНА ---

        // 5. Сначала вычисляем базовый поворот, заставляющий камеру смотреть на цель
        Quaternion lookAtTargetRotation = Quaternion.LookRotation(targetFocusPoint - transform.position);

        // 6. Вычисляем наш целевой наклон (как и раньше)
        HandleTilt();

        // 7. Создаем кватернион только для наклона
        Quaternion tiltRotation = Quaternion.Euler(0, 0, currentTilt);

        // 8. КОМБИНИРУЕМ! Мы берем поворот "взгляда на цель" и добавляем к нему наклон.
        // Это финальная, правильная ротация.
        Quaternion finalRotation = lookAtTargetRotation * tiltRotation;

        // 9. Применяем финальный поворот к камере. Убираем старую функцию SmoothQuaternion.
        // Quaternion.Slerp здесь подходит лучше для предсказуемого результата.
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime / Mathf.Max(0.001f, rotationSmoothTime));
    }


    void HandleTilt()
    {
        float targetTilt = 0f;

        // Проверяем, есть ли ссылка на модуль и активен ли бег
        if (playerWallRun != null && playerWallRun.IsWallRunning)
        {
            // Если мы зашли сюда, значит, камера ЗНАЕТ о беге по стене.
            // Вы увидите это сообщение в консоли Unity.
            Debug.Log("Wall Running Detected by Camera!");

            Vector3 wallNormal = playerWallRun.WallNormal;

            // Эта логика может быть ненадежной, если камера сильно вращается.
            // float dot = Vector3.Dot(wallNormal, transform.right); 
            // targetTilt = -dot * wallRunTiltAngle;

            // --- БОЛЕЕ НАДЕЖНЫЙ СПОСОБ ---
            // Узнаем, с какой стороны от игрока стена
            Vector3 playerRight = playerWallRun.transform.right;
            float dot = Vector3.Dot(wallNormal, playerRight);

            // Если dot < 0, стена слева (нормаль указывает вправо). Наклон положительный.
            // Если dot > 0, стена справа (нормаль указывает влево). Наклон отрицательный.
            if (dot < 0) // Стена слева
            {
                targetTilt = wallRunTiltAngle;
            }
            else // Стена справа
            {
                targetTilt = -wallRunTiltAngle;
            }
        }

        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltVelocity, tiltSmoothTime);
    }



    // Вспомогательная функция для плавного изменения Quaternion (альтернатива Slerp с временем)
    Quaternion SmoothQuaternion(Quaternion current, Quaternion target, float smoothTime)
    {
        return Quaternion.Slerp(current, target, 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.001f, smoothTime)));
    }

    // Для отладки можно нарисовать линии в редакторе
    void OnDrawGizmosSelected()
    {
        if (!target) return;

        Vector3 targetFocusPoint = target.position + offset;
        Quaternion desiredRotation = Quaternion.Euler(currentY, currentX, 0); // Используйте актуальные значения, если они есть
        Vector3 desiredPositionWithoutCollision = targetFocusPoint - (desiredRotation * Vector3.forward * currentDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(targetFocusPoint, desiredPositionWithoutCollision);
        Gizmos.DrawSphere(desiredPositionWithoutCollision, 0.1f);

        if (Application.isPlaying) // Отображать только во время игры, т.к. Raycast работает только тогда
        {
            RaycastHit hit;
            Vector3 directionToCamera = desiredPositionWithoutCollision - targetFocusPoint;
            if (Physics.Raycast(targetFocusPoint, directionToCamera.normalized, out hit, directionToCamera.magnitude, collisionLayers))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 0.15f);
                Gizmos.DrawLine(targetFocusPoint, hit.point);
            }
        }
    }


 
}
