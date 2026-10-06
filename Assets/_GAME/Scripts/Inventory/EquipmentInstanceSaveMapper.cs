using System.Collections.Generic;
using UnityEngine;

public static class EquipmentInstanceSaveMapper
{
    public static bool TryCreate(
        Equipment equipment,
        out EquippedEquipmentSaveData saveData)
    {
        saveData = null;
        if (equipment?.Data == null
            || string.IsNullOrWhiteSpace(equipment.Data.ItemId))
        {
            Debug.LogError("Every equipped item needs a stable ItemSO ItemId.");
            return false;
        }

        saveData = new EquippedEquipmentSaveData
        {
            itemId = equipment.Data.ItemId,
            instanceId = equipment.InstanceId,
            qualityRoll = equipment.QualityRoll
        };
        InventoryBuffStatSaveMapper.Copy(equipment, saveData);
        return true;
    }

    public static Equipment Restore(
        EquippedEquipmentSaveData saveData,
        ItemDatabaseSO itemDatabase,
        HashSet<string> loadedInstanceIds,
        Inventory owner)
    {
        if (saveData == null || itemDatabase == null)
        {
            return null;
        }

        ItemSO itemData = itemDatabase.GetItem(saveData.itemId);
        if (Item.Create(itemData) is not Equipment equipment)
        {
            Debug.LogWarning($"Cannot restore equipped item id '{saveData.itemId}'.");
            return null;
        }

        equipment.RestoreInstanceId(saveData.instanceId);
        if (loadedInstanceIds != null
            && !loadedInstanceIds.Add(equipment.InstanceId))
        {
            Debug.LogWarning($"Duplicate equipment id '{equipment.InstanceId}' was ignored.");
            return null;
        }

        equipment.RestoreQualityRoll(saveData.qualityRoll);
        InventoryBuffStatSaveMapper.Restore(equipment, saveData);
        equipment.SetInventory(owner);
        return equipment;
    }
}
