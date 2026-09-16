using System;
using UnityEngine;

[Serializable]
public class CharacterStat
{
    [SerializeField] private CharacterStatSO statBaseData;
   public int CurrentMaxHealth => currentMaxHealth;

    public int GetExpToNextLevel(int level)
    {
        return Mathf.RoundToInt(
            GameConfig.BASE_EXP * Mathf.Pow(level, GameConfig.POWER_EXP)
        );
    }
    [SerializeField]private int currentHealth;

    public int CurrentHealth => currentHealth;
    public float CurrentRunSpeed => currentRunSpeed;
    public int CurrentDamage => currentDamage;
    public int CurrentLevel => currentLevel;
    public float CurrentAttackSpeed => currentAttackSpeed;
    public float CurrentCriticalChance => currentCriticalChance;
    public float CurrentCriticalDamage => currentCriticalDamage;
    public float CurrentCooldownReduction => currentCooldownReduction;
    public int CurrentArmor => currentArmor;
    public float CurrentLifeSteal => currentLifeSteal;
    public float CurrentDodgeChance => currentDodgeChance;
    public float CurrentDamageAmplification => currentDamageAmplification;
    public bool IsDead => currentHealth <= 0;

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
        currentHealth = Mathf.Min(currentHealth, currentMaxHealth);
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

    public void OnInit(CharacterStatSO characterStatSO)
    {
        statBaseData = characterStatSO;
        ResetToBase(characterStatSO);
    }

    public void ResetToBase(CharacterStatSO characterStatSO)
    {
        SetCurrentMaxHealth(characterStatSO.BaseMaxHealth);
        currentHealth = CurrentMaxHealth;
        SetCurrentRunSpeed(characterStatSO.BaseRunSpeed);
        SetCurrentDamage(characterStatSO.BaseDamage);
        SetCurrentLevel(characterStatSO.BaseLevel);
        SetCurrentExperience(characterStatSO.BaseExperience);
        SetCurrentAttackSpeed(characterStatSO.BaseAttackSpeed);
        SetCurrentCriticalChance(characterStatSO.BaseCriticalChance);
        SetCurrentCriticalDamage(characterStatSO.BaseCriticalDamage);
        SetCurrentCooldownReduction(characterStatSO.BaseCooldownReduction);
        SetCurrentArmor(characterStatSO.BaseArmor);
        SetCurrentLifeSteal(characterStatSO.BaseLifeSteal);
        SetCurrentDodgeChance(characterStatSO.BaseDodgeChance);
        SetCurrentDamageAmplification(characterStatSO.BaseDamageAmplification);
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
        currentHealth = Mathf.Max(0, currentHealth - effectiveDamage);
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

        currentHealth += Mathf.Min(CurrentMaxHealth - currentHealth, amount);
    }


    public void AddExperience(int amount)
    {
        if (amount > 0)
        {
            SetCurrentExperience(currentExperience + amount);
        }
    }

    public void CheckLevelUp()
    {
        for(int i = 1; i <= 100000; i++)
        {
            if(currentExperience >= GetExpToNextLevel(currentLevel))
            {
                SetCurrentExperience(currentExperience - GetExpToNextLevel(currentLevel));
                SetCurrentLevel(currentLevel + 1);

            }
        }
    }
}
