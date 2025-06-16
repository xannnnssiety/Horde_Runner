using UnityEngine;
using UnityEngine.UI; // ќб€зательно добавьте эту строку дл€ работы с UI

[RequireComponent(typeof(PlayerController))]
public class PlayerUI : MonoBehaviour
{
    [Header("UI —сылки")]
    [Tooltip("ѕеретащите сюда текстовый элемент (Legacy) дл€ отображени€ скорости")]
    public Text speedDisplayText;

    // —сылка на главный контроллер дл€ получени€ данных
    private PlayerController _controller;

    private void Awake()
    {
        // ѕолучаем ссылку на контроллер
        _controller = GetComponent<PlayerController>();
    }

    // ƒл€ UI-элементов, которые просто "читают" данные, 
    // часто удобнее использовать собственный Update,
    // чтобы максимально отделить их от игровой логики.
    private void Update()
    {
        // ¬сегда провер€ем, назначена ли ссылка в инспекторе, чтобы избежать ошибок
        if (speedDisplayText != null)
        {
            // ¬ычисл€ем фактическую горизонтальную скорость персонажа
            float horizontalSpeed = new Vector3(_controller.PlayerVelocity.x, 0f, _controller.PlayerVelocity.z).magnitude;

            // ќбновл€ем текст
            speedDisplayText.text = $"Speed: {horizontalSpeed:F2}";
        }
    }
}