using System.Collections.Generic;
using System.Linq;

// ���� ����� �� ����� �� �������, �� ������ ������ ������ � ������
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

        // ������� ��������� ��� ������� ������
        _modifiers.Where(m => m.Type == ModifierType.Flat).ToList()
            .ForEach(m => finalValue += m.Value);

        // ����� ��������� ��� ���������� ������
        _modifiers.Where(m => m.Type == ModifierType.PercentAdd).ToList()
            .ForEach(m => percentAdd += m.Value);

        // ��������� ����� ���������� �������
        finalValue *= 1 + (percentAdd / 100);

        // � ����� ��������� ��� ����������������� ������
        _modifiers.Where(m => m.Type == ModifierType.PercentMult).ToList()
            .ForEach(m => finalValue *= 1 + (m.Value / 100));

        Value = finalValue;
    }
}