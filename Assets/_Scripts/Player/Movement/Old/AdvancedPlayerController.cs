using UnityEngine;


// Главный класс-контроллер, который управляет состояниями
[RequireComponent(typeof(CharacterController))]
public class AdvancedPlayerController : MonoBehaviour
{
    #region Debug
    [Header("Состояние (для отладки)")]
    [SerializeField] private string _currentStateName; // <<< НОВОЕ
    #endregion

    #region Public Variables & Inspector Settings
    [Header("Dependencies")]
    [Tooltip("Ссылка на главную камеру для расчета направления движения.")]
    public Transform cameraTransform;


    [Header("Movement Settings")]
    [Tooltip("Скорость бега по земле.")]
    public float moveSpeed = 8f;
    [Tooltip("Сила прыжка.")]
    public float jumpForce = 10f;
    [Tooltip("Сила гравитации. Применяется вручную.")]
    public float gravity = -20f;
    [Tooltip("Плавность поворота персонажа.")]
    public float turnSmoothTime = 0.1f;

    [Tooltip("Ссылка на компонент Animator персонажа.")]
    public Animator animator;

    [Header("Animation Settings")] 
    [Tooltip("Плавность смены значения 'Speed' в аниматоре.")] 
    public float animationDampTime = 0.1f; 

    [Header("Air Control")]
    [Tooltip("Насколько сильно игрок может влиять на движение в воздухе.")]
    public float airControlStrength = 4f;

    [Header("Grinding Settings")]
    [Tooltip("Скорость передвижения по рельсам.")]
    public float grindSpeed = 15f;
    [Tooltip("Тег для объектов, по которым можно грайндить.")]
    public string grindableTag = "Grindable";

    [Header("Wall-Running Settings")]
    [Tooltip("Скорость бега по стенам.")]
    public float wallRunSpeed = 12f;
    [Tooltip("Максимальная продолжительность бега по стене (в секундах).")]
    public float wallRunDuration = 2f;
    [Tooltip("Сила прыжка от стены (вверх и в сторону).")]
    public float wallJumpForce = 12f;
    [Tooltip("Тег для объектов, по которым можно бегать.")]
    public string wallRunnableTag = "WallRunnable";

    #endregion


    #region State Machine
    // --- Конечный автомат (State Machine) ---
    private PlayerBaseState _currentState;



    // Создаем экземпляры всех возможных состояний
    public GroundedState GroundedState { get; private set; }
    public InAirState InAirState { get; private set; }
    public GrindingState GrindingState { get; private set; }
    public WallRunningState WallRunningState { get; private set; }

    // Метод для смены состояний
    public void SwitchState(PlayerBaseState newState)
    {
        _currentState?.ExitState(); // Вызываем выход из текущего состояния
        _currentState = newState;
        _currentStateName = newState.GetType().Name;
        _currentState.EnterState(); // Входим в новое состояние
    }
    #endregion

    #region Animation Hashes 
    public readonly int SpeedHash = Animator.StringToHash("Speed");
    public readonly int JumpHash = Animator.StringToHash("Jump");
    public readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    public readonly int IsGrindingHash = Animator.StringToHash("IsGrinding");
    public readonly int IsWallRunningHash = Animator.StringToHash("IsWallRunning");
    #endregion


    #region Public Properties & Components
    // --- Компоненты и публичные свойства, доступные для всех состояний ---
    public CharacterController Controller { get; private set; }
    public Vector3 Velocity { get; set; } // Ключевая переменная для сохранения инерции (momentum)
    public float TurnSmoothVelocity; // Используется для SmoothDampAngle
    public Vector2 InputVector { get; private set; }
    public Transform LastGrindableRail { get; set; } // Сохраняем последнюю рельсу
    public Vector3 WallNormal { get; set; } // Нормаль стены для бега и отталкивания
    #endregion


    #region Unity Lifecycle
    private void Awake()
    {

        if (animator == null) 
        {
            animator = GetComponentInChildren<Animator>();
        }

        Controller = GetComponent<CharacterController>();

        // Инициализация состояний
        GroundedState = new GroundedState(this);
        InAirState = new InAirState(this);
        GrindingState = new GrindingState(this);
        WallRunningState = new WallRunningState(this);
    }

