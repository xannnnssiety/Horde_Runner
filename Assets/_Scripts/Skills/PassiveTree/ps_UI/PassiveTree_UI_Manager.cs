using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveTree_UI_Manager : MonoBehaviour
{
    [Header("������ �� ������")]
    [SerializeField] private PassiveSkillTree skillTreeAsset;
    [SerializeField] private GameManager gameManager; // ����� ��� ������� � SaveData

    [Header("��������� UI")]
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private GameObject skillNodePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform nodeContainer; // ������, ���� ����� ����������� ����
    [SerializeField] private Transform lineContainer;
    [SerializeField] private float gridSpacing = 150f; // ���������� ����� ������

    [Header("������� ����� �� �����")]
    [Tooltip("������ ��� ������� ������� (Normal)")]
    [SerializeField] private Vector2 normalNodeSize = new Vector2(80, 80);
    [Tooltip("������ ��� �������������� ������� (Notable)")]
    [SerializeField] private Vector2 notableNodeSize = new Vector2(120, 120);
    [Tooltip("������ ��� �������� ������� (Keystone)")]
    [SerializeField] private Vector2 keystoneNodeSize = new Vector2(160, 160);

    [Header("������ �� ����������")]
    [SerializeField] private PassiveTree_Navigation navigationController;

    [Header("����� �����")]
    [SerializeField] private Color unlockedLineColor = Color.yellow;
    [SerializeField] private Color lockedLineColor = Color.gray;
    [Header("����� �������")]
    [SerializeField] private Color unlockedNodeColor = Color.yellow;
    [SerializeField] private Color canBeUnlockedNodeColor = Color.white;
    [SerializeField] private Color lockedNodeColor = Color.gray;


    // ������ �� "��������" ��������� ���������
    private PassiveTree_Navigation _navigationController;
    // ������� ��� �������� ������� � UI-����� �� �� ID
    private Dictionary<string, PassiveSkill_UI_Node> _uiNodes = new Dictionary<string, PassiveSkill_UI_Node>();
    private List<Image> _connectionLines = new List<Image>();


    private void Awake()
    {
        // �������� ������ �� ���������, ������� ����� �� ���� �� �������
        _navigationController = GetComponent<PassiveTree_Navigation>();
    }

    void Start()
    {
        GenerateTree();
    }

    public void GenerateTree()
    {
        // ������� ������ ������
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        foreach (Transform child in lineContainer) Destroy(child.gameObject);
        _uiNodes.Clear();

        // --- ���� 1: ������� � ��������� ��� ���� ---
        foreach (PassiveSkillData skillData in skillTreeAsset.allSkills)
        {
            GameObject nodeObject = Instantiate(skillNodePrefab, nodeContainer);
            /*nodeObject.GetComponent<RectTransform>().anchoredPosition = skillData.gridPosition * gridSpacing;*/

            RectTransform nodeRectTransform = nodeObject.GetComponent<RectTransform>();
            // ������������� ������� �� �����
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
                default: // �� ������, ���� �� ������� ����� ���, � ���� ������ - ���
                    nodeRectTransform.sizeDelta = normalNodeSize;
                    break;
            }

            PassiveSkill_UI_Node uiNode = nodeObject.GetComponent<PassiveSkill_UI_Node>();
            uiNode.Setup(skillData, this);
            _uiNodes.Add(skillData.skillID, uiNode);
        }

        CalculateContentBounds();

        // --- ���� 2: ������ ����� ����� ������ ---
        DrawConnectionLines();

        // --- ���� 3: ��������� ������� ��� ���� ����� ---
        UpdateAllNodeVisuals();

        if (navigationController != null)
        {
            navigationController.EnableNavigation();
        }
        else
        {
            Debug.LogError("������ �� PassiveTree_Navigation �� ����������� � UI ���������!", this);
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
        // ������� ������ ������ �����
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
                        // ������� ������ �����
                        GameObject lineObj = Instantiate(linePrefab, lineContainer);
                        Image lineImage = lineObj.GetComponent<Image>();

                        // --- ���������� ��� ���������������� (�������� ��� ���������) ---
                        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
                        Vector2 parentPos = parentNode.GetComponent<RectTransform>().anchoredPosition;
                        Vector2 childPos = childNode.GetComponent<RectTransform>().anchoredPosition;
                        Vector2 direction = (childPos - parentPos).normalized;
                        float distance = Vector2.Distance(parentPos, childPos);
                        lineRect.anchoredPosition = parentPos;
                        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        lineRect.rotation = Quaternion.Euler(0, 0, angle);

                        // --- ��������� ����� � ������ ---
                        _connectionLines.Add(lineImage);
                    }
                }
            }
        }

        // ����� �������� ���� �����, ���� ��� ��������� �� ����
        UpdateLineVisuals();
    }

    private void UpdateLineVisuals()
    {
        SaveData saveData = SaveManager.LoadGame();
        int lineIndex = 0; // ��� ����� �������, ����� ����������� ����� � ��������

        foreach (PassiveSkillData skillData in skillTreeAsset.allSkills)
        {
            if (skillData.prerequisites != null)
            {
                foreach (PassiveSkillData prerequisiteData in skillData.prerequisites)
                {
                    // ��������, ��� ������ �� ������� �� ������� ������
                    if (lineIndex < _connectionLines.Count)
                    {
                        bool isParentUnlocked = saveData.unlockedPassiveIDs.Contains(prerequisiteData.skillID);
                        bool isChildUnlocked = saveData.unlockedPassiveIDs.Contains(skillData.skillID);

                        // ������������ ��������������� �����
                        _connectionLines[lineIndex].color = (isParentUnlocked && isChildUnlocked) ? unlockedLineColor : lockedLineColor;
                        lineIndex++;
                    }
                }
            }
        }
    }

    public void UpdateAllNodeVisuals()
    {
        SaveData saveData = SaveManager.LoadGame(); // �������� ���������� ������ ����������

        foreach (var entry in _uiNodes)
        {
            string skillID = entry.Key;
            PassiveSkill_UI_Node uiNode = entry.Value;
            PassiveSkillData skillData = skillTreeAsset.allSkills.Find(s => s.skillID == skillID);

            bool isUnlocked = saveData.unlockedPassiveIDs.Contains(skillID);
            bool canBeUnlocked = true; // �� ��������� �������, ��� ����� �������

            // ���������, ������� �� ��� "��������"
            foreach (var prerequisite in skillData.prerequisites)
            {
                if (!saveData.unlockedPassiveIDs.Contains(prerequisite.skillID))
                {
                    canBeUnlocked = false;
                    break;
                }
            }

            // ���� ����� ��� ������, �� �� ����� ���� "�������� ��� ��������"
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

    // ���� ����� ���������� �� ��������� ���� ��� �����
    public void OnSkillNodeClicked(PassiveSkillData clickedSkill)
    {
        // �������� ������� �� ������������� � GameManager
        gameManager.UnlockPassive(clickedSkill);

        // ����� ������� ������������� ��������� ��� ������, ����� ���������� ���������
        UpdateAllNodeVisuals();
        UpdateLineVisuals();
    }
}