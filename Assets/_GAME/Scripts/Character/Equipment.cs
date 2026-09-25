using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Equipment : IStatModifierSource
{
    [SerializeField] private string instanceId;
    [SerializeField] private EquipmentDataSO data;
    [SerializeField] private List<StatValue> socketStats = new List<StatValue>();
    [SerializeField] private List<StatValue> enchantmentStats = new List<StatValue>();
    [SerializeField] private List<StatValue> decorationStats = new List<StatValue>();

    [field: NonSerialized] public event Action Changed;
    [NonSerialized] private CharacterEquipment equippedBy;

    public Equipment()
    {
    }

    public Equipment(EquipmentDataSO data)
    {
        this.data = data;
        EnsureRuntimeState();
    }

    public string InstanceId
    {
        get
        {
            EnsureInstanceId();
            return instanceId;
        }
    }

    public EquipmentDataSO Data => data;
    public string ModifierSourceId => InstanceId;
    public EquipmentType EquipmentType => data != null ? data.EquipmentType : default;
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
        return SetSlotStat(socketStats, data != null ? data.SocketSlotCount : 0, slotIndex, stat);
    }

    public bool SetEnchantmentStat(int slotIndex, StatValue stat)
    {
        EnsureRuntimeState();
        return SetSlotStat(enchantmentStats, data != null ? data.EnchantmentSlotCount : 0, slotIndex, stat);
    }

    public bool SetDecorationStat(int slotIndex, StatValue stat)
    {
        EnsureRuntimeState();
        return SetSlotStat(decorationStats, data != null ? data.DecorationSlotCount : 0, slotIndex, stat);
    }

    public float GetStatValue(
        StatType statType,
        StatModifierOperation operation = StatModifierOperation.FLAT)
    {
        if (data == null || !StatTypeUtility.IsValid(statType))
        {
            return 0f;
        }

        EnsureRuntimeState();
        return data.GetStatValue(statType, operation)
            + GetStatValue(socketStats, data.SocketSlotCount, statType, operation)
            + GetStatValue(enchantmentStats, data.EnchantmentSlotCount, statType, operation)
            + GetStatValue(decorationStats, data.DecorationSlotCount, statType, operation);
    }

    public void CollectStatModifiers(List<StatModifier> output)
    {
        if (output == null || data == null)
        {
            return;
        }

        EnsureRuntimeState();
        AddStatModifiers(output, data.Stats, data.Stats != null ? data.Stats.Count : 0);
        AddStatModifiers(output, socketStats, data.SocketSlotCount);
        AddStatModifiers(output, enchantmentStats, data.EnchantmentSlotCount);
        AddStatModifiers(output, decorationStats, data.DecorationSlotCount);
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
        Changed?.Invoke();
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
        EnsureInstanceId();
        EnsureStatLists();

        if (data == null)
        {
            return;
        }

        EnsureMinimumListSize(socketStats, data.SocketSlotCount);
        EnsureMinimumListSize(enchantmentStats, data.EnchantmentSlotCount);
        EnsureMinimumListSize(decorationStats, data.DecorationSlotCount);
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

    private void EnsureInstanceId()
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            instanceId = Guid.NewGuid().ToString("N");
        }
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
}
