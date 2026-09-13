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

    public int CurrentMaxHealth
    {
        get => currentMaxHealth;
        set
        {
            currentMaxHealth = Mathf.Max(1, value);
            CurrentHealth = Mathf.Min(CurrentHealth, currentMaxHealth);
        }
    }

    public int CurrentHealth { get; private set; }
    public float CurrentRunSpeed { get => currentRunSpeed; set => currentRunSpeed = Mathf.Max(0f, value); }
    public int CurrentDamage { get => currentDamage; set => currentDamage = Mathf.Max(0, value); }
    public int CurrentLevel { get => currentLevel; set => currentLevel = Mathf.Max(1, value); }
    public int CurrentExperience { get => currentExperience; set => currentExperience = Mathf.Max(0, value); }
    public float CurrentAttackSpeed { get => currentAttackSpeed; set => currentAttackSpeed = Mathf.Max(0.01f, value); }
    public float CurrentCriticalChance { get => currentCriticalChance; set => currentCriticalChance = Mathf.Clamp01(value); }
    public float CurrentCriticalDamage { get => currentCriticalDamage; set => currentCriticalDamage = Mathf.Max(1f, value); }
    public float CurrentCooldownReduction { get => currentCooldownReduction; set => currentCooldownReduction = Mathf.Clamp01(value); }
    public int CurrentArmor { get => currentArmor; set => currentArmor = Mathf.Max(0, value); }
    public bool IsDead => CurrentHealth <= 0;

    private int currentMaxHealth;
    private float currentRunSpeed;
    private int currentDamage;
    private int currentLevel;
    private int currentExperience;
    private float currentAttackSpeed;
    private float currentCriticalChance;
    private float currentCriticalDamage;
    private float currentCooldownReduction;
    private int currentArmor;

    public void OnInit()
    {
        ResetToBase();
    }

    public void ResetToBase()
    {
        CurrentMaxHealth = BaseMaxHealth;
        CurrentHealth = CurrentMaxHealth;
        CurrentRunSpeed = BaseRunSpeed;
        CurrentDamage = BaseDamage;
        CurrentLevel = BaseLevel;
        CurrentExperience = BaseExperience;
        CurrentAttackSpeed = BaseAttackSpeed;
        CurrentCriticalChance = BaseCriticalChance;
        CurrentCriticalDamage = BaseCriticalDamage;
        CurrentCooldownReduction = BaseCooldownReduction;
        CurrentArmor = BaseArmor;
    }

    public bool TakeDamage(int incomingDamage)
    {
        if (IsDead || incomingDamage <= 0)
        {
            return false;
        }

        // Armor is flat reduction; each successful hit still deals at least 1 damage.
        int effectiveDamage = Mathf.Max(1, incomingDamage - Mathf.Max(0, CurrentArmor));
        CurrentHealth = Mathf.Max(0, CurrentHealth - effectiveDamage);
        return IsDead;
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
        CurrentLevel = Mathf.Max(1, value);
    }

    public void AddExperience(int amount)
    {
        if (amount > 0)
        {
            CurrentExperience = Mathf.Max(0, CurrentExperience);
            CurrentExperience += Mathf.Min(amount, int.MaxValue - CurrentExperience);
        }
    }
}
