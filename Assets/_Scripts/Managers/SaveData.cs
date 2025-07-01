using System.Collections.Generic;

// Атрибут [System.Serializable] ОБЯЗАТЕЛЕН.
// Он говорит Unity, что объекты этого класса можно превращать в JSON и обратно.
[System.Serializable]
public class SaveData
{
    // Валюта для покупки пассивных умений
    public int currency;

    // Список УНИКАЛЬНЫХ ID изученных навыков.
    // Мы храним не весь навык, а только его ID, что очень эффективно.
    public List<string> unlockedPassiveIDs;

    // Конструктор по умолчанию. Создает "чистое" сохранение для нового игрока.
    public SaveData()
    {
        currency = 0;
        unlockedPassiveIDs = new List<string>();
    }
}