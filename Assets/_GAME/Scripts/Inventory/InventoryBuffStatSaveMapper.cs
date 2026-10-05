using System.Collections.Generic;

public static class InventoryBuffStatSaveMapper
{
    public static void Copy(Equipment equipment, InventorySlotSaveData target)
    {
        target.buffStats ??= new List<BuffStatGroupSaveData>();
        target.buffStats.Clear();
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            BuffStatGroupSaveData group = new BuffStatGroupSaveData();
            CopyStats(equipment.GetBuffStats((BuffStatType)i), group.stats);
            target.buffStats.Add(group);
        }
    }

    public static void Restore(Equipment equipment, InventorySlotSaveData source)
    {
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            BuffStatType buffType = (BuffStatType)i;
            IReadOnlyList<StatValueSaveData> savedStats = GetSavedStats(source, buffType);
            equipment.RestoreBuffStats(buffType, RestoreStats(savedStats));
        }
    }

    private static IReadOnlyList<StatValueSaveData> GetSavedStats(
        InventorySlotSaveData source,
        BuffStatType buffStatType)
    {
        int index = (int)buffStatType;
        if (source.buffStats != null
            && index < source.buffStats.Count
            && source.buffStats[index] != null)
        {
            return source.buffStats[index].stats;
        }

        // Save cu duoc doc mot lan roi se duoc ghi lai theo list BuffStatType moi.
        switch (buffStatType)
        {
            case BuffStatType.SOCKET: return source.socketStats;
            case BuffStatType.ENCHANTMENT: return source.enchantmentStats;
            case BuffStatType.DECORATION: return source.decorationStats;
            default: return null;
        }
    }

    private static void CopyStats(
        IReadOnlyList<StatValue> source,
        List<StatValueSaveData> target)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            target.Add(source[i] == null ? null : new StatValueSaveData(source[i]));
        }
    }

    private static List<StatValue> RestoreStats(
        IReadOnlyList<StatValueSaveData> savedStats)
    {
        List<StatValue> result = new List<StatValue>();
        if (savedStats == null)
        {
            return result;
        }

        for (int i = 0; i < savedStats.Count; i++)
        {
            StatValueSaveData stat = savedStats[i];
            result.Add(stat != null ? stat.ToStatValue() : null);
        }
        return result;
    }
}
