using UnityEngine;

public static class HeroStatusText
{
    public static string GetStatName(StatType statType)
    {
        return statType switch
        {
            StatType.MAX_HEALTH => "Max Health",
            StatType.RUN_SPEED => "Run Speed",
            StatType.DAMAGE => "Damage",
            StatType.LEVEL => "Level",
            StatType.EXPERIENCE => "Experience",
            StatType.ATTACK_SPEED => "Attack Speed",
            StatType.CRITICAL_CHANCE => "Critical Chance",
            StatType.CRITICAL_DAMAGE => "Critical Damage",
            StatType.COOLDOWN_REDUCTION => "Cooldown Reduction",
            StatType.ARMOR => "Armor",
            StatType.LIFE_STEAL => "Life Steal",
            StatType.DODGE_CHANCE => "Dodge Chance",
            StatType.DAMAGE_AMPLIFICATION => "Damage Amplification",
            _ => statType.ToString()
        };
    }

    public static string FormatStat(StatType statType, float value)
    {
        if (IsPercentage(statType))
        {
            return $"{value * 100f:0.##}%";
        }

        if (statType == StatType.CRITICAL_DAMAGE)
        {
            return $"{value:0.##}x";
        }

        return Mathf.Approximately(value, Mathf.Round(value))
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.##");
    }

    private static bool IsPercentage(StatType statType)
    {
        return statType == StatType.CRITICAL_CHANCE
            || statType == StatType.COOLDOWN_REDUCTION
            || statType == StatType.LIFE_STEAL
            || statType == StatType.DODGE_CHANCE
            || statType == StatType.DAMAGE_AMPLIFICATION;
    }
}
