using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Equipment : Item, IStatModifierSource
{
    [SerializeField] private List<StatValue> socketStats = new List<StatValue>();
    [SerializeField] private List<StatValue> enchantmentStats = new List<StatValue>();
    [SerializeField] private List<StatValue> decorationStats = new List<StatValue>();

    [NonSerialized] private CharacterEquipment equippedBy;

    public Equipment()
    {
    }

    public Equipment(EquipmentDataSO data) : base(data)
    {
        EnsureRuntimeState();
    }

    public new EquipmentDataSO Data => base.Data as EquipmentDataSO;
    public string ModifierSourceId => InstanceId;
    public EquipmentType EquipmentType => Data != null ? Data.EquipmentType : default;
    public CharacterEquipment EquippedBy => equippedBy;
    public bool IsEquipped => equippedBy != null;

    public IReadOnlyList<StatValue> SocketStats
    {
        get
        {
            return socketStats != null
                ? socketStats
                : (IReadOnlyList<StatValue>)Array.Empty<StatValue>();
        }
    }

    public IReadOnlyList<StatValue> EnchantmentStats
    {
        get
        {
            return enchantmentStats != null
                ? enchantmentStats
                : (IReadOnlyList<StatValue>)Array.Empty<StatValue>();
        }
    }

    public IReadOnlyList<StatValue> DecorationStats
    {
        get
        {
            return decorationStats != null
                ? decorationStats
                : (IReadOnlyList<StatValue>)Array.Empty<StatValue>();
        }
    }

    public bool SetSocketStat(int slotIndex, StatValue stat)
    {
        EnsureRuntimeState();
        return SetSlotStat(socketStats, Data != null ? Data.SocketSlotCount : 0, slotIndex, stat);
    }

    public bool SetEnchantmentStat(int slotIndex, StatValue stat)
    {
        EnsureRuntimeState();
        return SetSlotStat(enchantmentStats, Data != null ? Data.EnchantmentSlotCount : 0, slotIndex, stat);
    }

    public bool SetDecorationStat(int slotIndex, StatValue stat)
    {
        EnsureRuntimeState();
        return SetSlotStat(decorationStats, Data != null ? Data.DecorationSlotCount : 0, slotIndex, stat);
    }

    public float GetStatValue(
        StatType statType,
        StatModifierOperation operation = StatModifierOperation.FLAT)
    {
        if (Data == null || !StatTypeUtility.IsValid(statType))
        {
            return 0f;
        }

        EnsureRuntimeState();
        return Data.GetStatValue(statType, operation)
            + GetStatValue(socketStats, Data.SocketSlotCount, statType, operation)
            + GetStatValue(enchantmentStats, Data.EnchantmentSlotCount, statType, operation)
            + GetStatValue(decorationStats, Data.DecorationSlotCount, statType, operation);
    }

    public void CollectStatModifiers(List<StatModifier> output)
    {
        if (output == null || Data == null)
        {
            return;
        }

        EnsureRuntimeState();
        AddStatModifiers(output, Data.Stats, Data.Stats != null ? Data.Stats.Count : 0);
        AddStatModifiers(output, socketStats, Data.SocketSlotCount);
        AddStatModifiers(output, enchantmentStats, Data.EnchantmentSlotCount);
        AddStatModifiers(output, decorationStats, Data.DecorationSlotCount);
    }

    private bool SetSlotStat(List<StatValue> stats, int slotCount, int slotIndex, StatValue stat)
    {
        EnsureMinimumListSize(stats, slotCount);
        if (slotIndex < 0 || slotIndex >= slotCount)
        {
            return false;
        }

        if (stat != null && !StatTypeUtility.CanHaveModifiers(stat.StatType))
        {
            return false;
        }

        stats[slotIndex] = stat;
        // Khong dung event: equipment dang mac cap nhat lai stat truc tiep sau khi slot thay doi.
        equippedBy?.ApplyStats();
        NotifyInventoryDataChanged();
        return true;
    }

    internal bool CanBeEquippedBy(CharacterEquipment characterEquipment)
    {
        return characterEquipment != null
            && (equippedBy == null || ReferenceEquals(equippedBy, characterEquipment));
    }

    internal void SetEquippedBy(CharacterEquipment characterEquipment)
    {
        equippedBy = characterEquipment;
    }

    internal void EnsureRuntimeState()
    {
        EnsureStatLists();

        if (Data == null)
        {
            return;
        }

        EnsureMinimumListSize(socketStats, Data.SocketSlotCount);
        EnsureMinimumListSize(enchantmentStats, Data.EnchantmentSlotCount);
        EnsureMinimumListSize(decorationStats, Data.DecorationSlotCount);
    }

    private static float GetStatValue(
        IReadOnlyList<StatValue> stats,
        int slotCount,
        StatType statType,
        StatModifierOperation operation)
    {
        float totalValue = 0f;
        int activeSlotCount = Mathf.Min(stats.Count, Mathf.Max(0, slotCount));
        for (int i = 0; i < activeSlotCount; i++)
        {
            StatValue stat = stats[i];
            if (stat != null && stat.StatType == statType && stat.Operation == operation)
            {
                totalValue += stat.Value;
            }
        }

        return totalValue;
    }

    private void EnsureStatLists()
    {
        if (socketStats == null)
        {
            socketStats = new List<StatValue>();
        }

        if (enchantmentStats == null)
        {
            enchantmentStats = new List<StatValue>();
        }

        if (decorationStats == null)
        {
            decorationStats = new List<StatValue>();
        }

    }

    private void AddStatModifiers(
        List<StatModifier> output,
        IReadOnlyList<StatValue> stats,
        int activeSlotCount)
    {
        if (stats == null)
        {
            return;
        }

        int count = Mathf.Min(stats.Count, Mathf.Max(0, activeSlotCount));
        for (int i = 0; i < count; i++)
        {
            StatValue stat = stats[i];
            if (stat == null || !StatTypeUtility.CanHaveModifiers(stat.StatType))
            {
                continue;
            }

            output.Add(new StatModifier(this, stat.StatType, stat.Value, stat.Operation));
        }
    }

    private static void EnsureMinimumListSize(List<StatValue> stats, int size)
    {
        while (stats.Count < size)
        {
            stats.Add(null);
        }
    }

    internal void RestoreEnhancementState(
        IReadOnlyList<StatValue> savedSocketStats,
        IReadOnlyList<StatValue> savedEnchantmentStats,
        IReadOnlyList<StatValue> savedDecorationStats)
    {
        ReplaceStatList(socketStats, savedSocketStats);
        ReplaceStatList(enchantmentStats, savedEnchantmentStats);
        ReplaceStatList(decorationStats, savedDecorationStats);
        EnsureRuntimeState();
    }

    private static void ReplaceStatList(List<StatValue> target, IReadOnlyList<StatValue> source)
    {
        target.Clear();
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            StatValue stat = source[i];
            target.Add(stat == null
                ? null
                : new StatValue(stat.StatType, stat.Value, stat.Operation));
        }
    }
}
