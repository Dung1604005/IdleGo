using System;
using UnityEngine;

[Serializable]
public class CharacterStat
{
    [SerializeField, Min(1)] private int baseMaxHealth = 100;
    [SerializeField, Min(0f)] private float baseRunSpeed = 5f;
    [SerializeField, Min(0)] private int baseDamage = 10;
    [SerializeField, Min(1)] private int baseLevel = 1;
    [SerializeField, Min(0)] private int baseExperience = 0;
    [SerializeField, Min(0.01f)] private float baseAttackSpeed = 1f;
    [SerializeField, Range(0f, 1f)] private float baseCriticalChance = 0.05f;
    [SerializeField, Min(1f)] private float baseCriticalDamage = 1.5f;
    [SerializeField, Range(0f, 1f)] private float baseCooldownReduction = 0f;
    [SerializeField, Min(0)] private int baseArmor = 0;
    [SerializeField, Range(0f, 1f)] private float baseLifeSteal = 0f;
    [SerializeField, Range(0f, 1f)] private float baseDodgeChance = 0f;
    [SerializeField, Min(0f)] private float baseDamageAmplification = 0f;

    public int BaseMaxHealth => Mathf.Max(1, baseMaxHealth);
    public float BaseRunSpeed => Mathf.Max(0f, baseRunSpeed);
    public int BaseDamage => Mathf.Max(0, baseDamage);
    public int BaseLevel => Mathf.Max(1, baseLevel);
    public int BaseExperience => Mathf.Max(0, baseExperience);
    public float BaseAttackSpeed => Mathf.Max(0.01f, baseAttackSpeed);
    public float BaseCriticalChance => Mathf.Clamp01(baseCriticalChance);
    public float BaseCriticalDamage => Mathf.Max(1f, baseCriticalDamage);
    public float BaseCooldownReduction => Mathf.Clamp01(baseCooldownReduction);
    public int BaseArmor => Mathf.Max(0, baseArmor);
    public float BaseLifeSteal => Mathf.Clamp01(baseLifeSteal);
    public float BaseDodgeChance => Mathf.Clamp01(baseDodgeChance);
    public float BaseDamageAmplification => Mathf.Max(0f, baseDamageAmplification);

    public int CurrentMaxHealth => currentMaxHealth;

    public int GetExpToNextLevel(int level)
    {
        return Mathf.RoundToInt(
            GameConfig.BASE_EXP * Mathf.Pow(level, GameConfig.POWER_EXP)
        );
    }

    public int CurrentHealth { get; private set; }
    public float CurrentRunSpeed => currentRunSpeed;
    public int CurrentDamage => currentDamage;
    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public float CurrentAttackSpeed => currentAttackSpeed;
    public float CurrentCriticalChance => currentCriticalChance;
    public float CurrentCriticalDamage => currentCriticalDamage;
    public float CurrentCooldownReduction => currentCooldownReduction;
    public int CurrentArmor => currentArmor;
    public float CurrentLifeSteal => currentLifeSteal;
    public float CurrentDodgeChance => currentDodgeChance;
    public float CurrentDamageAmplification => currentDamageAmplification;
    public bool IsDead => CurrentHealth <= 0;

    [SerializeField] private int currentMaxHealth;
    [SerializeField] private float currentRunSpeed;
    [SerializeField] private int currentDamage;
    [SerializeField] private int currentLevel;
    [SerializeField] private int currentExperience;
    [SerializeField] private float currentAttackSpeed;
    [SerializeField] private float currentCriticalChance;
    [SerializeField] private float currentCriticalDamage;
    [SerializeField] private float currentCooldownReduction;
    [SerializeField] private int currentArmor;
    [SerializeField] private float currentLifeSteal;
    [SerializeField] private float currentDodgeChance;
    [SerializeField] private float currentDamageAmplification;

    public void SetCurrentMaxHealth(int value)
    {
        currentMaxHealth = Mathf.Max(1, value);
        CurrentHealth = Mathf.Min(CurrentHealth, currentMaxHealth);
    }

    public void SetCurrentRunSpeed(float value)
    {
        currentRunSpeed = Mathf.Max(0f, value);
    }

    public void SetCurrentDamage(int value)
    {
        currentDamage = Mathf.Max(0, value);
    }

    public void SetCurrentLevel(int value)
    {
        currentLevel = Mathf.Max(1, value);
    }

    public void SetCurrentExperience(int value)
    {
        currentExperience = Mathf.Max(0, value);
    }

    public void SetCurrentAttackSpeed(float value)
    {
        currentAttackSpeed = Mathf.Max(0.01f, value);
    }

    public void SetCurrentCriticalChance(float value)
    {
        currentCriticalChance = Mathf.Clamp01(value);
    }

    public void SetCurrentCriticalDamage(float value)
    {
        currentCriticalDamage = Mathf.Max(1f, value);
    }

    public void SetCurrentCooldownReduction(float value)
    {
        currentCooldownReduction = Mathf.Clamp01(value);
    }

    public void SetCurrentArmor(int value)
    {
        currentArmor = Mathf.Max(0, value);
    }

    public void SetCurrentLifeSteal(float value)
    {
        currentLifeSteal = Mathf.Clamp01(value);
    }

    public void SetCurrentDodgeChance(float value)
    {
        currentDodgeChance = Mathf.Clamp01(value);
    }

    public void SetCurrentDamageAmplification(float value)
    {
        currentDamageAmplification = Mathf.Max(0f, value);
    }

    public void OnInit()
    {
        ResetToBase();
    }

    public void ResetToBase()
    {
        SetCurrentMaxHealth(BaseMaxHealth);
        CurrentHealth = CurrentMaxHealth;
        SetCurrentRunSpeed(BaseRunSpeed);
        SetCurrentDamage(BaseDamage);
        SetCurrentLevel(BaseLevel);
        SetCurrentExperience(BaseExperience);
        SetCurrentAttackSpeed(BaseAttackSpeed);
        SetCurrentCriticalChance(BaseCriticalChance);
        SetCurrentCriticalDamage(BaseCriticalDamage);
        SetCurrentCooldownReduction(BaseCooldownReduction);
        SetCurrentArmor(BaseArmor);
        SetCurrentLifeSteal(BaseLifeSteal);
        SetCurrentDodgeChance(BaseDodgeChance);
        SetCurrentDamageAmplification(BaseDamageAmplification);
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
        // Armor is flat reduction; each successful hit still deals at least 1 damage.
        int effectiveDamage = Mathf.Max(1, incomingDamage - Mathf.Max(0, CurrentArmor));
        CurrentHealth = Mathf.Max(0, CurrentHealth - effectiveDamage);
        return IsDead;
    }

    public bool CanDodge()
    {
        return CurrentDodgeChance >= 1f || UnityEngine.Random.value < CurrentDodgeChance;
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        CurrentHealth += Mathf.Min(CurrentMaxHealth - CurrentHealth, amount);
    }

    public void SetLevel(int value)
    {
        SetCurrentLevel(value);
    }

    public void AddExperience(int amount)
    {
        if (amount > 0)
        {
            SetCurrentExperience(CurrentExperience);
            SetCurrentExperience(CurrentExperience + Mathf.Min(amount, int.MaxValue - CurrentExperience));
        }
    }

    public void CheckLevelUp()
    {
        for(int i = 1; i <= 100000; i++)
        {
            
        }
    }
}
