using System.Collections.Generic;
using System.Text;

public static class EquipmentInformationText
{
    public static string BuildMainStats(Equipment equipment)
    {
        if (equipment?.Data == null)
        {
            return string.Empty;
        }

        EquipmentMainStats mainStats = equipment.GetMainStats();
        StringBuilder builder = new StringBuilder();
        AppendStat(builder, mainStats?.MainStat);
        if (mainStats != null && mainStats.HasAttacksPerSecond)
        {
            AppendLine(builder,
                $"Attacks Per Second: {mainStats.AttacksPerSecond:0.##}");
        }
        return builder.ToString();
    }

    public static string BuildSubStats(Equipment equipment)
    {
        IReadOnlyList<StatValue> stats = equipment?.GetSubStats();
        if (stats == null || stats.Count == 0)
        {
            return "No Substats";
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < stats.Count; i++)
        {
            AppendStat(builder, stats[i]);
        }
        return builder.ToString();
    }

    public static string BuildEnhancementSlots(Equipment equipment)
    {
        if (equipment?.Data == null)
        {
            return string.Empty;
        }

        equipment.EnsureRuntimeState();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            BuffStatType buffType = (BuffStatType)i;
            AppendSlots(
                builder,
                GetBuffStatName(buffType),
                equipment.Data.GetBuffSlotCount(buffType),
                equipment.GetBuffStats(buffType));
        }
        return builder.ToString();
    }

    public static string GetRarityName(RarityType rarity)
    {
        return rarity.ToString();
    }

    public static string GetRequirementName(CharacterRequirementType requirement)
    {
        switch (requirement)
        {
            case CharacterRequirementType.MELEE: return "Melee";
            case CharacterRequirementType.RANGER: return "Ranger";
            case CharacterRequirementType.MAGE: return "Mage";
            default: return string.Empty;
        }
    }

    private static void AppendSlots(
        StringBuilder builder,
        string label,
        int slotCount,
        IReadOnlyList<StatValue> stats)
    {
        for (int i = 0; i < slotCount; i++)
        {
            StatValue stat = stats != null && i < stats.Count ? stats[i] : null;
            string value = stat != null ? FormatStat(stat) : "Empty";
            AppendLine(builder, $"{label} {i + 1}: {value}");
        }
    }

    public static string GetBuffStatName(BuffStatType buffStatType)
    {
        switch (buffStatType)
        {
            case BuffStatType.SOCKET: return "Socket";
            case BuffStatType.ENCHANTMENT: return "Enchantment";
            case BuffStatType.DECORATION: return "Decoration";
            default: return buffStatType.ToString();
        }
    }

    private static void AppendStat(StringBuilder builder, StatValue stat)
    {
        if (stat != null)
        {
            AppendLine(builder, FormatStat(stat));
        }
    }

    private static void AppendLine(StringBuilder builder, string line)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }
        builder.Append(line);
    }

    public static string FormatStat(StatValue stat)
    {
        if (stat == null)
        {
            return string.Empty;
        }

        bool isPercent = stat.Operation != StatModifierOperation.FLAT
            || IsRatioStat(stat.StatType);
        float displayValue = isPercent ? stat.Value * 100f : stat.Value;
        string suffix = isPercent ? "%" : string.Empty;
        return $"{GetStatName(stat.StatType)} {displayValue:+0.##;-0.##;0}{suffix}";
    }

    private static bool IsRatioStat(StatType statType)
    {
        return statType == StatType.CRITICAL_CHANCE
            || statType == StatType.CRITICAL_DAMAGE
            || statType == StatType.COOLDOWN_REDUCTION
            || statType == StatType.LIFE_STEAL
            || statType == StatType.DODGE_CHANCE
            || statType == StatType.DAMAGE_AMPLIFICATION;
    }

    private static string GetStatName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MAX_HEALTH: return "Max Health";
            case StatType.RUN_SPEED: return "Run Speed";
            case StatType.DAMAGE: return "Damage";
            case StatType.ATTACK_SPEED: return "Attack Speed";
            case StatType.CRITICAL_CHANCE: return "Critical Chance";
            case StatType.CRITICAL_DAMAGE: return "Critical Damage";
            case StatType.COOLDOWN_REDUCTION: return "Cooldown Reduction";
            case StatType.ARMOR: return "Armor";
            case StatType.LIFE_STEAL: return "Life Steal";
            case StatType.DODGE_CHANCE: return "Dodge Chance";
            case StatType.DAMAGE_AMPLIFICATION: return "Damage Amplification";
            default: return statType.ToString();
        }
    }
}