    private void Start()
    {
        // Начальное состояние - на земле
        SwitchState(GroundedState);
    }

    private void Update()
    {
        // Считываем инпут каждый кадр
        InputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Делегируем всю логику текущему состоянию
        _currentState.UpdateState();
        UpdateAnimator();

    }

    private void UpdateAnimator()
    {
        // Вычисляем горизонтальную скорость для параметра Speed
        Vector3 horizontalVelocity = new Vector3(Controller.velocity.x, 0, Controller.velocity.z);
        float speed = horizontalVelocity.magnitude;

        // Используем SmoothDamp для плавного изменения значения в аниматоре
        animator.SetFloat(SpeedHash, speed, animationDampTime, Time.deltaTime);
    }

    // Этот метод вызывается, когда CharacterController сталкивается с другим коллайдером
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Передаем информацию о столкновении в текущее состояние для обработки
        _currentState.HandleCollision(hit);
    }
    #endregion
}

#region State Machine Abstract Base

// Абстрактный базовый класс для всех состояний
public abstract class PlayerBaseState
{
    protected readonly AdvancedPlayerController Ctx; // Контекст (ссылка на главный контроллер)

    protected PlayerBaseState(AdvancedPlayerController context)
    {
        Ctx = context;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public virtual void HandleCollision(ControllerColliderHit hit) { } // Виртуальный метод, т.к. не всем состояниям нужна обработка столкновений
}

#endregion


#region Concrete States

// --- Состояние: На земле (Grounded) ---
public class GroundedState : PlayerBaseState
{
    public GroundedState(AdvancedPlayerController context) : base(context) { }

    public override void EnterState()
    {
        // При приземлении гасим вертикальную скорость, чтобы не "подпрыгивать"
        var velocity = Ctx.Velocity;
        velocity.y = -2f; // Небольшая отрицательная скорость для стабильного isGrounded
        Ctx.Velocity = velocity;
        Ctx.animator.SetBool(Ctx.IsGroundedHash, true);
    }

