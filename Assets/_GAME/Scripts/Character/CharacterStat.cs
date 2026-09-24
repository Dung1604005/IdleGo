using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterStat
{
    [SerializeField] private CharacterStatSO statBaseData;
    [SerializeField] private List<float> currentStats = new List<float>();
    [SerializeField] private int currentHealth;

    private Character character;

    public int CurrentMaxHealth => Mathf.RoundToInt(GetCurrentStat(StatType.MAX_HEALTH));
    public int CurrentHealth => currentHealth;
    public int CurrentLevel => Mathf.RoundToInt(GetCurrentStat(StatType.LEVEL));
    public int CurrentExperience => Mathf.RoundToInt(GetCurrentStat(StatType.EXPERIENCE));
    public bool IsDead => currentHealth <= 0;

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

        return currentStats[(int)statType];
    }

    public void SetCurrentStat(StatType statType, float value)
    {
        EnsureCurrentStats();
        if (!StatTypeUtility.IsValid(statType))
        {
            return;
        }

        currentStats[(int)statType] = StatTypeUtility.NormalizeValue(statType, value);
        if (statType == StatType.MAX_HEALTH)
        {
            currentHealth = Mathf.Min(currentHealth, CurrentMaxHealth);
        }
    }

    public void AddCurrentStat(StatType statType, float amount)
    {
        SetCurrentStat(statType, GetCurrentStat(statType) + amount);
    }

    public void ResetToBase(CharacterStatSO characterStatSO)
    {
        if (characterStatSO == null)
        {
            Debug.LogError("CharacterStat needs CharacterStatSO before ResetToBase().");
            return;
        }

        EnsureCurrentStats();
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
            SetCurrentStat(StatType.EXPERIENCE,CurrentExperience + amount);
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
}
