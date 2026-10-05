using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public partial class Equipment : Item, IStatModifierSource
{
    [SerializeField] private List<BuffStatGroup> buffStats =
        new List<BuffStatGroup>();

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
    public IReadOnlyList<BuffStatGroup> BuffStats => buffStats;

    public IReadOnlyList<StatValue> GetBuffStats(BuffStatType buffStatType)
    {
        EnsureBuffStatGroups();
        return BuffStatTypeUtility.IsValid(buffStatType)
            ? buffStats[(int)buffStatType].Stats
            : Array.Empty<StatValue>();
    }

    public bool SetBuffStat(
        BuffStatType buffStatType,
        int slotIndex,
        StatValue stat)
    {
        if (!BuffStatTypeUtility.IsValid(buffStatType))
        {
            return false;
        }
        EnsureRuntimeState();
        List<StatValue> stats = GetMutableBuffStats(buffStatType);
        int slotCount = Data != null ? Data.GetBuffSlotCount(buffStatType) : 0;
        return SetSlotStat(stats, slotCount, slotIndex, stat);
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
        float value = GetRolledBaseStatValue(statType, operation);
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            BuffStatType buffType = (BuffStatType)i;
            value += GetStatValue(
                buffStats[i].Stats,
                Data.GetBuffSlotCount(buffType),
                statType,
                operation);
        }
        return value;
    }

    public void CollectStatModifiers(List<StatModifier> output)
    {
        if (output == null || Data == null)
        {
            return;
        }

        EnsureRuntimeState();
        AddRolledBaseStatModifiers(output);
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            BuffStatType buffType = (BuffStatType)i;
            AddStatModifiers(
                output,
                buffStats[i].Stats,
                Data.GetBuffSlotCount(buffType));
        }
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
        EnsureBuffStatGroups();
        if (Data == null)
        {
            return;
        }
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            BuffStatType buffType = (BuffStatType)i;
            EnsureSlotListSize(
                buffStats[i].MutableStats,
                Data.GetBuffSlotCount(buffType));
        }
    }

    internal void RestoreBuffStats(
        BuffStatType buffStatType,
        IReadOnlyList<StatValue> savedStats)
    {
        if (!BuffStatTypeUtility.IsValid(buffStatType))
        {
            return;
        }

        EnsureBuffStatGroups();
        ReplaceStatList(GetMutableBuffStats(buffStatType), savedStats);
        if (Data != null)
        {
            EnsureSlotListSize(
                GetMutableBuffStats(buffStatType),
                Data.GetBuffSlotCount(buffStatType));
        }
    }

    private bool SetSlotStat(
        List<StatValue> stats,
        int slotCount,
        int slotIndex,
        StatValue stat)
    {
        if (slotIndex < 0 || slotIndex >= slotCount
            || (stat != null && !StatTypeUtility.CanHaveModifiers(stat.StatType)))
        {
            return false;
        }

        stats[slotIndex] = stat;
        // Data doi truoc, sau do character tinh lai stat va Inventory save roi refresh UI.
        equippedBy?.OnEquipmentDataChanged(this);
        NotifyInventoryDataChanged();
        return true;
    }

    private List<StatValue> GetMutableBuffStats(BuffStatType buffStatType)
    {
        return buffStats[(int)buffStatType].MutableStats;
    }

    private void EnsureBuffStatGroups()
    {
        buffStats ??= new List<BuffStatGroup>();
        while (buffStats.Count < BuffStatTypeUtility.Count)
        {
            buffStats.Add(new BuffStatGroup());
        }

        if (buffStats.Count > BuffStatTypeUtility.Count)
        {
            buffStats.RemoveRange(
                BuffStatTypeUtility.Count,
                buffStats.Count - BuffStatTypeUtility.Count);
        }

        for (int i = 0; i < buffStats.Count; i++)
        {
            buffStats[i] ??= new BuffStatGroup();
        }
    }

    private static float GetStatValue(
        IReadOnlyList<StatValue> stats,
        int slotCount,
        StatType statType,
        StatModifierOperation operation)
    {
        float totalValue = 0f;
        int count = Mathf.Min(stats.Count, Mathf.Max(0, slotCount));
        for (int i = 0; i < count; i++)
        {
            StatValue stat = stats[i];
            if (stat != null && stat.StatType == statType && stat.Operation == operation)
            {
                totalValue += stat.Value;
            }
        }
        return totalValue;
    }

    private void AddStatModifiers(
        List<StatModifier> output,
        IReadOnlyList<StatValue> stats,
        int activeSlotCount)
    {
        int count = Mathf.Min(stats.Count, Mathf.Max(0, activeSlotCount));
        for (int i = 0; i < count; i++)
        {
            StatValue stat = stats[i];
            if (stat != null && StatTypeUtility.CanHaveModifiers(stat.StatType))
            {
                output.Add(new StatModifier(
                    this, stat.StatType, stat.Value, stat.Operation));
            }
        }
    }

    private static void EnsureSlotListSize(List<StatValue> stats, int size)
    {
        while (stats.Count < size)
        {
            stats.Add(null);
        }

        if (stats.Count > size)
        {
            stats.RemoveRange(size, stats.Count - size);
        }
    }

    private static void ReplaceStatList(
        List<StatValue> target,
        IReadOnlyList<StatValue> source)
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
