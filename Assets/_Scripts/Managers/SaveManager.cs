using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class SaveManager
{
    // ��������������� ����� ��� ������������ �������. �� �� ������ ���� � ��������� �����.
    [System.Serializable]
    private class SerializableSaveData
    {
        public int currency;
        public int totalPurchasesMade;
        public int totalCurrencySpent;
        public List<string> unlockedPassiveKeys;
        public List<int> unlockedPassiveValues;
    }

    private static string saveFilePath = Path.Combine(Application.persistentDataPath, "savedata.json");

    public static void SaveGame(SaveData data)
    {
        // 1. ������������ ������� � ������
        SerializableSaveData serializableData = new SerializableSaveData
        {
            currency = data.currency,
            totalPurchasesMade = data.totalPurchasesMade,
            totalCurrencySpent = data.totalCurrencySpent,
            unlockedPassiveKeys = data.unlockedPassives.Keys.ToList(),
            unlockedPassiveValues = data.unlockedPassives.Values.ToList()
        };

        // 2. ����������� � ���������
        string json = JsonUtility.ToJson(serializableData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"���� ��������� �: {saveFilePath}");
    }

    public static SaveData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SerializableSaveData serializableData = JsonUtility.FromJson<SerializableSaveData>(json);

            // 1. ������� ����� ������ SaveData
            SaveData data = new SaveData
            {
                currency = serializableData.currency,
                totalPurchasesMade = serializableData.totalPurchasesMade,
                totalCurrencySpent = serializableData.totalCurrencySpent,
                unlockedPassives = new Dictionary<string, int>()
            };

            // 2. �������� ������� ������� �� �������
            for (int i = 0; i < serializableData.unlockedPassiveKeys.Count; i++)
            {
                data.unlockedPassives.Add(serializableData.unlockedPassiveKeys[i], serializableData.unlockedPassiveValues[i]);
            }

            Debug.Log("���������� ������� ���������.");
            return data;
        }
        else
        {
            Debug.LogWarning("���� ���������� �� ������. ������� ����� ����������.");
            return new SaveData();
        }
    }
}