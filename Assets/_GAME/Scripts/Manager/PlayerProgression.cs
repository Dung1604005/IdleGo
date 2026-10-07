using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgression
{
    [SerializeField] private GlobalStat globalStats = new GlobalStat();

    public GlobalStat GlobalStats => globalStats ??= new GlobalStat();
    public int GlobalLevel => GlobalStats.CurrentLevel;

    public bool SetLevel(int value)
    {
        int normalizedValue = Mathf.Max(1, value);
        if (GlobalLevel == normalizedValue)
        {
            return false;
        }

        GlobalStats.SetStat(GlobalStatType.LEVEL, normalizedValue);
        return true;
    }

    public int AddEnemyExperience(int baseExperience)
    {
        return GlobalStats.AddRewardExperience(baseExperience);
    }

    public void CopyTo(List<float> output)
    {
        GlobalStats.CopyTo(output);
    }

    public void Restore(IReadOnlyList<float> savedStats, int legacyLevel)
    {
        globalStats ??= new GlobalStat();
        globalStats.Restore(savedStats, legacyLevel);
    }
}
