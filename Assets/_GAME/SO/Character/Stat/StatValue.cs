using System;
using UnityEngine;

[Serializable]
public class StatValue
{
    [SerializeField] private StatType statType;
    [SerializeField] private float value;
    [SerializeField] private StatModifierOperation operation;

    public StatValue()
    {
    }

    public StatValue(
        StatType statType,
        float value,
        StatModifierOperation operation = StatModifierOperation.FLAT)
    {
        this.statType = statType;
        this.value = value;
        this.operation = operation;
    }

    public StatType StatType => statType;
    public float Value => value;
    public StatModifierOperation Operation => operation;
}
