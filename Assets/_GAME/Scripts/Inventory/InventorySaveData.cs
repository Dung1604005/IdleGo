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
    public List<EquippedEquipmentSaveData> equippedEquipments =
        new List<EquippedEquipmentSaveData>();

    // Field cu chi dung de migrate save khi equipment con nam trong InventorySlot.
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
public class ItemInstanceSaveData
{
    public string itemId;
    public string instanceId;
    public float qualityRoll = 1f;
    public List<BuffStatGroupSaveData> buffStats = new List<BuffStatGroupSaveData>();

    // Ba field cu chi dung de migrate PlayerPrefs da luu truoc khi co BuffStatType.
    public List<StatValueSaveData> socketStats = new List<StatValueSaveData>();
    public List<StatValueSaveData> enchantmentStats = new List<StatValueSaveData>();
    public List<StatValueSaveData> decorationStats = new List<StatValueSaveData>();
}

[Serializable]
public class InventorySlotSaveData : ItemInstanceSaveData
{
    public int slotIndex;
    public int amount;
}

[Serializable]
public class EquippedEquipmentSaveData : ItemInstanceSaveData
{
}

[Serializable]
public class BuffStatGroupSaveData
{
    public List<BuffStatSlotSaveData> slots = new List<BuffStatSlotSaveData>();

    // Field cu chi dung de doc save da tao truoc khi slot co co isEmpty.
    public List<StatValueSaveData> stats = new List<StatValueSaveData>();
}

[Serializable]
public class BuffStatSlotSaveData
{
    public bool isEmpty = true;
    public StatValueSaveData stat;
    public BuffStatRaritySaveData sourceRarity;

    public BuffStatSlotSaveData()
    {
    }

    public BuffStatSlotSaveData(StatValue value)
    {
        isEmpty = value == null;
        stat = value != null ? new StatValueSaveData(value) : null;
        if (value is BuffStatValue buffStat)
        {
            sourceRarity = new BuffStatRaritySaveData(buffStat.SourceRarity);
        }
    }

    public StatValue ToStatValue()
    {
        if (isEmpty || stat == null)
        {
            return null;
        }

        StatValue value = stat.ToStatValue();
        return sourceRarity != null
            ? new BuffStatValue(value, sourceRarity.rarity)
            : value;
    }
}

[Serializable]
public class BuffStatRaritySaveData
{
    public RarityType rarity;

    public BuffStatRaritySaveData()
    {
    }

    public BuffStatRaritySaveData(RarityType sourceRarity)
    {
        rarity = sourceRarity;
    }
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

    public bool IsLegacyEmptySlot()
    {
        return statType == default && value == 0f && operation == default;
    }
}
