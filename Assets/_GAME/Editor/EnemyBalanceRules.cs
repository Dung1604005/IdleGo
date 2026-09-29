using UnityEngine;

internal readonly struct EnemyTypeBalance
{
    public readonly float Health;
    public readonly float Damage;
    public readonly float Armor;
    public readonly float Experience;
    public readonly float RunSpeed;
    public readonly float AttackSpeed;
    public readonly float CriticalChance;
    public readonly float CriticalDamage;
    public readonly float DodgeChance;

    public EnemyTypeBalance(
        float health,
        float damage,
        float armor,
        float experience,
        float runSpeed,
        float attackSpeed,
        float criticalChance,
        float criticalDamage,
        float dodgeChance)
    {
        Health = health;
        Damage = damage;
        Armor = armor;
        Experience = experience;
        RunSpeed = runSpeed;
        AttackSpeed = attackSpeed;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;
        DodgeChance = dodgeChance;
    }
}

internal static class EnemyBalanceRules
{
    public const float MinAdjustment = 0.5f;
    public const float MaxAdjustment = 2f;
    public const float BaseHealth = 25f;
    public const float BaseDamage = 5f;
    public const float BaseExperience = 10f;
    public const float EquipmentTierGrowth = 1.84f;
    public const int LevelsPerTier = 5;

    public static float GetProgressionMultiplier(int level)
    {
        int normalizedLevel = Mathf.Max(1, level);
        int tier = normalizedLevel / LevelsPerTier;
        int tierStartLevel = tier == 0 ? 1 : tier * LevelsPerTier;
        int levelInsideTier = normalizedLevel - tierStartLevel;
        return Mathf.Pow(EquipmentTierGrowth, tier)
            * Mathf.Pow(1.05f, levelInsideTier);
    }

    public static float GetBaseArmor(int level)
    {
        // Armor tang theo muc giam damage muc tieu, tranh raw Armor tang vo han.
        float reduction = Mathf.Min(0.45f, (Mathf.Max(1, level) - 1) * 0.005f);
        return reduction <= 0f ? 0f : 100f * reduction / (1f - reduction);
    }

    public static EnemyTypeBalance GetTypeBalance(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.ELITE => new EnemyTypeBalance(
                4f, 1.55f, 1.5f, 5f, 1.05f, 1.05f, 0.08f, 1.6f, 0.02f),
            EnemyType.BOSS => new EnemyTypeBalance(
                15f, 2.2f, 2f, 20f, 0.9f, 0.9f, 0.1f, 1.75f, 0.05f),
            _ => new EnemyTypeBalance(
                1f, 1f, 1f, 1f, 1f, 1f, 0.05f, 1.5f, 0f)
        };
    }

    public static bool IsPercentage(StatType statType)
    {
        return statType == StatType.CRITICAL_CHANCE
            || statType == StatType.COOLDOWN_REDUCTION
            || statType == StatType.LIFE_STEAL
            || statType == StatType.DODGE_CHANCE
            || statType == StatType.DAMAGE_AMPLIFICATION;
    }
}
