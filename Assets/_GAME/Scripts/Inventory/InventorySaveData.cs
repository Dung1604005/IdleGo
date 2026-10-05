using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public int capacity;
    public int globalPlayerLevel = 1;
    public List<InventorySlotSaveData> slots = new List<InventorySlotSaveData>();
    public PlayerRosterSaveData playerRoster = new PlayerRosterSaveData();

    // Du lieu cu duoc giu lai de chuyen save theo team index sang save theo CharacterId.
    public List<PlayerEquipmentSaveData> playerEquipments = new List<PlayerEquipmentSaveData>();

    // Giu field cu de load save mot Player da tao truoc khi inventory chuyen len PlayerManager.
    public List<string> equippedItemInstanceIds = new List<string>();
}

[Serializable]
public class PlayerRosterSaveData
{
    public List<string> unlockedCharacterIds = new List<string>();
    public List<string> teamCharacterIds = new List<string>();
    public List<PlayerCharacterSaveData> characters = new List<PlayerCharacterSaveData>();
}

[Serializable]
public class PlayerCharacterSaveData
{
    public string characterId;
    public List<float> currentStats = new List<float>();
    public List<string> equippedItemInstanceIds = new List<string>();
    public List<EquippedSkillSaveData> equippedSkills = new List<EquippedSkillSaveData>();
}

[Serializable]
public class EquippedSkillSaveData
{
    public string skillId;
    public int level;
}

[Serializable]
public class PlayerEquipmentSaveData
{
    public int playerIndex;
    public List<string> equippedItemInstanceIds = new List<string>();
}

[Serializable]
public class InventorySlotSaveData
{
    public int slotIndex;
    public string itemId;
    public string instanceId;
    public int amount;
    public float qualityRoll = 1f;
    public List<BuffStatGroupSaveData> buffStats = new List<BuffStatGroupSaveData>();

    // Ba field cu chi dung de migrate PlayerPrefs da luu truoc khi co BuffStatType.
    public List<StatValueSaveData> socketStats = new List<StatValueSaveData>();
    public List<StatValueSaveData> enchantmentStats = new List<StatValueSaveData>();
    public List<StatValueSaveData> decorationStats = new List<StatValueSaveData>();
}

[Serializable]
public class BuffStatGroupSaveData
{
    public List<StatValueSaveData> stats = new List<StatValueSaveData>();
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
