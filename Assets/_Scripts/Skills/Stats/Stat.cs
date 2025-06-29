using System.Collections.Generic;
using System.Linq;

// Этот класс не висит на объекте, он просто хранит данные в памяти
public class Stat
{
    public float BaseValue;
    public float Value { get; private set; }

    private readonly List<StatModifier> _modifiers;

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
        _modifiers = new List<StatModifier>();
        CalculateFinalValue();
    }

    public void AddModifier(StatModifier mod)
    {
        _modifiers.Add(mod);
        CalculateFinalValue();
    }

    public void RemoveModifier(StatModifier mod)
    {
        _modifiers.Remove(mod);
        CalculateFinalValue();
    }

    private void CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float percentAdd = 0;

        // Сначала применяем все плоские бонусы
        _modifiers.Where(m => m.Type == ModifierType.Flat).ToList()
            .ForEach(m => finalValue += m.Value);

        // Затем суммируем все процентные бонусы
        _modifiers.Where(m => m.Type == ModifierType.PercentAdd).ToList()
            .ForEach(m => percentAdd += m.Value);

        // Применяем сумму процентных бонусов
        finalValue *= 1 + (percentAdd / 100);

        // В конце применяем все мультипликативные бонусы
        _modifiers.Where(m => m.Type == ModifierType.PercentMult).ToList()
            .ForEach(m => finalValue *= 1 + (m.Value / 100));

        Value = finalValue;
    }
}