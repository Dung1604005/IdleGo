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
            CopySlots(equipment.GetBuffStats((BuffStatType)i), group.slots);
            target.buffStats.Add(group);
        }
    }

    public static void Restore(Equipment equipment, InventorySlotSaveData source)
    {
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            BuffStatType buffType = (BuffStatType)i;
            BuffStatGroupSaveData group = GetSavedGroup(source, buffType);
            IReadOnlyList<StatValue> restoredStats = group?.slots != null
                && group.slots.Count > 0
                ? RestoreSlots(group.slots)
                : RestoreLegacyStats(GetLegacyStats(source, group, buffType));
            equipment.RestoreBuffStats(buffType, restoredStats);
        }
    }

    private static BuffStatGroupSaveData GetSavedGroup(
        InventorySlotSaveData source,
        BuffStatType buffStatType)
    {
        int index = (int)buffStatType;
        if (source.buffStats != null
            && index < source.buffStats.Count
            && source.buffStats[index] != null)
        {
            return source.buffStats[index];
        }
        return null;
    }

    private static IReadOnlyList<StatValueSaveData> GetLegacyStats(
        InventorySlotSaveData source,
        BuffStatGroupSaveData group,
        BuffStatType buffStatType)
    {
        if (group?.stats != null && group.stats.Count > 0)
        {
            return group.stats;
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

    private static void CopySlots(
        IReadOnlyList<StatValue> source,
        List<BuffStatSlotSaveData> target)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            // Khong luu null truc tiep trong List vi JsonUtility co the tao object mac dinh.
            target.Add(new BuffStatSlotSaveData(source[i]));
        }
    }

    private static List<StatValue> RestoreSlots(
        IReadOnlyList<BuffStatSlotSaveData> savedSlots)
    {
        List<StatValue> result = new List<StatValue>();
        for (int i = 0; i < savedSlots.Count; i++)
        {
            result.Add(savedSlots[i]?.ToStatValue());
        }
        return result;
    }

    private static List<StatValue> RestoreLegacyStats(
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
            bool isEmpty = stat == null || stat.IsLegacyEmptySlot();
            result.Add(isEmpty ? null : stat.ToStatValue());
        }
        return result;
    }
}
