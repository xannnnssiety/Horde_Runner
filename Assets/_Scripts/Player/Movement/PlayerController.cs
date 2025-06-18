using UnityEngine;
using System;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerGroundedMovement))] // Добавляем требования для модулей
[RequireComponent(typeof(PlayerAirborneMovement))]
[RequireComponent(typeof(PlayerWallMovement))]
[RequireComponent(typeof(PlayerGrind))]
[RequireComponent(typeof(PlayerAnimation))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerDash))]
[RequireComponent(typeof(PlayerSlide))]
[RequireComponent(typeof(PlayerWallRun))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Grounded,
        InAir,
        Grinding,
        WallSliding,
        Sliding,
        WallRunning
    }

    // --- ОБЩЕДОСТУПНЫЕ СВОЙСТВА (API для модулей) ---
    public Vector3 PlayerVelocity { get; set; }
    public Vector3 InputDirection { get; set; }
    public PlayerState CurrentState { get; private set; }
    public float CurrentMoveSpeed { get; set; }
    public bool IsGrounded { get; private set; }
    public float GravityValue => gravity;
    public bool IsWallSliding { get; set; } // Модуль стены будет управлять этим флагом
    
    public float TurnSmoothVelocity; // Переменная для SmoothDampAngle
    public event Action OnJump;
    public event Action OnWallJump;

    // Компоненты
    public CharacterController CharacterController { get; private set; }
    public Animator Animator { get; private set; }
    public Camera MainCamera { get; private set; }

    // --- ССЫЛКИ НА МОДУЛИ ---
    private PlayerInput _inputModule;
    private PlayerGroundedMovement _groundedMovementModule;
    private PlayerAirborneMovement _airborneMovementModule;
    private PlayerWallMovement _wallMovementModule;
    private PlayerGrind _grindModule;
    private PlayerAnimation _animationModule;
    private PlayerDash _dashModule;
    private PlayerSlide _slideModule;
    private PlayerWallRun _wallRunModule;


    [Header("Глобальные настройки")]
    public float baseMoveSpeed = 5f;
    public float maxMoveSpeed = 10f;
    public float turnSmoothTime = 0.1f;
    
    public float coyoteTime = 0.15f;
    public float jumpHeight = 2f; 
    public float speedChangeRate = 2f;

    public float gravity = -41.62f;
    

    [Header("Отладка")]
    [SerializeField] private PlayerState currentStateForInspector;

    private float coyoteTimeCounter;
    private bool wantsToSlide = false;

    private void Awake()
    {
        // Получаем ссылки на основные компоненты
        CharacterController = GetComponent<CharacterController>();
        Animator = GetComponent<Animator>();

        // Получаем ссылки на все наши модули
        _inputModule = GetComponent<PlayerInput>();
        _groundedMovementModule = GetComponent<PlayerGroundedMovement>();
        _airborneMovementModule = GetComponent<PlayerAirborneMovement>();
        _wallMovementModule = GetComponent<PlayerWallMovement>();
        _grindModule = GetComponent<PlayerGrind>();
        _animationModule = GetComponent<PlayerAnimation>();
        _dashModule = GetComponent<PlayerDash>();
        _slideModule = GetComponent<PlayerSlide>();
        _wallRunModule = GetComponent<PlayerWallRun>();
    }

    private void Start()
    {
        

        if (MainCamera == null && Camera.main != null) MainCamera = Camera.main;
        else if (MainCamera == null) Debug.LogError("Камера не найдена и не назначена!");

        CurrentMoveSpeed = baseMoveSpeed;
        SetState(CharacterController.isGrounded ? PlayerState.Grounded : PlayerState.InAir);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        _inputModule.TickUpdate(); // Сначала обновляем ввод
        HandleGroundedCheck();    // Затем проверяем состояние земли
        _dashModule.TickUpdate();
        _slideModule.TickUpdate();


        if (_dashModule.IsDashing || _slideModule.IsSliding || _wallRunModule.IsWallRunning)
        {
            // Мы все еще применяем движение в конце кадра, поэтому дэш будет работать
            if (_wallRunModule.IsWallRunning)
            {
                _wallRunModule.TickUpdate();
            }
        }
        else
        {
            switch (CurrentState)
            {
                case PlayerState.Grounded:
                    _groundedMovementModule.TickUpdate();
                    _grindModule.CheckForGrindStart();
                    break;
                case PlayerState.InAir:
                    _airborneMovementModule.TickUpdate();
                    _wallMovementModule.TickUpdate();
                    _wallRunModule.TickUpdate();
                    _grindModule.CheckForGrindStart();
                    
                    break;
                case PlayerState.Grinding:
                    _grindModule.TickUpdate();
                    break;
                case PlayerState.WallSliding:
                    _wallMovementModule.TickUpdate(); // Модуль стены сам обработает прыжок и падение
                    break;
                case PlayerState.Sliding: 
                    _slideModule.TickUpdate();
                    break;
            }
        }

        ApplyGravity();

        // Финальный шаг: применяем движение
        CharacterController.Move(PlayerVelocity * Time.deltaTime);

        // Обновляем анимации и UI в самом конце
        _animationModule.TickUpdate();
        
    }

    private void HandleGroundedCheck()
    {
        IsGrounded = CharacterController.isGrounded;

        if (IsGrounded && PlayerVelocity.y < 0)
        {
            var velocity = PlayerVelocity;
            velocity.y = -2f;
            PlayerVelocity = velocity;
        }

        if (IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            if (CurrentState == PlayerState.InAir || CurrentState == PlayerState.WallSliding)
            {
                SetState(PlayerState.Grounded);
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (CurrentState == PlayerState.Grounded && coyoteTimeCounter <= 0)
            {
                SetState(PlayerState.InAir);
            }
        }
    }

    private void ApplyGravity()
    {
        // Если активен дэш или грайнд/скольжение по стене, гравитация не нужна
        if (_dashModule.IsDashing || CurrentState == PlayerState.Grinding || IsWallSliding || _wallRunModule.IsWallRunning)
        {
            return; // Выходим из метода
        }

        // Получаем текущее значение гравитации из модуля слайда.
        // Если мы не "пикируем", он вернет стандартное значение.
        float currentGravity = _slideModule.GetCurrentGravity();

        var velocity = PlayerVelocity;
        velocity.y += currentGravity * Time.deltaTime;
        PlayerVelocity = velocity;
    }



    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        currentStateForInspector = newState;
    }

    public bool CanUseCoyoteTime()
    {
        return coyoteTimeCounter > 0f;
    }

    public void ConsumeCoyoteTime()
    {
        coyoteTimeCounter = 0f;
    }
}