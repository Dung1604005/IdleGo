using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuffStatType
{
    SOCKET = 0,
    ENCHANTMENT = 1,
    DECORATION = 2
}

public static class BuffStatTypeUtility
{
    public const int Count = (int)BuffStatType.DECORATION + 1;

    public static bool IsValid(BuffStatType buffStatType)
    {
        int index = (int)buffStatType;
        return index >= 0 && index < Count;
    }
}

[Serializable]
public sealed class BuffStatGroup
{
    [SerializeField] private List<StatValue> stats = new List<StatValue>();

    public IReadOnlyList<StatValue> Stats => MutableStats;

    internal List<StatValue> MutableStats
    {
        get
        {
            stats ??= new List<StatValue>();
            return stats;
        }
    }
}
