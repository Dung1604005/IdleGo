using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CharacterStatProgress : IStatModifierSource
{
    [SerializeField, Min(0)] private int availableStatPoints;
    [SerializeField] private List<int> statUpgradeCounts = new List<int>();
    [SerializeField] private CharacterSkillProgress skillProgress =
        new CharacterSkillProgress();

    [NonSerialized] private CharacterStatProgressSO progressData;
    [NonSerialized] private Player owner;
    [NonSerialized] private List<StatModifier> modifierBuffer;
    [NonSerialized] private List<IStatModifierSource> sourceBuffer;
    public string ModifierSourceId => owner != null
        ? $"CHARACTER_PROGRESS_{owner.CharacterId}"
        : "CHARACTER_PROGRESS";
    public int AvailableStatPoints => Mathf.Max(0, availableStatPoints);
    public CharacterStatProgressSO ProgressData => progressData;
    public void OnInit(CharacterStatProgressSO data, Player player)
    {
        progressData = data;
        owner = player;
        skillProgress ??= new CharacterSkillProgress();
        skillProgress.OnInit(progressData);
        EnsureUpgradeCounts();
        ApplyStatModifiers();
    }

    public void OnDespawn()
    {
        owner = null;
        progressData = null;
        modifierBuffer?.Clear();
        sourceBuffer?.Clear();
    }
    public int GetUpgradeCount(StatType statType)
    {
        EnsureUpgradeCounts();
        return StatTypeUtility.IsValid(statType)
            ? Mathf.Max(0, statUpgradeCounts[(int)statType])
            : 0;
    }

    public int GetSkillLevel(CombatSkill skill)
    {
        return skillProgress != null ? skillProgress.GetLevel(skill) : 0;
    }
    public bool TryUpgradeStat(StatType statType)
    {
        if (!TryGetRule(statType, out CharacterStatUpgradeRule rule)
            || availableStatPoints < rule.StatPointCost)
        {
            return false;
        }

        availableStatPoints -= rule.StatPointCost;
        statUpgradeCounts[(int)statType]++;
        ApplyStatModifiers();
        return true;
    }
    public bool TryDowngradeStat(StatType statType)
    {
        if (!TryGetRule(statType, out CharacterStatUpgradeRule rule)
            || statUpgradeCounts[(int)statType] <= 0)
        {
            return false;
        }

        statUpgradeCounts[(int)statType]--;
        availableStatPoints += rule.StatPointCost;
        ApplyStatModifiers();
        return true;
    }

    public bool ResetStatPoints()
    {
        int refundedPoints = CalculateStatPointRefund();
        if (refundedPoints <= 0)
        {
            return false;
        }

        for (int i = 0; i < statUpgradeCounts.Count; i++)
        {
            statUpgradeCounts[i] = 0;
        }

        availableStatPoints += refundedPoints;
        ApplyStatModifiers();
        return true;
    }

    public bool TryUpgradeSkill(CombatSkill skill)
    {
        if (!skillProgress.TryGetUpgrade(
                skill,
                owner,
                out int nextLevel,
                out int pointCost)
            || availableStatPoints < pointCost)
        {
            return false;
        }

        availableStatPoints -= pointCost;
        skillProgress.SetLevel(skill, nextLevel);
        owner?.Combat.SetEquippedSkillLevel(skill, nextLevel - 1);
        return true;
    }

    public void AddLevelStatPoints(int gainedLevels)
    {
        if (gainedLevels > 0)
        {
            availableStatPoints += gainedLevels;
        }
    }

    public void CopyTo(CharacterStatProgressSaveData output)
    {
        if (output == null)
        {
            return;
        }

        EnsureUpgradeCounts();
        output.version = 1;
        output.availableStatPoints = AvailableStatPoints;
        output.statUpgradeCounts.Clear();
        output.statUpgradeCounts.AddRange(statUpgradeCounts);
        skillProgress.CopyTo(output.skillLevels);
    }

    public void Restore(
        CharacterStatProgressSaveData savedData,
        int currentLevel)
    {
        EnsureUpgradeCounts();
        if (savedData == null || savedData.version <= 0)
        {
            RestoreLegacyProgress(currentLevel);
            return;
        }

        availableStatPoints = Mathf.Max(0, savedData.availableStatPoints);
        RestoreUpgradeCounts(savedData.statUpgradeCounts);
        skillProgress.Restore(savedData.skillLevels);
        ApplyStatModifiers();
    }

    private void ApplyStatModifiers()
    {
        if (owner?.Stats == null)
        {
            return;
        }

        EnsureBuffers();
        modifierBuffer.Clear();
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            AddModifierForStat((StatType)i, statUpgradeCounts[i]);
        }

        owner.Stats.ReplaceModifiersFromSources(sourceBuffer, modifierBuffer);
    }

    private void AddModifierForStat(StatType statType, int upgradeCount)
    {
        if (upgradeCount <= 0
            || !TryGetRule(statType, out CharacterStatUpgradeRule rule))
        {
            return;
        }

        modifierBuffer.Add(new StatModifier(
            this,
            statType,
            rule.ValuePerUpgrade * upgradeCount));
    }

    private int CalculateStatPointRefund()
    {
        int refund = 0;
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            if (TryGetRule((StatType)i, out CharacterStatUpgradeRule rule))
            {
                refund += Mathf.Max(0, statUpgradeCounts[i]) * rule.StatPointCost;
            }
        }
        return refund;
    }

    private bool TryGetRule(
        StatType statType,
        out CharacterStatUpgradeRule rule)
    {
        EnsureUpgradeCounts();
        rule = default;
        return progressData != null
            && progressData.TryGetRule(statType, out rule);
    }

    private void RestoreLegacyProgress(int currentLevel)
    {
        availableStatPoints = Mathf.Max(0, currentLevel - 1);
        RestoreUpgradeCounts(null);
        skillProgress.Restore(null);
        ApplyStatModifiers();
    }

    private void RestoreUpgradeCounts(IReadOnlyList<int> savedCounts)
    {
        for (int i = 0; i < statUpgradeCounts.Count; i++)
        {
            statUpgradeCounts[i] = savedCounts != null && i < savedCounts.Count
                ? Mathf.Max(0, savedCounts[i])
                : 0;
        }
    }

    private void EnsureUpgradeCounts()
    {
        statUpgradeCounts ??= new List<int>();
        while (statUpgradeCounts.Count < StatTypeUtility.StatCount)
        {
            statUpgradeCounts.Add(0);
        }
        if (statUpgradeCounts.Count > StatTypeUtility.StatCount)
        {
            statUpgradeCounts.RemoveRange(
                StatTypeUtility.StatCount,
                statUpgradeCounts.Count - StatTypeUtility.StatCount);
        }
    }

    private void EnsureBuffers()
    {
        modifierBuffer ??= new List<StatModifier>();
        sourceBuffer ??= new List<IStatModifierSource> { this };
        if (sourceBuffer.Count == 0)
        {
            sourceBuffer.Add(this);
        }
    }
}
