using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterStat
{
    [SerializeField] private CharacterStatSO statBaseData;
    [SerializeField, StatList] private List<float> currentStats = new List<float>();
    [SerializeField] private int currentHealth;

    private Character character;
    [NonSerialized] private List<StatModifier> modifiers;
    [NonSerialized] private List<float> calculatedStats;
    [NonSerialized] private bool areCalculatedStatsDirty = true;

    public int CurrentMaxHealth => Mathf.RoundToInt(GetCurrentStat(StatType.MAX_HEALTH));
    public int CurrentHealth => currentHealth;
    public int CurrentLevel => Mathf.RoundToInt(GetCurrentStat(StatType.LEVEL));
    public int CurrentExperience => Mathf.RoundToInt(GetCurrentStat(StatType.EXPERIENCE));
    public bool IsDead => currentHealth <= 0;
    public IReadOnlyList<StatModifier> Modifiers
    {
        get
        {
            EnsureRuntimeCollections();
            return modifiers;
        }
    }

    public void OnInit(CharacterStatSO characterStatSO, Character owner)
    {
        statBaseData = characterStatSO;
        character = owner;
        ResetToBase(characterStatSO);
    }

    public float GetCurrentStat(StatType statType)
    {
        EnsureCurrentStats();
        if (!StatTypeUtility.IsValid(statType))
        {
            return 0f;
        }

        RecalculateStatsIfNeeded();
        return calculatedStats[(int)statType];
    }

    public void SetCurrentStat(StatType statType, float value)
    {
        EnsureCurrentStats();
        if (!StatTypeUtility.IsValid(statType))
        {
            return;
        }

        currentStats[(int)statType] = StatTypeUtility.NormalizeValue(statType, value);
        areCalculatedStatsDirty = true;
        if (statType == StatType.MAX_HEALTH)
        {
            currentHealth = Mathf.Min(currentHealth, CurrentMaxHealth);
        }
    }

    public void AddCurrentStat(StatType statType, float amount)
    {
        EnsureCurrentStats();
        if (!StatTypeUtility.IsValid(statType))
        {
            return;
        }

        SetCurrentStat(statType, currentStats[(int)statType] + amount);
    }

    public bool AddModifier(StatModifier modifier)
    {
        EnsureRuntimeCollections();
        if (!IsValidModifier(modifier))
        {
            return false;
        }

        modifiers.Add(modifier);
        OnModifiersChanged();
        return true;
    }

    public int RemoveModifiersFromSource(object source)
    {
        EnsureRuntimeCollections();
        if (source == null)
        {
            return 0;
        }

        int removedCount = RemoveModifiersFromSourcesInternal(new[] { source });
        if (removedCount > 0)
        {
            OnModifiersChanged();
        }

        return removedCount;
    }

    public void ReplaceModifiersFromSources(
        IReadOnlyList<object> sourcesToReplace,
        IReadOnlyList<StatModifier> replacementModifiers)
    {
        EnsureRuntimeCollections();
        RemoveModifiersFromSourcesInternal(sourcesToReplace);

        if (replacementModifiers != null)
        {
            for (int i = 0; i < replacementModifiers.Count; i++)
            {
                StatModifier modifier = replacementModifiers[i];
                if (IsValidModifier(modifier))
                {
                    modifiers.Add(modifier);
                }
            }
        }

        OnModifiersChanged();
    }

    public void ClearAllModifiers()
    {
        EnsureRuntimeCollections();
        if (modifiers.Count == 0)
        {
            return;
        }

        modifiers.Clear();
        OnModifiersChanged();
    }

    public void RestoreHealthToMax()
    {
        currentHealth = CurrentMaxHealth;
    }

    public void ResetToBase(CharacterStatSO characterStatSO)
    {
        if (characterStatSO == null)
        {
            Debug.LogError("CharacterStat needs CharacterStatSO before ResetToBase().");
            return;
        }

        EnsureCurrentStats();
        EnsureRuntimeCollections();
        modifiers.Clear();
        areCalculatedStatsDirty = true;
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            StatType statType = (StatType)i;
            SetCurrentStat(statType, characterStatSO.GetBaseStat(statType));
        }

        currentHealth = CurrentMaxHealth;
    }

    public int GetExpToNextLevel(int level)
    {
        return Mathf.RoundToInt(
            GameConfig.BASE_EXP * Mathf.Pow(level, GameConfig.POWER_EXP)
        );
    }

    public bool TakeDamage(int incomingDamage)
    {
        if (IsDead || incomingDamage <= 0)
        {
            return false;
        }

        if (CanDodge())
        {
            return false;
        }

        // Armor giảm damage theo giá trị cố định; một đòn đánh trúng luôn gây ít nhất 1 damage.
        character.ChangeAnim(GameConfig.ANIM_HURT);
        int effectiveDamage = Mathf.Max(1, incomingDamage - (int)GetCurrentStat(StatType.ARMOR));
        currentHealth = Mathf.Max(0, currentHealth - effectiveDamage);
        return IsDead;
    }

    public bool CanDodge()
    {
        return GetCurrentStat(StatType.DODGE_CHANCE) >= 1f || UnityEngine.Random.value < GetCurrentStat(StatType.DODGE_CHANCE);
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        currentHealth += Mathf.Min(CurrentMaxHealth - currentHealth, amount);
    }

    public void AddExperience(int amount)
    {
        if (amount > 0)
        {
            AddCurrentStat(StatType.EXPERIENCE, amount);
        }
    }

    public void CheckLevelUp()
    {
        for (int i = 1; i <= 100000; i++)
        {
            int requiredExperience = GetExpToNextLevel(CurrentLevel);
            if (CurrentExperience < requiredExperience)
            {
                return;
            }
            SetCurrentStat(StatType.EXPERIENCE,CurrentExperience - requiredExperience);
            SetCurrentStat(StatType.LEVEL, CurrentLevel + 1);

        }
    }

    private void EnsureCurrentStats()
    {
        if (currentStats == null)
        {
            currentStats = new List<float>();
        }

        StatTypeUtility.EnsureListSize(currentStats);
    }

    private void EnsureRuntimeCollections()
    {
        if (modifiers == null)
        {
            modifiers = new List<StatModifier>();
        }

        if (calculatedStats == null)
        {
            calculatedStats = new List<float>();
            areCalculatedStatsDirty = true;
        }

        StatTypeUtility.EnsureListSize(calculatedStats);
    }

    private void RecalculateStatsIfNeeded()
    {
        EnsureRuntimeCollections();
        if (!areCalculatedStatsDirty)
        {
            return;
        }

        for (int statIndex = 0; statIndex < StatTypeUtility.StatCount; statIndex++)
        {
            StatType statType = (StatType)statIndex;
            float flatValue = 0f;
            float additivePercent = 0f;
            float multiplicativePercent = 1f;

            for (int modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
            {
                StatModifier modifier = modifiers[modifierIndex];
                if (modifier.StatType != statType)
                {
                    continue;
                }

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

            float calculatedValue = (currentStats[statIndex] + flatValue)
                * (1f + additivePercent)
                * multiplicativePercent;
            calculatedStats[statIndex] = StatTypeUtility.NormalizeValue(statType, calculatedValue);
        }

        areCalculatedStatsDirty = false;
    }

    private int RemoveModifiersFromSourcesInternal(IReadOnlyList<object> sources)
    {
        if (sources == null || sources.Count == 0)
        {
            return 0;
        }

        int removedCount = 0;
        for (int modifierIndex = modifiers.Count - 1; modifierIndex >= 0; modifierIndex--)
        {
            object modifierSource = modifiers[modifierIndex].Source;
            for (int sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
            {
                if (!ReferenceEquals(modifierSource, sources[sourceIndex]))
                {
                    continue;
                }

                modifiers.RemoveAt(modifierIndex);
                removedCount++;
                break;
            }
        }

        return removedCount;
    }

    private void OnModifiersChanged()
    {
        areCalculatedStatsDirty = true;
        currentHealth = Mathf.Min(currentHealth, CurrentMaxHealth);
    }

    private static bool IsValidModifier(StatModifier modifier)
    {
        return modifier != null
            && modifier.Source != null
            && StatTypeUtility.CanHaveModifiers(modifier.StatType)
            && Enum.IsDefined(typeof(StatModifierOperation), modifier.Operation);
    }
}
