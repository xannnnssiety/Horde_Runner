using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    [Tooltip("������� ���������� ������")]
    public int currency;

    [Tooltip("����� ���������� �������, ������ �� �������� ���")]
    public int totalPurchasesMade;

    public int totalCurrencySpent;

    // ���������� ������� ��� �������� ������ �������� ������� ������
    // ����: skillID, ��������: ������� ������� (������� ��� ������)
    public Dictionary<string, int> unlockedPassives;

    // ����������� ��� ������ ����������
    public SaveData()
    {
        currency = 0;
        totalPurchasesMade = 0;
        totalCurrencySpent = 0;
        unlockedPassives = new Dictionary<string, int>();
    }
}