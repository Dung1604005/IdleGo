using UnityEngine;

internal static partial class EquipmentBalanceRules
{
    public const float BaseItemPower = 10f;
    public const int LevelsPerEquipmentTier = 5;
    public const float EquipmentTierGrowth = 1.84f;
    public const float MinRoll = 0.90f;
    public const float MaxRoll = 1.10f;

    public static int GetEquipmentTier(int level)
    {
        return Mathf.Max(0, level / LevelsPerEquipmentTier);
    }

    public static float GetLevelPower(int level)
    {
        int equipmentTier = GetEquipmentTier(level);
        return BaseItemPower * Mathf.Pow(EquipmentTierGrowth, equipmentTier);
    }

    public static float GetRarityMultiplier(RarityType rarity)
    {
        float[] values =
        {
            1f,
            1.50f,
            2.25f,
            3.375f,
            5.90625f,
            11.8125f,
            26.578125f,
            66.4453125f
        };
        int index = Mathf.Clamp((int)rarity, 0, values.Length - 1);
        return values[index];
    }

    public static float GetSlotMultiplier(EquipmentType type)
    {
        float[] values = { 1.30f, 0.95f, 1.20f, 1f, 1f, 0.85f, 0.80f, 0.90f };
        int index = Mathf.Clamp((int)type, 0, values.Length - 1);
        return values[index];
    }

    public static float GetBaseBudgetShare(RarityType rarity)
    {
        float[] values = { 1f, 1f, 0.90f, 0.85f, 0.80f, 0.75f, 0.70f, 0.65f };
        int index = Mathf.Clamp((int)rarity, 0, values.Length - 1);
        return values[index];
    }

    public static float GetMainBudgetShare(RarityType rarity)
    {
        float[] values = { 1f, 0.80f, 0.70f, 0.68f, 0.60f, 0.58f, 0.55f, 0.50f };
        int index = Mathf.Clamp((int)rarity, 0, values.Length - 1);
        return values[index];
    }

    public static int GetRequiredSubStatCount(RarityType rarity)
    {
        int[] values = { 0, 1, 2, 2, 3, 3, 4, 4 };
        int index = Mathf.Clamp((int)rarity, 0, values.Length - 1);
        return values[index];
    }

    public static StatModifierOperation GetOperation(StatType statType)
    {
        if (statType == StatType.RUN_SPEED || statType == StatType.ATTACK_SPEED)
        {
            return StatModifierOperation.PERCENT_ADD;
        }

        return StatModifierOperation.FLAT;
    }

    public static float ConvertPowerToValue(
        StatType statType,
        float power,
        float levelPower,
        CharacterRequirementType requirement)
    {
        switch (statType)
        {
            case StatType.DAMAGE:
                return Mathf.Round(power * GetDamageMultiplier(requirement));
            case StatType.MAX_HEALTH:
                return Mathf.Round(power * 10f);
            case StatType.ARMOR:
                return Round(power / 2f, 1);
            default:
                return ConvertNormalizedStat(statType, power, levelPower);
        }
    }

    public static float GetDamageMultiplier(CharacterRequirementType requirement)
    {
        switch (requirement)
        {
            case CharacterRequirementType.RANGER:
                return 0.80f;
            case CharacterRequirementType.MELEE:
                return 1f;
            case CharacterRequirementType.MAGE:
                return 1.25f;
            default:
                return 1f;
        }
    }

    private static float ConvertNormalizedStat(
        StatType statType,
        float power,
        float levelPower)
    {
        float normalizedBudget = levelPower > 0f ? power / levelPower : 0f;
        float costPerPercent = GetNormalizedCost(statType);
        float percentagePoints = normalizedBudget / costPerPercent;
        return Round(percentagePoints / 100f, 4);
    }

    private static float GetNormalizedCost(StatType statType)
    {
        switch (statType)
        {
            case StatType.CRITICAL_CHANCE: return 0.040f;
            case StatType.CRITICAL_DAMAGE: return 0.015f;
            case StatType.ATTACK_SPEED: return 0.025f;
            case StatType.RUN_SPEED: return 0.030f;
            case StatType.COOLDOWN_REDUCTION: return 0.050f;
            case StatType.LIFE_STEAL: return 0.080f;
            case StatType.DODGE_CHANCE: return 0.060f;
            case StatType.DAMAGE_AMPLIFICATION: return 0.040f;
            default: return 1f;
        }
    }

    private static float Round(float value, int digits)
    {
        float multiplier = Mathf.Pow(10f, digits);
        return Mathf.Round(value * multiplier) / multiplier;
    }
}
