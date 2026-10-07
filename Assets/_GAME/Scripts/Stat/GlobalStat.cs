using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class GlobalStat
{
    [SerializeField] private List<float> currentStats = new List<float>();

    public int CurrentLevel => Mathf.RoundToInt(GetStat(GlobalStatType.LEVEL));
    public int CurrentExperience => Mathf.RoundToInt(
        GetStat(GlobalStatType.EXPERIENCE));

    public float GetStat(GlobalStatType statType)
    {
        EnsureStats();
        return GlobalStatTypeUtility.IsValid(statType)
            ? currentStats[(int)statType]
            : 0f;
    }

    public float GetMultiplier(GlobalStatType buffType)
    {
        return IsBuffType(buffType) ? 1f + GetStat(buffType) : 1f;
    }

    public int ApplyExperienceBuff(int baseExperience)
    {
        return Mathf.Max(0, Mathf.RoundToInt(
            Mathf.Max(0, baseExperience)
            * GetMultiplier(GlobalStatType.EXP_BUFF)));
    }

    public int ApplyGoldBuff(int baseGold)
    {
        return Mathf.Max(0, Mathf.RoundToInt(
            Mathf.Max(0, baseGold)
            * GetMultiplier(GlobalStatType.GOLD_BUFF)));
    }

    internal void SetStat(GlobalStatType statType, float value)
    {
        EnsureStats();
        if (GlobalStatTypeUtility.IsValid(statType))
        {
            currentStats[(int)statType] =
                GlobalStatTypeUtility.NormalizeValue(statType, value);
        }
    }

    internal void AddStat(GlobalStatType statType, float amount)
    {
        SetStat(statType, GetStat(statType) + amount);
    }

    internal int AddRewardExperience(int baseExperience)
    {
        int awardedExperience = ApplyExperienceBuff(baseExperience);
        AddStat(GlobalStatType.EXPERIENCE, awardedExperience);
        CheckLevelUp();
        return awardedExperience;
    }

    internal void AddExperience(int amount)
    {
        AddStat(GlobalStatType.EXPERIENCE, Mathf.Max(0, amount));
        CheckLevelUp();
    }

    internal void CopyTo(List<float> output)
    {
        if (output == null)
        {
            return;
        }

        EnsureStats();
        output.Clear();
        output.AddRange(currentStats);
    }

    internal void Restore(
        IReadOnlyList<float> savedStats,
        int legacyGlobalLevel)
    {
        EnsureStats();
        for (int i = 0; i < GlobalStatTypeUtility.Count; i++)
        {
            currentStats[i] = GlobalStatTypeUtility.GetDefaultValue(
                (GlobalStatType)i);
        }

        if (savedStats == null || savedStats.Count == 0)
        {
            SetStat(GlobalStatType.LEVEL, legacyGlobalLevel);
            return;
        }

        int count = Mathf.Min(savedStats.Count, GlobalStatTypeUtility.Count);
        for (int i = 0; i < count; i++)
        {
            SetStat((GlobalStatType)i, savedStats[i]);
        }
    }

    private void CheckLevelUp()
    {
        for (int i = 0; i < 100000; i++)
        {
            int requiredExp = GetExpToNextLevel(CurrentLevel);
            if (CurrentExperience < requiredExp)
            {
                return;
            }

            SetStat(
                GlobalStatType.EXPERIENCE,
                CurrentExperience - requiredExp);
            SetStat(GlobalStatType.LEVEL, CurrentLevel + 1);
        }
    }

    private static int GetExpToNextLevel(int level)
    {
        return Mathf.Max(1, Mathf.RoundToInt(
            GameConfig.BASE_EXP * Mathf.Pow(level, GameConfig.POWER_EXP)));
    }

    private void EnsureStats()
    {
        currentStats ??= new List<float>();
        GlobalStatTypeUtility.EnsureListSize(currentStats);
    }

    private static bool IsBuffType(GlobalStatType statType)
    {
        return statType >= GlobalStatType.GOLD_BUFF
            && statType <= GlobalStatType.SPEED_BUFF;
    }
}
