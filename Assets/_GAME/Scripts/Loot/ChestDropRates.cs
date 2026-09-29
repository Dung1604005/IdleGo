using System;
using UnityEngine;

[Serializable]
public class ChestDropRates
{
    [SerializeField, Range(0f, 1f)] private float normalRate = 0.1f;
    [SerializeField, Range(0f, 1f)] private float eliteRate = 0.35f;
    [SerializeField, Range(0f, 1f)] private float bossRate = 1f;

    [NonSerialized] private float normalBonus;
    [NonSerialized] private float eliteBonus;
    [NonSerialized] private float bossBonus;

    public float GetCurrentRate(ChestType chestType)
    {
        return Mathf.Clamp01(GetBaseRate(chestType) + GetBonus(chestType));
    }

    public bool Roll(ChestType chestType)
    {
        float rate = GetCurrentRate(chestType);
        return rate >= 1f || (rate > 0f && UnityEngine.Random.value < rate);
    }

    public void Increase(ChestType chestType, float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        switch (chestType)
        {
            case ChestType.NORMAL: normalBonus += amount; break;
            case ChestType.ELITE: eliteBonus += amount; break;
            case ChestType.BOSS: bossBonus += amount; break;
        }
    }

    public void ResetBonuses()
    {
        normalBonus = 0f;
        eliteBonus = 0f;
        bossBonus = 0f;
    }

    private float GetBaseRate(ChestType chestType)
    {
        return chestType switch
        {
            ChestType.NORMAL => normalRate,
            ChestType.ELITE => eliteRate,
            ChestType.BOSS => bossRate,
            _ => 0f
        };
    }

    private float GetBonus(ChestType chestType)
    {
        return chestType switch
        {
            ChestType.NORMAL => normalBonus,
            ChestType.ELITE => eliteBonus,
            ChestType.BOSS => bossBonus,
            _ => 0f
        };
    }
}
