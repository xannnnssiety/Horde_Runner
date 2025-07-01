using UnityEngine;
using System.IO;

// ����������� ����� - ��� �� ����� ��������� ��� ������ �� ������.
// ��� ������ ����� �������� ��������: SaveManager.SaveGame(...)
public static class SaveManager
{
    // ���� � ����� ����������. Application.persistentDataPath - ��� ����������� �����
    // � �������, ��������������� ��� �������� ������ ���� (��������� � �� ��������� ��� ����������).
    private static string saveFilePath = Path.Combine(Application.persistentDataPath, "savedata.json");

    /// <summary>
    /// ��������� ������ ���� � JSON ����.
    /// </summary>
    /// <param name="data">������ SaveData ��� ����������.</param>
    public static void SaveGame(SaveData data)
    {
        // ���������� ������ � ������ ������� JSON
        string json = JsonUtility.ToJson(data, true); // true ��� ��������� ��������������

        // ���������� ������ � ����
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"���� ��������� �: {saveFilePath}");
    }

    /// <summary>
    /// ��������� ������ ���� �� JSON �����.
    /// </summary>
    /// <returns>����������� ������ SaveData ��� �����, ���� ���� �� ������.</returns>
    public static SaveData LoadGame()
    {
        // ���������, ���������� �� ���� ����������
        if (File.Exists(saveFilePath))
        {
            // ������ ���� ����� �� �����
            string json = File.ReadAllText(saveFilePath);

            // ���������� JSON ������ ������� � ������ SaveData
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            Debug.Log("���������� ������� ���������.");
            return data;
        }
        else
        {
            // ���� ����� ���, ��� ������ ������. ������� ����� ������.
            Debug.LogWarning("���� ���������� �� ������. ������� ����� ����������.");
            return new SaveData();
        }
    }
}