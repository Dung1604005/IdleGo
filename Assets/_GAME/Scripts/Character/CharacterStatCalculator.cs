using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CharacterStatCalculator
{
    private readonly List<StatModifier> modifiers = new List<StatModifier>();
    private readonly List<float> calculatedStats = new List<float>();
    private bool isDirty = true;

    public IReadOnlyList<StatModifier> Modifiers => modifiers;

    public float GetValue(IReadOnlyList<float> baseStats, StatType statType)
    {
        if (baseStats == null || !StatTypeUtility.IsValid(statType))
        {
            return 0f;
        }

        RecalculateIfNeeded(baseStats);
        return calculatedStats[(int)statType];
    }

    public bool AddModifier(StatModifier modifier)
    {
        if (!IsValidModifier(modifier))
        {
            return false;
        }

        modifiers.Add(modifier);
        MarkDirty();
        return true;
    }

    public int RemoveModifiersFromSource(IStatModifierSource source)
    {
        if (source == null)
        {
            return 0;
        }

        int removedCount = RemoveModifiersFromSources(new[] { source });
        if (removedCount > 0)
        {
            MarkDirty();
        }
        return removedCount;
    }

    public void ReplaceModifiers(
        IReadOnlyList<IStatModifierSource> sourcesToReplace,
        IReadOnlyList<StatModifier> replacements)
    {
        RemoveModifiersFromSources(sourcesToReplace);
        AddValidModifiers(replacements);
        MarkDirty();
    }

    public bool ClearModifiers()
    {
        if (modifiers.Count == 0)
        {
            return false;
        }

        modifiers.Clear();
        MarkDirty();
        return true;
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    private void RecalculateIfNeeded(IReadOnlyList<float> baseStats)
    {
        EnsureCalculatedStats();
        if (!isDirty)
        {
            return;
        }

        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            calculatedStats[i] = CalculateStat(baseStats, (StatType)i, i);
        }
        isDirty = false;
    }

    private float CalculateStat(
        IReadOnlyList<float> baseStats,
        StatType statType,
        int statIndex)
    {
        float flatValue = 0f;
        float additivePercent = 0f;
        float multiplicativePercent = 1f;
        AccumulateModifiers(
            statType,
            ref flatValue,
            ref additivePercent,
            ref multiplicativePercent);

        // Thu tu bat buoc: cong flat, nhan tong percent add, roi nhan tung percent multiply.
        float value = (baseStats[statIndex] + flatValue)
            * (1f + additivePercent)
            * multiplicativePercent;
        return StatTypeUtility.NormalizeValue(statType, value);
    }

    private void AccumulateModifiers(
        StatType statType,
        ref float flatValue,
        ref float additivePercent,
        ref float multiplicativePercent)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier modifier = modifiers[i];
            if (modifier.StatType != statType)
            {
                continue;
            }

            ApplyModifier(
                modifier,
                ref flatValue,
                ref additivePercent,
                ref multiplicativePercent);
        }
    }

    private static void ApplyModifier(
        StatModifier modifier,
        ref float flatValue,
        ref float additivePercent,
        ref float multiplicativePercent)
    {
        switch (modifier.Operation)
        {
            case StatModifierOperation.FLAT:
                flatValue += modifier.Value;
                break;
            case StatModifierOperation.PERCENT_ADD:
                additivePercent += modifier.Value;
                break;
            case StatModifierOperation.PERCENT_MULTIPLY:
                multiplicativePercent *= 1f + modifier.Value;
                break;
        }
    }

    private int RemoveModifiersFromSources(
        IReadOnlyList<IStatModifierSource> sources)
    {
        if (sources == null || sources.Count == 0)
        {
            return 0;
        }

        int removedCount = 0;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (ContainsSource(sources, modifiers[i].Source))
            {
                modifiers.RemoveAt(i);
                removedCount++;
            }
        }
        return removedCount;
    }

    private static bool ContainsSource(
        IReadOnlyList<IStatModifierSource> sources,
        IStatModifierSource target)
    {
        for (int i = 0; i < sources.Count; i++)
        {
            if (ReferenceEquals(target, sources[i]))
            {
                return true;
            }
        }
        return false;
    }

    private void AddValidModifiers(IReadOnlyList<StatModifier> replacements)
    {
        for (int i = 0; replacements != null && i < replacements.Count; i++)
        {
            if (IsValidModifier(replacements[i]))
            {
                modifiers.Add(replacements[i]);
            }
        }
    }

    private void EnsureCalculatedStats()
    {
        StatTypeUtility.EnsureListSize(calculatedStats);
    }

    private static bool IsValidModifier(StatModifier modifier)
    {
        return modifier != null
            && modifier.Source != null
            && StatTypeUtility.CanHaveModifiers(modifier.StatType)
            && Enum.IsDefined(typeof(StatModifierOperation), modifier.Operation);
    }
}
