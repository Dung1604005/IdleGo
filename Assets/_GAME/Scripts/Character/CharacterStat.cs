using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterStat
{
    [SerializeField] private CharacterStatSO statBaseData;
    [SerializeField, StatList] private List<float> currentStats = new List<float>();
    [SerializeField] private int currentHealth;

    [NonSerialized] private Character character;
    [NonSerialized] private CharacterStatCalculator calculator;
    [NonSerialized] private CharacterLevelProgression levelProgression;
    [NonSerialized] private CharacterHealthController healthController;

    public int CurrentMaxHealth => Mathf.RoundToInt(
        GetCurrentStat(StatType.MAX_HEALTH));
    public int CurrentHealth => currentHealth;
    public int CurrentLevel => Mathf.RoundToInt(GetCurrentStat(StatType.LEVEL));
    public int CurrentExperience => Mathf.RoundToInt(
        GetCurrentStat(StatType.EXPERIENCE));
    public bool IsDead => currentHealth <= 0;
    public IReadOnlyList<StatModifier> Modifiers => Calculator.Modifiers;

    private CharacterStatCalculator Calculator =>
        calculator ??= new CharacterStatCalculator();
    private CharacterLevelProgression LevelProgression =>
        levelProgression ??= new CharacterLevelProgression(this);
    private CharacterHealthController HealthController =>
        healthController ??= new CharacterHealthController();

    public void OnInit(CharacterStatSO characterStatSO, Character owner)
    {
        statBaseData = characterStatSO;
        character = owner;
        ResetToBase(characterStatSO);
    }

    public float GetCurrentStat(StatType statType)
    {
        EnsureCurrentStats();
        return StatTypeUtility.IsValid(statType)
            ? Calculator.GetValue(currentStats, statType)
            : 0f;
    }

    public void SetCurrentStat(StatType statType, float value)
    {
        EnsureCurrentStats();
        if (!StatTypeUtility.IsValid(statType))
        {
            return;
        }

        currentStats[(int)statType] =
            StatTypeUtility.NormalizeValue(statType, value);
        Calculator.MarkDirty();
        if (statType == StatType.MAX_HEALTH)
        {
            ClampCurrentHealth();
        }
    }

    public void AddCurrentStat(StatType statType, float amount)
    {
        EnsureCurrentStats();
        if (StatTypeUtility.IsValid(statType))
        {
            SetCurrentStat(statType, currentStats[(int)statType] + amount);
        }
    }

    public void CopyProgressTo(List<float> output)
    {
        if (output == null)
        {
            return;
        }

        EnsureCurrentStats();
        output.Clear();
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            // Save chi giu stat goc; modifier se duoc gan lai tu equipment.
            output.Add(currentStats[i]);
        }
    }

    public void RestoreProgress(IReadOnlyList<float> savedStats)
    {
        if (savedStats == null)
        {
            return;
        }

        EnsureCurrentStats();
        int count = Mathf.Min(savedStats.Count, StatTypeUtility.StatCount);
        for (int i = 0; i < count; i++)
        {
            currentStats[i] = StatTypeUtility.NormalizeValue(
                (StatType)i,
                savedStats[i]);
        }

        Calculator.MarkDirty();
        RestoreHealthToMax();
    }

    public bool AddModifier(StatModifier modifier)
    {
        if (!Calculator.AddModifier(modifier))
        {
            return false;
        }

        ClampCurrentHealth();
        return true;
    }

    public int RemoveModifiersFromSource(IStatModifierSource source)
    {
        int removedCount = Calculator.RemoveModifiersFromSource(source);
        if (removedCount > 0)
        {
            ClampCurrentHealth();
        }
        return removedCount;
    }

    public void ReplaceModifiersFromSources(
        IReadOnlyList<IStatModifierSource> sourcesToReplace,
        IReadOnlyList<StatModifier> replacementModifiers)
    {
        Calculator.ReplaceModifiers(
            sourcesToReplace,
            replacementModifiers);
        ClampCurrentHealth();
    }

    public void ClearAllModifiers()
    {
        if (Calculator.ClearModifiers())
        {
            ClampCurrentHealth();
        }
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

        statBaseData = characterStatSO;
        EnsureCurrentStats();
        Calculator.ClearModifiers();
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            StatType type = (StatType)i;
            currentStats[i] = StatTypeUtility.NormalizeValue(
                type,
                characterStatSO.GetBaseStat(type));
        }

        Calculator.MarkDirty();
        RestoreHealthToMax();
    }

    public int GetExpToNextLevel(int level)
    {
        return LevelProgression.GetExpToNextLevel(level);
    }

    public void AddExperience(int amount)
    {
        LevelProgression.AddExperience(amount);
    }

    public void CheckLevelUp()
    {
        LevelProgression.CheckLevelUp();
    }

    public bool TakeDamage(int incomingDamage)
    {
        return HealthController.TakeDamage(
            this,
            character,
            ref currentHealth,
            incomingDamage);
    }

    public bool CanDodge()
    {
        return HealthController.CanDodge(this);
    }

    public void Heal(int amount)
    {
        HealthController.Heal(this, ref currentHealth, amount);
    }

    private void EnsureCurrentStats()
    {
        currentStats ??= new List<float>();
        StatTypeUtility.EnsureListSize(currentStats);
    }

    private void ClampCurrentHealth()
    {
        currentHealth = Mathf.Min(currentHealth, CurrentMaxHealth);
    }
}
