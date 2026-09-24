using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    MAX_HEALTH = 0,
    RUN_SPEED = 1,
    DAMAGE = 2,
    LEVEL = 3,
    EXPERIENCE = 4,
    ATTACK_SPEED = 5,
    CRITICAL_CHANCE = 6,
    CRITICAL_DAMAGE = 7,
    COOLDOWN_REDUCTION = 8,
    ARMOR = 9,
    LIFE_STEAL = 10,
    DODGE_CHANCE = 11,
    DAMAGE_AMPLIFICATION = 12
}

public static class StatTypeUtility
{
    public const int StatCount = (int)StatType.DAMAGE_AMPLIFICATION + 1;

    public static bool IsValid(StatType statType)
    {
        int statIndex = (int)statType;
        return statIndex >= 0 && statIndex < StatCount;
    }

    public static void EnsureListSize(List<float> stats)
    {
        if (stats == null)
        {
            return;
        }

        // Mỗi index của list phải trùng với giá trị số đã khai báo trong StatType.
        while (stats.Count < StatCount)
        {
            stats.Add(GetDefaultValue((StatType)stats.Count));
        }

        if (stats.Count > StatCount)
        {
            stats.RemoveRange(StatCount, stats.Count - StatCount);
        }
    }

    public static float NormalizeValue(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.MAX_HEALTH:
            case StatType.LEVEL:
                return Mathf.Max(1f, value);

            case StatType.ATTACK_SPEED:
                return Mathf.Max(0.01f, value);

            case StatType.CRITICAL_DAMAGE:
                return Mathf.Max(1f, value);

            case StatType.CRITICAL_CHANCE:
            case StatType.COOLDOWN_REDUCTION:
            case StatType.LIFE_STEAL:
            case StatType.DODGE_CHANCE:
                return Mathf.Clamp01(value);

            case StatType.RUN_SPEED:
            case StatType.DAMAGE:
            case StatType.EXPERIENCE:
            case StatType.ARMOR:
            case StatType.DAMAGE_AMPLIFICATION:
                return Mathf.Max(0f, value);

            default:
                return value;
        }
    }

    private static float GetDefaultValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.MAX_HEALTH:
                return 100f;
            case StatType.RUN_SPEED:
                return 5f;
            case StatType.DAMAGE:
                return 10f;
            case StatType.LEVEL:
                return 1f;
            case StatType.ATTACK_SPEED:
                return 1f;
            case StatType.CRITICAL_CHANCE:
                return 0.05f;
            case StatType.CRITICAL_DAMAGE:
                return 1.5f;
            default:
                return 0f;
        }
    }
}
