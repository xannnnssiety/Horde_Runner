using UnityEngine;
using System.IO;

// Статический класс - его не нужно создавать или вешать на объект.
// Его методы можно вызывать напрямую: SaveManager.SaveGame(...)
public static class SaveManager
{
    // Путь к файлу сохранения. Application.persistentDataPath - это специальная папка
    // в системе, предназначенная для хранения данных игры (безопасна и не удаляется при обновлении).
    private static string saveFilePath = Path.Combine(Application.persistentDataPath, "savedata.json");

    /// <summary>
    /// Сохраняет данные игры в JSON файл.
    /// </summary>
    /// <param name="data">Объект SaveData для сохранения.</param>
    public static void SaveGame(SaveData data)
    {
        // Превращаем объект в строку формата JSON
        string json = JsonUtility.ToJson(data, true); // true для красивого форматирования

        // Записываем строку в файл
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Игра сохранена в: {saveFilePath}");
    }

    /// <summary>
    /// Загружает данные игры из JSON файла.
    /// </summary>
    /// <returns>Загруженный объект SaveData или новый, если файл не найден.</returns>
    public static SaveData LoadGame()
    {
        // Проверяем, существует ли файл сохранения
        if (File.Exists(saveFilePath))
        {
            // Читаем весь текст из файла
            string json = File.ReadAllText(saveFilePath);

            // Превращаем JSON строку обратно в объект SaveData
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            Debug.Log("Сохранение успешно загружено.");
            return data;
        }
        else
        {
            // Если файла нет, это первый запуск. Создаем новые данные.
            Debug.LogWarning("Файл сохранения не найден. Создано новое сохранение.");
            return new SaveData();
        }
    }
}