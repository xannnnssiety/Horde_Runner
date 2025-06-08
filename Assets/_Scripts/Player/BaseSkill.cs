using UnityEngine;

public abstract class BaseSkill : MonoBehaviour
{
    [Header("Base Skill Stats")]
    [Tooltip("Общий урон, нанесенный этим скиллом за забег")]
    public float totalDamageDealt = 0;

    // Этот метод будет ОБЯЗАТЕЛЕН для реализации в каждом дочернем скилле.
    // Он будет отвечать за применение всех статов.
    protected abstract void UpdateSkillStats();

    // Подписываемся на событие, когда скилл становится активным
    protected virtual void OnEnable()
    {
        PlayerStatsManager.OnStatsChanged += UpdateSkillStats;
        // Сразу обновляем статы при появлении скилла
        UpdateSkillStats();
    }

    // Отписываемся, когда скилл выключается или уничтожается
    protected virtual void OnDisable()
    {
        PlayerStatsManager.OnStatsChanged -= UpdateSkillStats;
    }
}