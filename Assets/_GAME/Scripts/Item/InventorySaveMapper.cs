using System.Collections.Generic;
using UnityEngine;

public static class InventorySaveMapper
{
    public static bool TryCreateSaveData(
        InventoryStorage storage,
        CharacterEquipment characterEquipment,
        out InventorySaveData saveData)
    {
        saveData = new InventorySaveData
        {
            capacity = storage.Capacity
        };

        IReadOnlyList<InventorySlot> slots = storage.Slots;
        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.IsEmpty)
            {
                continue;
            }

            if (!TryCreateSlotSaveData(slot, i, out InventorySlotSaveData slotSaveData))
            {
                saveData = null;
                return false;
            }

            saveData.slots.Add(slotSaveData);
        }

        AddEquippedItemIds(storage, characterEquipment, saveData.equippedItemInstanceIds);
        return true;
    }

    public static bool ApplySaveData(
        InventorySaveData saveData,
        ItemDatabaseSO itemDatabase,
        InventoryStorage storage,
        InventoryEquipmentHandler equipmentHandler,
        Inventory owner)
    {
        if (!CanApplySaveData(saveData, itemDatabase))
        {
            return false;
        }

        int loadedCapacity = Mathf.Max(1, saveData.capacity);
        List<InventorySlot> loadedSlots = InventoryStorage.CreateEmptySlots(loadedCapacity);
        HashSet<string> loadedInstanceIds = new HashSet<string>();

        if (saveData.slots != null)
        {
            RestoreSlots(saveData.slots, itemDatabase, loadedSlots, loadedInstanceIds);
        }

        storage.Replace(loadedCapacity, loadedSlots, owner);
        equipmentHandler.RestoreEquippedItems(saveData.equippedItemInstanceIds);
        return true;
    }

    private static bool TryCreateSlotSaveData(
        InventorySlot slot,
        int slotIndex,
        out InventorySlotSaveData slotSaveData)
    {
        slotSaveData = null;
        Item item = slot.Item;
        if (item.Data == null || string.IsNullOrWhiteSpace(item.Data.ItemId))
        {
            Debug.LogError("Every saved item needs a stable ItemSO ItemId.");
            return false;
        }

        slotSaveData = new InventorySlotSaveData
        {
            slotIndex = slotIndex,
            itemId = item.Data.ItemId,
            instanceId = item.InstanceId,
            amount = slot.Amount
        };

        if (item is Equipment equipment)
        {
            CopyStats(equipment.SocketStats, slotSaveData.socketStats);
            CopyStats(equipment.EnchantmentStats, slotSaveData.enchantmentStats);
            CopyStats(equipment.DecorationStats, slotSaveData.decorationStats);
        }

        return true;
    }

    private static void AddEquippedItemIds(
        InventoryStorage storage,
        CharacterEquipment characterEquipment,
        List<string> output)
    {
        if (characterEquipment == null)
        {
            return;
        }

        IReadOnlyList<Equipment> equippedItems = characterEquipment.EquippedItems;
        for (int i = 0; i < equippedItems.Count; i++)
        {
            Equipment equipment = equippedItems[i];
            if (equipment != null && storage.GetSlot(equipment) != null)
            {
                output.Add(equipment.InstanceId);
            }
        }
    }

    private static bool CanApplySaveData(InventorySaveData saveData, ItemDatabaseSO itemDatabase)
    {
        return saveData != null
            && (saveData.slots == null
                || saveData.slots.Count == 0
                || itemDatabase != null);
    }

    private static void RestoreSlots(
        IReadOnlyList<InventorySlotSaveData> savedSlots,
        ItemDatabaseSO itemDatabase,
        List<InventorySlot> loadedSlots,
        HashSet<string> loadedInstanceIds)
    {
        for (int i = 0; i < savedSlots.Count; i++)
        {
            InventorySlotSaveData slotSaveData = savedSlots[i];
            Item item = RestoreItem(slotSaveData, itemDatabase, loadedInstanceIds);
            if (item == null)
            {
                continue;
            }

            int slotIndex = InventoryStorage.GetRestoreSlotIndex(
                loadedSlots,
                slotSaveData.slotIndex
            );
            if (slotIndex < 0)
            {
                Debug.LogWarning("Inventory save contains more items than its capacity.");
                return;
            }

            loadedSlots[slotIndex].Restore(item, Mathf.Max(1, slotSaveData.amount));
        }
    }

    private static Item RestoreItem(
        InventorySlotSaveData slotSaveData,
        ItemDatabaseSO itemDatabase,
        HashSet<string> loadedInstanceIds)
    {
        // JSON chỉ giữ ID ổn định; ItemDatabase chịu trách nhiệm trả lại reference ItemSO.
        ItemSO itemData = itemDatabase.GetItem(slotSaveData.itemId);
        if (itemData == null)
        {
            Debug.LogWarning($"Cannot restore item id '{slotSaveData.itemId}'.");
            return null;
        }

        Item item = Item.Create(itemData);
        item.RestoreInstanceId(slotSaveData.instanceId);
        if (!loadedInstanceIds.Add(item.InstanceId))
        {
            Debug.LogWarning($"Duplicate inventory instance id '{item.InstanceId}' was ignored.");
            return null;
        }

        if (item is Equipment equipment)
        {
            equipment.RestoreEnhancementState(
                RestoreStats(slotSaveData.socketStats),
                RestoreStats(slotSaveData.enchantmentStats),
                RestoreStats(slotSaveData.decorationStats)
            );
        }

        return item;
    }

    private static void CopyStats(IReadOnlyList<StatValue> source, List<StatValueSaveData> target)
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

    private static List<StatValue> RestoreStats(IReadOnlyList<StatValueSaveData> savedStats)
    {
        List<StatValue> result = new List<StatValue>();
        if (savedStats == null)
        {
            return result;
        }

        for (int i = 0; i < savedStats.Count; i++)
        {
            result.Add(savedStats[i] != null ? savedStats[i].ToStatValue() : null);
        }

        return result;
    }
}
