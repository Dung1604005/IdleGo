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
            AppendLine(builder, $"Số đòn mỗi giây: {mainStats.AttacksPerSecond:0.##}");
        }
        return builder.ToString();
    }

    public static string BuildSubStats(Equipment equipment)
    {
        IReadOnlyList<StatValue> stats = equipment?.GetSubStats();
        if (stats == null || stats.Count == 0)
        {
            return "Không có chỉ số phụ";
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
        AppendSlots(builder, "Khảm", equipment.Data.SocketSlotCount, equipment.SocketStats);
        AppendSlots(builder, "Phù phép", equipment.Data.EnchantmentSlotCount,
            equipment.EnchantmentStats);
        AppendSlots(builder, "Trang trí", equipment.Data.DecorationSlotCount,
            equipment.DecorationStats);
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
            string value = stat != null ? FormatStat(stat) : "Trống";
            AppendLine(builder, $"{label} {i + 1}: {value}");
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

    private static string FormatStat(StatValue stat)
    {
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
            case StatType.MAX_HEALTH: return "Máu tối đa";
            case StatType.RUN_SPEED: return "Tốc độ chạy";
            case StatType.DAMAGE: return "Sát thương";
            case StatType.ATTACK_SPEED: return "Tốc độ tấn công";
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
}
