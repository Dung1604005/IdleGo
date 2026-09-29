using System.Collections.Generic;
using UnityEngine;

internal static class EquipmentBalanceRules
{
    public const float BaseItemPower = 10f;
    public const float LevelGrowth = 1.05f;
    public const float MinRoll = 0.90f;
    public const float MaxRoll = 1.10f;
    public static readonly StatType[] AvailableStats =
    {
        StatType.MAX_HEALTH,
        StatType.RUN_SPEED,
        StatType.DAMAGE,
        StatType.ATTACK_SPEED,
        StatType.CRITICAL_CHANCE,
        StatType.CRITICAL_DAMAGE,
        StatType.COOLDOWN_REDUCTION,
        StatType.ARMOR,
        StatType.LIFE_STEAL,
        StatType.DODGE_CHANCE,
        StatType.DAMAGE_AMPLIFICATION
    };
    public static float GetLevelPower(int level)
    {
        return BaseItemPower * Mathf.Pow(LevelGrowth, Mathf.Max(0, level - 1));
    }

    public static float GetRarityMultiplier(RarityType rarity)
    {
        float[] values = { 1f, 1.25f, 1.56f, 1.95f, 2.44f, 3.05f, 3.81f, 4.77f };
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

    public static float ConvertPowerToValue(StatType statType, float power, float levelPower)
    {
        switch (statType)
        {
            case StatType.DAMAGE:
                return Mathf.Round(power);
            case StatType.MAX_HEALTH:
                return Mathf.Round(power * 10f);
            case StatType.ARMOR:
                return Round(power / 2f, 1);
            default:
                return ConvertNormalizedStat(statType, power, levelPower);
        }
    }

    public static StatType GetRecommendedMain(EquipmentDataSO equipment)
    {
        switch (equipment.EquipmentType)
        {
            case EquipmentType.MAIN_WEAPON:
                return StatType.DAMAGE;
            case EquipmentType.OFF_HAND_WEAPON:
                return GetOffHandMain(equipment.CharacterRequirement);
            case EquipmentType.BODY_ARMOR:
            case EquipmentType.LEG_ARMOR:
                return StatType.MAX_HEALTH;
            case EquipmentType.HEAD_ARMOR:
                return StatType.ARMOR;
            case EquipmentType.SHOES:
                return StatType.RUN_SPEED;
            case EquipmentType.RING:
                return StatType.CRITICAL_CHANCE;
            case EquipmentType.NECKLACE:
                return StatType.DAMAGE_AMPLIFICATION;
            default:
                return StatType.DAMAGE;
        }
    }

    public static List<StatType> GetRecommendedSubs(
        EquipmentDataSO equipment,
        StatType mainStat)
    {
        StatType[] candidates = GetSubCandidates(equipment);
        int requiredCount = GetRequiredSubStatCount(equipment.RarityType);
        List<StatType> result = new List<StatType>(requiredCount);
        for (int i = 0; i < candidates.Length && result.Count < requiredCount; i++)
        {
            if (candidates[i] != mainStat && !result.Contains(candidates[i]))
            {
                result.Add(candidates[i]);
            }
        }

        return result;
    }

    public static string GetDisplayName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MAX_HEALTH: return "Máu tối đa";
            case StatType.RUN_SPEED: return "Tốc độ chạy";
            case StatType.DAMAGE: return "Sát thương";
            case StatType.ATTACK_SPEED: return "Tốc độ đánh";
            case StatType.CRITICAL_CHANCE: return "Tỉ lệ chí mạng";
            case StatType.CRITICAL_DAMAGE: return "Sát thương chí mạng";
            case StatType.COOLDOWN_REDUCTION: return "Giảm hồi chiêu";
            case StatType.ARMOR: return "Giáp";
            case StatType.LIFE_STEAL: return "Hút máu";
            case StatType.DODGE_CHANCE: return "Tỉ lệ né";
            case StatType.DAMAGE_AMPLIFICATION: return "Khuếch đại sát thương";
            default: return statType.ToString();
        }
    }

    public static bool IsRatioStat(StatType statType)
    {
        return statType != StatType.DAMAGE
            && statType != StatType.MAX_HEALTH
            && statType != StatType.ARMOR;
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

    private static StatType GetOffHandMain(CharacterRequirementType requirement)
    {
        switch (requirement)
        {
            case CharacterRequirementType.MELEE:
                return StatType.ARMOR;
            case CharacterRequirementType.RANGER:
                return StatType.ATTACK_SPEED;
            case CharacterRequirementType.MAGE:
                return StatType.COOLDOWN_REDUCTION;
            default:
                return StatType.DAMAGE;
        }
    }

    private static StatType[] GetSubCandidates(EquipmentDataSO equipment)
    {
        switch (equipment.EquipmentType)
        {
            case EquipmentType.MAIN_WEAPON:
                return new[] { StatType.CRITICAL_CHANCE, StatType.CRITICAL_DAMAGE,
                    StatType.ATTACK_SPEED, StatType.DAMAGE_AMPLIFICATION, StatType.LIFE_STEAL };
            case EquipmentType.OFF_HAND_WEAPON:
                return GetOffHandCandidates(equipment.CharacterRequirement);
            case EquipmentType.BODY_ARMOR:
                return new[] { StatType.ARMOR, StatType.DODGE_CHANCE,
                    StatType.LIFE_STEAL, StatType.RUN_SPEED, StatType.MAX_HEALTH };
            case EquipmentType.HEAD_ARMOR:
                return new[] { StatType.MAX_HEALTH, StatType.COOLDOWN_REDUCTION,
                    StatType.DAMAGE_AMPLIFICATION, StatType.CRITICAL_CHANCE };
            case EquipmentType.LEG_ARMOR:
                return new[] { StatType.ARMOR, StatType.DODGE_CHANCE,
                    StatType.RUN_SPEED, StatType.LIFE_STEAL };
            case EquipmentType.SHOES:
                return new[] { StatType.DODGE_CHANCE, StatType.ATTACK_SPEED,
                    StatType.MAX_HEALTH, StatType.ARMOR };
            case EquipmentType.RING:
                return new[] { StatType.CRITICAL_DAMAGE, StatType.DAMAGE,
                    StatType.LIFE_STEAL, StatType.ATTACK_SPEED };
            default:
                return new[] { StatType.COOLDOWN_REDUCTION, StatType.CRITICAL_DAMAGE,
                    StatType.LIFE_STEAL, StatType.MAX_HEALTH };
        }
    }

    private static StatType[] GetOffHandCandidates(CharacterRequirementType requirement)
    {
        switch (requirement)
        {
            case CharacterRequirementType.MELEE:
                return new[] { StatType.MAX_HEALTH, StatType.DAMAGE,
                    StatType.LIFE_STEAL, StatType.DODGE_CHANCE };
            case CharacterRequirementType.RANGER:
                return new[] { StatType.DAMAGE, StatType.CRITICAL_CHANCE,
                    StatType.CRITICAL_DAMAGE, StatType.DODGE_CHANCE };
            case CharacterRequirementType.MAGE:
                return new[] { StatType.DAMAGE_AMPLIFICATION, StatType.DAMAGE,
                    StatType.CRITICAL_DAMAGE, StatType.MAX_HEALTH };
            default:
                return new[] { StatType.MAX_HEALTH, StatType.ARMOR,
                    StatType.COOLDOWN_REDUCTION, StatType.DODGE_CHANCE,
                    StatType.DAMAGE };
        }
    }

    private static float Round(float value, int digits)
    {
        float multiplier = Mathf.Pow(10f, digits);
        return Mathf.Round(value * multiplier) / multiplier;
    }
}
