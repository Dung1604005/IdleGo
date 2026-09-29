using System.Collections.Generic;

internal static partial class EquipmentBalanceRules
{
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

    public static StatType GetRecommendedMain(EquipmentDataSO equipment)
    {
        switch (equipment.EquipmentType)
        {
            case EquipmentType.MAIN_WEAPON:
                return StatType.DAMAGE;
            case EquipmentType.OFF_HAND_WEAPON:
                return GetOffHandMain(equipment.CharacterRequirement);
            case EquipmentType.BODY_ARMOR:
            case EquipmentType.COAT:
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
            case EquipmentType.COAT:
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
}
