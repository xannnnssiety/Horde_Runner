using UnityEngine;

// Ётот компонент требует, чтобы на объекте был главный PlayerController
[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour
{
    // —сылка на главный контроллер дл€ записи данных
    private PlayerController _controller;

    private void Awake()
    {
        // ѕолучаем ссылку на контроллер при старте
        _controller = GetComponent<PlayerController>();
    }

    // Ётот метод будет вызыватьс€ из Update() главного контроллера
    public void TickUpdate()
    {
        // —читываем оси ввода
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // —оздаем вектор направлени€ и нормализуем его, чтобы избежать ускорени€ по диагонали
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // «аписываем полученное направление в публичное свойство главного контроллера,
        // чтобы все остальные модули могли его прочитать
        _controller.InputDirection = direction;

        // «десь же можно будет обрабатывать и другие нажати€, например:
        // if (Input.GetButtonDown("Dash")) { _controller.OnDashInput(); }
        // if (Input.GetButtonDown("Shoot")) { _controller.OnShootInput(); }
    }
}