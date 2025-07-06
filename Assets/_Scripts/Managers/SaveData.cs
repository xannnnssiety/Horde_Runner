using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    [Tooltip("Текущее количество валюты")]
    public int currency;

    [Tooltip("Общее количество покупок, влияет на инфляцию цен")]
    public int totalPurchasesMade;

    public int totalCurrencySpent;

    // Используем словарь для хранения уровня прокачки каждого навыка
    // Ключ: skillID, Значение: текущий уровень (сколько раз купили)
    public Dictionary<string, int> unlockedPassives;

    // Конструктор для нового сохранения
    public SaveData()
    {
        currency = 0;
        totalPurchasesMade = 0;
        totalCurrencySpent = 0;
        unlockedPassives = new Dictionary<string, int>();
    }
}