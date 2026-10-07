using System;
using UnityEngine;

[Serializable]
public sealed class BuffStatValue : StatValue
{
    [SerializeField] private RarityType sourceRarity;

    public BuffStatValue()
    {
    }

    public BuffStatValue(StatValue stat, RarityType rarity)
        : base(stat.StatType, stat.Value, stat.Operation)
    {
        sourceRarity = rarity;
    }

    public RarityType SourceRarity => sourceRarity;
}
