using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public int capacity;
    public List<InventorySlotSaveData> slots = new List<InventorySlotSaveData>();
    public List<string> equippedItemInstanceIds = new List<string>();
}

[Serializable]
public class InventorySlotSaveData
{
    public int slotIndex;
    public string itemId;
    public string instanceId;
    public int amount;
    public List<StatValueSaveData> socketStats = new List<StatValueSaveData>();
    public List<StatValueSaveData> enchantmentStats = new List<StatValueSaveData>();
    public List<StatValueSaveData> decorationStats = new List<StatValueSaveData>();
}

[Serializable]
public class StatValueSaveData
{
    public StatType statType;
    public float value;
    public StatModifierOperation operation;

    public StatValueSaveData()
    {
    }

    public StatValueSaveData(StatValue stat)
    {
        statType = stat.StatType;
        value = stat.Value;
        operation = stat.Operation;
    }

    public StatValue ToStatValue()
    {
        return new StatValue(statType, value, operation);
    }
}
