using System.Collections.Generic;

public partial class Equipment
{
    public bool HasEmptyBuffSlot(BuffStatType buffStatType)
    {
        return TryGetEmptyBuffSlot(buffStatType, out _);
    }

    internal bool TryApplyEnchantStat(
        BuffStatType buffStatType,
        StatValue stat,
        RarityType sourceRarity)
    {
        if (stat == null
            || !StatTypeUtility.CanHaveModifiers(stat.StatType)
            || !TryGetEmptyBuffSlot(buffStatType, out int slotIndex))
        {
            return false;
        }

        List<StatValue> stats = GetMutableBuffStats(buffStatType);
        stats[slotIndex] = new BuffStatValue(stat, sourceRarity);

        // Character tinh lai stat ngay; Inventory se save va refresh UI sau khi tru material.
        equippedBy?.OnEquipmentDataChanged(this);
        return true;
    }

    private bool TryGetEmptyBuffSlot(
        BuffStatType buffStatType,
        out int slotIndex)
    {
        slotIndex = -1;
        if (!BuffStatTypeUtility.IsValid(buffStatType) || Data == null)
        {
            return false;
        }

        EnsureRuntimeState();
        List<StatValue> stats = GetMutableBuffStats(buffStatType);
        int slotCount = Data.GetBuffSlotCount(buffStatType);
        // Luon lap day slot trong dau tien de thu tu UI va save on dinh.
        for (int i = 0; i < slotCount; i++)
        {
            if (stats[i] == null)
            {
                slotIndex = i;
                return true;
            }
        }
        return false;
    }

    private static StatValue CloneBuffStat(StatValue stat)
    {
        if (stat is BuffStatValue buffStat)
        {
            return new BuffStatValue(buffStat, buffStat.SourceRarity);
        }

        return stat == null
            ? null
            : new StatValue(stat.StatType, stat.Value, stat.Operation);
    }
}
