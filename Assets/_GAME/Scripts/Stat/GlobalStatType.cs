using System.Collections.Generic;
using UnityEngine;

public enum GlobalStatType
{
    LEVEL = 0,
    EXPERIENCE = 1,
    GOLD_BUFF = 2,
    EXP_BUFF = 3,
    DAMAGE_BUFF = 4,
    SPEED_BUFF = 5
}

public static class GlobalStatTypeUtility
{
    public const int Count = (int)GlobalStatType.SPEED_BUFF + 1;

    public static bool IsValid(GlobalStatType statType)
    {
        int index = (int)statType;
        return index >= 0 && index < Count;
    }

    public static float NormalizeValue(GlobalStatType statType, float value)
    {
        switch (statType)
        {
            case GlobalStatType.LEVEL:
                return Mathf.Max(1f, value);
            case GlobalStatType.EXPERIENCE:
            case GlobalStatType.GOLD_BUFF:
            case GlobalStatType.EXP_BUFF:
            case GlobalStatType.DAMAGE_BUFF:
            case GlobalStatType.SPEED_BUFF:
                return Mathf.Max(0f, value);
            default:
                return value;
        }
    }

    public static float GetDefaultValue(GlobalStatType statType)
    {
        return statType == GlobalStatType.LEVEL ? 1f : 0f;
    }

    public static void EnsureListSize(List<float> stats)
    {
        if (stats == null)
        {
            return;
        }

        while (stats.Count < Count)
        {
            stats.Add(GetDefaultValue((GlobalStatType)stats.Count));
        }

        if (stats.Count > Count)
        {
            stats.RemoveRange(Count, stats.Count - Count);
        }
    }
}
