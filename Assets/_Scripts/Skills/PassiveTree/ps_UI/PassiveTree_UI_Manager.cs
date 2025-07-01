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

    [Header("Ссылки на компоненты")]
    [SerializeField] private PassiveTree_Navigation navigationController;

    [Header("Цвета")]
    [SerializeField] private Color unlockedLineColor = Color.yellow;
    [SerializeField] private Color lockedLineColor = Color.gray;


    // Ссылка на "соседний" компонент навигации
    private PassiveTree_Navigation _navigationController;
    // Словарь для быстрого доступа к UI-узлам по их ID
    private Dictionary<string, PassiveSkill_UI_Node> _uiNodes = new Dictionary<string, PassiveSkill_UI_Node>();


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
            nodeObject.GetComponent<RectTransform>().anchoredPosition = skillData.gridPosition * gridSpacing;
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
        SaveData saveData = SaveManager.LoadGame();

        foreach (PassiveSkillData skillData in skillTreeAsset.allSkills)
        {
            // Проходим по всем "родителям" (prerequisites) текущего навыка
            if (skillData.prerequisites != null)
            {
                foreach (PassiveSkillData prerequisiteData in skillData.prerequisites)
                {
                    // Находим UI-узлы для дочернего и родительского навыков
                    if (_uiNodes.TryGetValue(skillData.skillID, out PassiveSkill_UI_Node childNode) &&
                        _uiNodes.TryGetValue(prerequisiteData.skillID, out PassiveSkill_UI_Node parentNode))
                    {
                        // Создаем объект линии из префаба
                        GameObject lineObj = Instantiate(linePrefab, lineContainer);
                        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
                        Image lineImage = lineObj.GetComponent<Image>();

                        // Получаем позиции узлов в координатах Canvas
                        Vector2 parentPos = parentNode.GetComponent<RectTransform>().anchoredPosition;
                        Vector2 childPos = childNode.GetComponent<RectTransform>().anchoredPosition;

                        // --- Математика для позиционирования и вращения линии ---

                        // 1. Устанавливаем позицию линии в позицию родительского узла
                        lineRect.anchoredPosition = parentPos;

                        // 2. Вычисляем вектор направления от родителя к ребенку
                        Vector2 direction = (childPos - parentPos).normalized;

                        // 3. Вычисляем расстояние между узлами - это будет длина нашей линии
                        float distance = Vector2.Distance(parentPos, childPos);

                        // 4. Устанавливаем длину линии (растягиваем по ширине)
                        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);

                        // 5. Находим угол поворота линии
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                        // 6. Применяем поворот
                        lineRect.rotation = Quaternion.Euler(0, 0, angle);

                        // 7. Устанавливаем цвет линии
                        bool isParentUnlocked = saveData.unlockedPassiveIDs.Contains(prerequisiteData.skillID);
                        bool isChildUnlocked = saveData.unlockedPassiveIDs.Contains(skillData.skillID);
                        lineImage.color = (isParentUnlocked && isChildUnlocked) ? unlockedLineColor : lockedLineColor;
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

            uiNode.UpdateVisuals(isUnlocked, canBeUnlocked);
        }
    }

    // Этот метод вызывается из дочернего узла при клике
    public void OnSkillNodeClicked(PassiveSkillData clickedSkill)
    {
        // Передаем команду на разблокировку в GameManager
        gameManager.UnlockPassive(clickedSkill);

        // После попытки разблокировки обновляем все дерево, чтобы отобразить изменения
        UpdateAllNodeVisuals();
    }
}