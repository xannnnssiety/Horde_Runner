using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerController;

/// <summary>
/// Главный "дирижер" для всех активных умений игрока.
/// Управляет их жизненным циклом: добавлением, перезарядкой, активацией и обновлением.
/// </summary>
public class ActiveSkillManager : MonoBehaviour
{
    // Внутренний класс-обертка для удобного управления каждым умением
    private class ActiveSkillInstance
    {
        public ActiveSkill skillLogic; // Ссылка на компонент с логикой умения
        public float cooldownTimer;    // Персональный таймер перезарядки
    }

    [Header("Ссылки")]
    [Tooltip("Ссылка на компонент PlayerStats. Если не указана, будет искаться на этом же объекте.")]
    [SerializeField] private PlayerStats playerStats;
    

    // "Арсенал" игрока - список всех активных умений, которые у него есть
    private readonly List<ActiveSkillInstance> _activeSkills = new List<ActiveSkillInstance>();

    private void Awake()
    {
        

        // Автоматически находим PlayerStats, если он не указан в инспекторе
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
    }

    private void OnEnable()
    {
        // Подписываемся на событие изменения статов игрока.
        // Это позволит нам динамически обновлять характеристики умений.
        if (playerStats != null)
        {
            playerStats.OnStatChanged += HandleStatChanged;
        }
    }

    private void OnDisable()
    {
        // ОБЯЗАТЕЛЬНО отписываемся, чтобы избежать утечек памяти и ошибок.
        if (playerStats != null)
        {
            playerStats.OnStatChanged -= HandleStatChanged;
        }
    }

    private void Update()
    {
        // Проходим по каждому умению в нашем арсенале
        foreach (var instance in _activeSkills)
        {
            // Уменьшаем его таймер перезарядки
            instance.cooldownTimer -= Time.deltaTime;

            // Если таймер дошел до нуля (или ниже)
            if (instance.cooldownTimer <= 0)
            {
                // 1. Активируем логику умения (выпускаем снаряд, наносим урон и т.д.)
                instance.skillLogic.Activate();

                // 2. ОПОВЕЩАЕМ ВСЮ ИГРУ о том, что умение было использовано.
                // Это нужно для перков вроде "Разрыв Времени".
                GameEvents.ReportPlayerAbilityUsed(instance.skillLogic.skillData);

                // 3. Сбрасываем таймер на текущее значение перезарядки умения
                // Мы используем свойство .currentCooldown из самого умения, которое уже рассчитано
                instance.cooldownTimer += instance.skillLogic.currentCooldown;
            }
        }
    }




    /// <summary>
    /// Добавляет новое умение в арсенал игрока или улучшает существующее.
    /// </summary>
    public void AddSkill(ActiveSkillData skillToAdd)
    {
        // --- Логика улучшения ---
        // Ищем, не является ли новое умение улучшением для уже существующего.
        ActiveSkillInstance skillToUpgrade = _activeSkills.FirstOrDefault(s =>
            (s.skillLogic.skillData.nextLevelSkill != null && s.skillLogic.skillData.nextLevelSkill == skillToAdd) ||
            (s.skillLogic.skillData.ultimateVersionSkill != null && s.skillLogic.skillData.ultimateVersionSkill == skillToAdd)
        );

        // Если нашли умение для улучшения...
        if (skillToUpgrade != null)
        {
            // ...удаляем его из нашего списка и уничтожаем его игровой объект.
            _activeSkills.Remove(skillToUpgrade);
            Destroy(skillToUpgrade.skillLogic.gameObject);
            Debug.Log($"Улучшено умение: {skillToUpgrade.skillLogic.skillData.skillName} -> {skillToAdd.skillName}");
        }
        else
        {
            Debug.Log($"Добавлено новое умение: {skillToAdd.skillName}");
        }

        // --- Логика добавления ---
        // 1. Создаем экземпляр префаба с логикой умения.
        // Делаем его дочерним к этому менеджеру для порядка в иерархии.
        GameObject skillObject = Instantiate(skillToAdd.skillLogicPrefab, transform);
        ActiveSkill newSkill = skillObject.GetComponent<ActiveSkill>();

        // 2. Инициализируем умение, передавая ему данные и ссылку на статы игрока.
        newSkill.Initialize(skillToAdd, playerStats);

        // 3. Создаем новый экземпляр-обертку и добавляем его в наш арсенал.
        _activeSkills.Add(new ActiveSkillInstance
        {
            skillLogic = newSkill,
            cooldownTimer = newSkill.currentCooldown // Начинаем с полного кулдауна
        });
    }

    /// <summary>
    /// Метод, вызываемый событием OnStatChanged из PlayerStats.
    /// </summary>
    private void HandleStatChanged(StatType type, float value)
    {
        RecalculateAllSkillStats();
    }

    /// <summary>
    /// Заставляет все активные умения в арсенале пересчитать свои характеристики.
    /// </summary>
    private void RecalculateAllSkillStats()
    {
        Debug.Log("Статы игрока изменились. Пересчитываем характеристики всех активных умений.");
        foreach (var instance in _activeSkills)
        {
            instance.skillLogic.RecalculateStats();
        }
    }

    public void ForceRecalculateAllSkills()
    {
        RecalculateAllSkillStats();
    }

}