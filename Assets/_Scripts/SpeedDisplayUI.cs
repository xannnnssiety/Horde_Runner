using UnityEngine;
using UnityEngine.UI; // Используйте это, если выбрали обычный UI Text
// using TMPro; // Используйте это, если выбрали TextMeshPro - Text, и закомментируйте UnityEngine.UI

public class SpeedDisplayUI : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement; // Ссылка на скрипт PlayerMovement
    public Text speedTextLabel;           // Ссылка на компонент UI Text
    // public TextMeshProUGUI speedTextLabelTMP; // Используйте эту строку вместо верхней, если используете TextMeshPro

    [Header("Settings")]
    public string prefixText = "Speed: "; // Текст, который будет отображаться перед значением скорости
    public string formatString = "F1";    // Формат для отображения числа (F1 = 1 знак после запятой, F2 = 2 знака и т.д.)

    void Start()
    {
        // Попытка найти PlayerMovement, если он не назначен в инспекторе
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("SpeedDisplayUI: PlayerMovement скрипт не найден на сцене и не назначен!");
                enabled = false; // Отключаем скрипт, если нет игрока
                return;
            }
        }

        // Проверка, назначен ли текстовый компонент
#if UNITY_EDITOR // Этот блок будет работать только в редакторе
        if (speedTextLabel == null
            // && speedTextLabelTMP == null // Раскомментируйте эту часть, если используете TextMeshPro
            )
        {
            Debug.LogError("SpeedDisplayUI: Текстовый компонент (Text или TextMeshProUGUI) не назначен в инспекторе!");
            enabled = false; // Отключаем скрипт
            return;
        }
#endif
    }

    void Update()
    {
        if (playerMovement == null) return; // Дополнительная проверка на случай, если игрок уничтожен

        // Получаем текущую скорость
        float currentSpeed = playerMovement.currentMoveSpeed;

        // Обновляем текст
        if (speedTextLabel != null)
        {
            speedTextLabel.text = prefixText + currentSpeed.ToString(formatString) + " km/h";
        }
        /* // Раскомментируйте этот блок, если используете TextMeshPro, и закомментируйте блок выше
        else if (speedTextLabelTMP != null)
        {
            speedTextLabelTMP.text = prefixText + currentSpeed.ToString(formatString);
        }
        */
    }
}