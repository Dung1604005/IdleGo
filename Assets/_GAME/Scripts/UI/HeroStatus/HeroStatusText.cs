using UnityEngine;

public static class HeroStatusText
{
    public static string GetStatName(StatType statType)
    {
        return StatTypeUtility.GetDisplayName(statType);
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