    public override void UpdateState()
    {
        // 1. Проверка на переход в другие состояния
        // Прыжок
        if (Input.GetButtonDown("Jump"))
        {
            var velocity = Ctx.Velocity;
            velocity.y = Ctx.jumpForce;
            Ctx.Velocity = velocity;
            Ctx.animator.SetTrigger(Ctx.JumpHash);
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // Упал с уступа
        if (!Ctx.Controller.isGrounded)
        {
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // 2. Логика движения
        Vector2 input = Ctx.InputVector;
        if (input.sqrMagnitude > 0.01f) // Если есть ввод
        {
            // Расчет направления движения относительно камеры
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + Ctx.cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Поворот персонажа
            float angle = Mathf.SmoothDampAngle(Ctx.transform.eulerAngles.y, targetAngle, ref Ctx.TurnSmoothVelocity, Ctx.turnSmoothTime);
            Ctx.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Применение скорости
            Vector3 horizontalVelocity = moveDir.normalized * Ctx.moveSpeed;
            Ctx.Velocity = new Vector3(horizontalVelocity.x, Ctx.Velocity.y, horizontalVelocity.z);
        }
        else // Если ввода нет, останавливаемся
        {
            Ctx.Velocity = new Vector3(0, Ctx.Velocity.y, 0);
        }

        // Применение движения
        Ctx.Controller.Move(Ctx.Velocity * Time.deltaTime);
    }

    public override void ExitState() 
    {
        Ctx.animator.SetBool(Ctx.IsGroundedHash, false);
    }

}


// --- Состояние: В воздухе (InAir) ---
public class InAirState : PlayerBaseState
{
    public InAirState(AdvancedPlayerController context) : base(context) { }

    public override void EnterState() { }

    public override void UpdateState()
    {
        // 1. Проверка на приземление
        if (Ctx.Controller.isGrounded && Ctx.Velocity.y < 0)
        {
            Ctx.SwitchState(Ctx.GroundedState);
            return;
        }

        // 2. Логика движения в воздухе
        // Применение гравитации
        var velocity = Ctx.Velocity;
        velocity.y += Ctx.gravity * Time.deltaTime;

        // Контроль в воздухе (Air Control)
        Vector2 input = Ctx.InputVector;
        if (input.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + Ctx.cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Плавно добавляем контроль к текущей инерции, а не заменяем ее
            velocity += moveDir.normalized * Ctx.airControlStrength * Time.deltaTime;
        }

        Ctx.Velocity = velocity;

        // Применение движения
        Ctx.Controller.Move(Ctx.Velocity * Time.deltaTime);
    }

    public override void HandleCollision(ControllerColliderHit hit)
    {
        // Проверка на столкновение с рельсой
        if (hit.gameObject.CompareTag(Ctx.grindableTag))
        {
            Ctx.LastGrindableRail = hit.transform;
            Ctx.SwitchState(Ctx.GrindingState);
        }
        // Проверка на столкновение со стеной
        else if (hit.gameObject.CompareTag(Ctx.wallRunnableTag))
        {
            // Убедимся, что мы ударяемся о стену, а не о пол или потолок
            // Нормаль - это вектор, перпендикулярный поверхности. Для стен он будет горизонтальным.
            if (Mathf.Abs(hit.normal.y) < 0.1f)
            {
                Ctx.WallNormal = hit.normal;
                Ctx.SwitchState(Ctx.WallRunningState);
            }
        }
    }

    public override void ExitState() { }
}


// --- Состояние: Грайнд (Grinding) ---
// --- Состояние: Грайнд (Grinding) ---
// --- Состояние: Грайнд (Grinding) ---
// --- Состояние: Грайнд (Grinding) ---
public class GrindingState : PlayerBaseState
{
    public GrindingState(AdvancedPlayerController context) : base(context) { }

    private Vector3 _grindDirection; // Направление движения вдоль рельсы
    private Vector3 _railAxis;       // Ось, вдоль которой вытянута рельса

    public override void EnterState()
    {
        Ctx.animator.SetBool(Ctx.IsGrindingHash, true);

        // --- УМНАЯ ЛОГИКА ОПРЕДЕЛЕНИЯ ОСИ РЕЛЬСЫ ---
        Transform rail = Ctx.LastGrindableRail;
        var railCollider = rail.GetComponent<Collider>();
        if (railCollider == null)
        {
            Debug.LogError("Grindable object is missing a Collider!", rail.gameObject);
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // Определяем самую длинную ось коллайдера. Это и будет направлением рельсы.
        Vector3 size = railCollider.bounds.size;
        if (size.x > size.y && size.x > size.z)
        {
            _railAxis = rail.right; // Самая длинная ось - X
        }
        else if (size.y > size.x && size.y > size.z)
        {
            _railAxis = rail.up; // Самая длинная ось - Y
        }
        else
        {
            _railAxis = rail.forward; // Самая длинная ось - Z
        }

        // Определяем, в какую сторону двигаться вдоль этой оси, на основе скорости игрока
        float dot = Vector3.Dot(Ctx.Velocity, _railAxis);
        _grindDirection = (dot >= 0) ? _railAxis : -_railAxis;

        // Сбрасываем вертикальную скорость
        var velocity = Ctx.Velocity;
        velocity.y = 0;
        Ctx.Velocity = velocity;

        // Принудительно приклеиваемся в первый раз
        SnapToRail();
    }

    public override void UpdateState()
    {
        if (Input.GetButtonDown("Jump"))
        {
            var velocity = _grindDirection * Ctx.grindSpeed; // Сохраняем инерцию от грайнда
            velocity.y = Ctx.jumpForce;
            Ctx.Velocity = velocity;
            Ctx.animator.SetTrigger(Ctx.JumpHash);
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        SnapToRail();

        // Проверяем, не закончилась ли рельса
        if (!Physics.Raycast(Ctx.transform.position, Vector3.down, 1.5f))
        {
            // Устанавливаем инерцию перед падением
            Ctx.Velocity = _grindDirection * Ctx.grindSpeed;
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // Движение
        Vector3 movement = _grindDirection * Ctx.grindSpeed * Time.deltaTime;
        Ctx.Controller.Move(movement);

        // Поворот
        Quaternion targetRotation = Quaternion.LookRotation(_grindDirection, Vector3.up);
        Ctx.transform.rotation = Quaternion.Slerp(Ctx.transform.rotation, targetRotation, Time.deltaTime * 15f);
    }

    private void SnapToRail()
    {
        Transform rail = Ctx.LastGrindableRail;
        if (rail == null) return;

        // Проекция положения игрока на ось рельсы
        Vector3 playerToRailCenter = Ctx.transform.position - rail.position;
        float projection = Vector3.Dot(playerToRailCenter, _railAxis);
        Vector3 pointOnRailLine = rail.position + _railAxis * projection;

        // Позиционирование на верхушке коллайдера
        float railTopY = rail.GetComponent<Collider>().bounds.max.y;
        Vector3 targetPosition = new Vector3(pointOnRailLine.x, railTopY, pointOnRailLine.z);

        // Плавно притягиваемся
        Ctx.transform.position = Vector3.Lerp(Ctx.transform.position, targetPosition, Time.deltaTime * 20f);
    }

    public override void ExitState()
    {
        Ctx.animator.SetBool(Ctx.IsGrindingHash, false);
    }
}


// --- Состояние: Бег по стене (Wall-Running) ---
public class WallRunningState : PlayerBaseState
{
    private float _wallRunTimer;

    public WallRunningState(AdvancedPlayerController context) : base(context) { }

    public override void EnterState()
    {
        Ctx.animator.SetBool(Ctx.IsWallRunningHash, true);
        _wallRunTimer = Ctx.wallRunDuration;

        // Математика для определения направления бега по стене.
        // WallNormal - вектор от стены к нам.
        // Vector3.up - вектор вверх.
        // Vector3.Cross - векторное произведение, дает вектор, перпендикулярный двум другим.
        // Это и будет направлением движения вдоль стены.
        Vector3 wallForward = Vector3.Cross(Ctx.WallNormal, Vector3.up);

        // Определяем, в какую сторону бежать (вперед или назад вдоль стены)
        // Сравниваем с направлением взгляда игрока (камеры)
        if (Vector3.Dot(Ctx.cameraTransform.forward, wallForward) < 0)
        {
            wallForward = -wallForward; // Если смотрим в противоположную сторону, инвертируем
        }

        // Устанавливаем скорость для бега по стене
        var velocity = wallForward * Ctx.wallRunSpeed;
        velocity.y = 0; // Сбрасываем вертикальную скорость, чтобы не падать сразу
        Ctx.Velocity = velocity;

        // Поворачиваем персонажа
        Ctx.transform.rotation = Quaternion.LookRotation(wallForward);
    }

    public override void UpdateState()
    {
        _wallRunTimer -= Time.deltaTime;

        // 1. Проверка на переход
        // Таймер вышел, или мы больше не касаемся стены (упрощенная проверка)
        if (_wallRunTimer <= 0)
        {
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // Прыжок от стены
        if (Input.GetButtonDown("Jump"))
        {
            Ctx.animator.SetTrigger(Ctx.JumpHash);
            // Прыжок происходит в направлении от стены и вверх
            Vector3 jumpDirection = (Ctx.WallNormal + Vector3.up).normalized;
            Ctx.Velocity = jumpDirection * Ctx.wallJumpForce; // Полностью заменяем скорость для мощного отталкивания
            Ctx.SwitchState(Ctx.InAirState);
            return;
        }

        // 2. Логика движения
        // Небольшая сила, прижимающая к стене, и антигравитация
        Vector3 antiGravity = -Ctx.gravity * 0.5f * Time.deltaTime * Vector3.up;
        Ctx.Controller.Move((Ctx.Velocity + antiGravity) * Time.deltaTime);

        // Постоянно проверяем, есть ли стена рядом, чтобы не бежать по воздуху
        // Для этого пускаем луч в сторону стены
        if (!Physics.Raycast(Ctx.transform.position, -Ctx.WallNormal, 1.0f))
        {
            Ctx.SwitchState(Ctx.InAirState);
        }
    }

    public override void ExitState() 
    {
        Ctx.animator.SetBool(Ctx.IsWallRunningHash, false);
    }
}


#endregion