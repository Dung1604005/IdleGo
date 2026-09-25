using System;

public enum StatModifierOperation
{
    FLAT = 0,
    // Các modifier cùng loại được cộng lại trước khi nhân, ví dụ 0.1 + 0.2 = +30%.
    PERCENT_ADD = 1,
    // Mỗi modifier được nhân riêng, ví dụ 0.1 và 0.2 tạo thành x1.1 x1.2.
    PERCENT_MULTIPLY = 2
}

[Serializable]
public sealed class StatModifier
{
    [NonSerialized] private readonly object source;
    private readonly StatType statType;
    private readonly float value;
    private readonly StatModifierOperation operation;

    public StatModifier(
        object source,
        StatType statType,
        float value,
        StatModifierOperation operation = StatModifierOperation.FLAT)
    {
        this.source = source;
        this.statType = statType;
        this.value = value;
        this.operation = operation;
    }

    public object Source => source;
    public StatType StatType => statType;
    public float Value => value;
    public StatModifierOperation Operation => operation;
}
