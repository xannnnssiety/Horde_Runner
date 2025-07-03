using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveTree_UI_Manager : MonoBehaviour
{
    [Header("Ссылки на ассеты")]
    [SerializeField] private PassiveSkillTree skillTreeAsset;
    [SerializeField] private GameManager gameManager; // Нужен для доступа к SaveData

    [Header("Настройки UI")]
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private GameObject skillNodePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform nodeContainer; // Панель, куда будут создаваться узлы
    [SerializeField] private Transform lineContainer;
    [SerializeField] private float gridSpacing = 150f; // Расстояние между узлами

    [Header("Размеры узлов по тирам")]
    [Tooltip("Размер для обычных навыков (Normal)")]
    [SerializeField] private Vector2 normalNodeSize = new Vector2(80, 80);
    [Tooltip("Размер для примечательных навыков (Notable)")]
    [SerializeField] private Vector2 notableNodeSize = new Vector2(120, 120);
    [Tooltip("Размер для ключевых навыков (Keystone)")]
    [SerializeField] private Vector2 keystoneNodeSize = new Vector2(160, 160);

    [Header("Ссылки на компоненты")]
    [SerializeField] private PassiveTree_Navigation navigationController;

    [Header("Цвета линий")]
    [SerializeField] private Color unlockedLineColor = Color.yellow;
    [SerializeField] private Color lockedLineColor = Color.gray;
    [Header("Цвета скиллов")]
    [SerializeField] private Color unlockedNodeColor = Color.yellow;
    [SerializeField] private Color canBeUnlockedNodeColor = Color.white;
    [SerializeField] private Color lockedNodeColor = Color.gray;


    // Ссылка на "соседний" компонент навигации
    private PassiveTree_Navigation _navigationController;
    // Словарь для быстрого доступа к UI-узлам по их ID
    private Dictionary<string, PassiveSkill_UI_Node> _uiNodes = new Dictionary<string, PassiveSkill_UI_Node>();
    private List<Image> _connectionLines = new List<Image>();


    private void Awake()
    {
        // Получаем ссылку на компонент, который висит на этом же объекте
        _navigationController = GetComponent<PassiveTree_Navigation>();
    }

    void Start()
    {
        GenerateTree();
    }

    public void GenerateTree()
    {
        // Очищаем старое дерево
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        foreach (Transform child in lineContainer) Destroy(child.gameObject);
        _uiNodes.Clear();

        // --- ЭТАП 1: Создаем и размещаем все узлы ---
        foreach (PassiveSkillData skillData in skillTreeAsset.allSkills)
        {
            GameObject nodeObject = Instantiate(skillNodePrefab, nodeContainer);
            /*nodeObject.GetComponent<RectTransform>().anchoredPosition = skillData.gridPosition * gridSpacing;*/

            RectTransform nodeRectTransform = nodeObject.GetComponent<RectTransform>();
            // Устанавливаем позицию на сетке
            nodeRectTransform.anchoredPosition = skillData.gridPosition * gridSpacing;

            switch (skillData.skillTier)
            {
                case PassiveSkillTier.Normal:
                    nodeRectTransform.sizeDelta = normalNodeSize;
                    break;
                case PassiveSkillTier.Notable:
                    nodeRectTransform.sizeDelta = notableNodeSize;
                    break;
                case PassiveSkillTier.Keystone:
                    nodeRectTransform.sizeDelta = keystoneNodeSize;
                    break;
                default: // На случай, если мы добавим новый тир, а сюда логику - нет
                    nodeRectTransform.sizeDelta = normalNodeSize;
                    break;
            }

            PassiveSkill_UI_Node uiNode = nodeObject.GetComponent<PassiveSkill_UI_Node>();
            uiNode.Setup(skillData, this);
            _uiNodes.Add(skillData.skillID, uiNode);
        }

        CalculateContentBounds();

        // --- ЭТАП 2: Рисуем линии между узлами ---
        DrawConnectionLines();

        // --- ЭТАП 3: Обновляем внешний вид всех узлов ---
        UpdateAllNodeVisuals();

        if (navigationController != null)
        {
            navigationController.EnableNavigation();
        }
        else
        {
            Debug.LogError("Ссылка на PassiveTree_Navigation не установлена в UI Менеджере!", this);
        }
    }

    private void CalculateContentBounds()
    {
        if (skillTreeAsset.allSkills.Count == 0) return;
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        foreach (PassiveSkillData skillData in skillTreeAsset.allSkills)
        {
            Vector2 position = skillData.gridPosition * gridSpacing;
            if (position.x < minX) minX = position.x;
            if (position.x > maxX) maxX = position.x;
            if (position.y < minY) minY = position.y;
            if (position.y > maxY) maxY = position.y;
        }
        float padding = gridSpacing;
        contentRect.sizeDelta = new Vector2((maxX - minX) + padding, (maxY - minY) + padding);
    }

    private void DrawConnectionLines()
    {
        // Очищаем старый список линий
        _connectionLines.Clear();

        foreach (PassiveSkillData skillData in skillTreeAsset.allSkills)
        {
            if (skillData.prerequisites != null)
            {
                foreach (PassiveSkillData prerequisiteData in skillData.prerequisites)
                {
                    if (_uiNodes.TryGetValue(skillData.skillID, out PassiveSkill_UI_Node childNode) &&
                        _uiNodes.TryGetValue(prerequisiteData.skillID, out PassiveSkill_UI_Node parentNode))
                    {
                        // Создаем объект линии
                        GameObject lineObj = Instantiate(linePrefab, lineContainer);
                        Image lineImage = lineObj.GetComponent<Image>();

                        // --- Математика для позиционирования (остается без изменений) ---
                        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
                        Vector2 parentPos = parentNode.GetComponent<RectTransform>().anchoredPosition;
                        Vector2 childPos = childNode.GetComponent<RectTransform>().anchoredPosition;
                        Vector2 direction = (childPos - parentPos).normalized;
                        float distance = Vector2.Distance(parentPos, childPos);
                        lineRect.anchoredPosition = parentPos;
                        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        lineRect.rotation = Quaternion.Euler(0, 0, angle);

                        // --- СОХРАНЯЕМ ЛИНИЮ В СПИСОК ---
                        _connectionLines.Add(lineImage);
                    }
                }
            }
        }

        // После создания всех линий, один раз обновляем их цвет
        UpdateLineVisuals();
    }

    private void UpdateLineVisuals()
    {
        SaveData saveData = SaveManager.LoadGame();
        int lineIndex = 0; // Нам нужен счетчик, чтобы сопоставить линии с навыками

        foreach (PassiveSkillData skillData in skillTreeAsset.allSkills)
        {
            if (skillData.prerequisites != null)
            {
                foreach (PassiveSkillData prerequisiteData in skillData.prerequisites)
                {
                    // Убедимся, что индекс не выходит за пределы списка
                    if (lineIndex < _connectionLines.Count)
                    {
                        bool isParentUnlocked = saveData.unlockedPassiveIDs.Contains(prerequisiteData.skillID);
                        bool isChildUnlocked = saveData.unlockedPassiveIDs.Contains(skillData.skillID);

                        // Раскрашиваем соответствующую линию
                        _connectionLines[lineIndex].color = (isParentUnlocked && isChildUnlocked) ? unlockedLineColor : lockedLineColor;
                        lineIndex++;
                    }
                }
            }
        }
    }

    public void UpdateAllNodeVisuals()
    {
        SaveData saveData = SaveManager.LoadGame(); // Получаем актуальные данные сохранения

        foreach (var entry in _uiNodes)
        {
            string skillID = entry.Key;
            PassiveSkill_UI_Node uiNode = entry.Value;
            PassiveSkillData skillData = skillTreeAsset.allSkills.Find(s => s.skillID == skillID);

            bool isUnlocked = saveData.unlockedPassiveIDs.Contains(skillID);
            bool canBeUnlocked = true; // По умолчанию считаем, что можно изучить

            // Проверяем, изучены ли все "родители"
            foreach (var prerequisite in skillData.prerequisites)
            {
                if (!saveData.unlockedPassiveIDs.Contains(prerequisite.skillID))
                {
                    canBeUnlocked = false;
                    break;
                }
            }

            // Если навык уже изучен, он не может быть "доступен для изучения"
            if (isUnlocked)
            {
                canBeUnlocked = false;
            }

            Color targetColor;
            if (isUnlocked)
            {
                targetColor = unlockedNodeColor;
            }
            else if (canBeUnlocked)
            {
                targetColor = canBeUnlockedNodeColor;
            }
            else
            {
                targetColor = lockedNodeColor;
            }

            uiNode.UpdateVisuals(targetColor);
        }
    }

    // Этот метод вызывается из дочернего узла при клике
    public void OnSkillNodeClicked(PassiveSkillData clickedSkill)
    {
        // Передаем команду на разблокировку в GameManager
        gameManager.UnlockPassive(clickedSkill);

        // После попытки разблокировки обновляем все дерево, чтобы отобразить изменения
        UpdateAllNodeVisuals();
        UpdateLineVisuals();
    }
}