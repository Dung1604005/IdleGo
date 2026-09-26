using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemHandler
{
    private readonly InventoryStorage storage;
    private readonly InventoryEquipmentHandler equipmentHandler;

    public InventoryItemHandler(
        InventoryStorage inventoryStorage,
        InventoryEquipmentHandler inventoryEquipmentHandler)
    {
        storage = inventoryStorage;
        equipmentHandler = inventoryEquipmentHandler;
    }

    public bool AddItem(Item item, int amount, Inventory owner)
    {
        // Kiểm tra toàn bộ sức chứa trước để không tạo trạng thái thêm được một phần rồi thất bại.
        if (!CanAddItem(item, amount))
        {
            return false;
        }

        int remainingAmount = item.IsStackable
            ? FillExistingStacks(item, amount)
            : amount;
        bool usedInputInstance = false;

        List<InventorySlot> slots = storage.MutableSlots;
        for (int i = 0; i < slots.Count && remainingAmount > 0; i++)
        {
            InventorySlot slot = slots[i];
            if (!slot.IsEmpty)
            {
                continue;
            }

            Item itemForSlot = usedInputInstance ? Item.Create(item.Data) : item;
            int addedAmount = slot.AddItem(itemForSlot, remainingAmount);
            if (addedAmount <= 0)
            {
                continue;
            }

            usedInputInstance = true;
            itemForSlot.SetInventory(owner);
            remainingAmount -= addedAmount;
        }

        return remainingAmount == 0;
    }

    public bool RemoveItem(Item item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        InventorySlot slot = storage.GetSlot(item);
        if (slot == null || slot.Amount < amount)
        {
            return false;
        }

        bool removeWholeSlot = amount >= slot.Amount;
        RemoveFromSlot(slot, amount, removeWholeSlot);
        return true;
    }

    public bool RemoveItem(ItemSO itemData, int amount)
    {
        if (itemData == null || amount <= 0 || storage.CountItem(itemData) < amount)
        {
            return false;
        }

        int remainingAmount = amount;
        List<InventorySlot> slots = storage.MutableSlots;
        for (int i = slots.Count - 1; i >= 0 && remainingAmount > 0; i--)
        {
            InventorySlot slot = slots[i];
            if (!InventoryStorage.HasSameData(slot.Item, itemData))
            {
                continue;
            }

            int removeAmount = Mathf.Min(remainingAmount, slot.Amount);
            RemoveFromSlot(slot, removeAmount, removeAmount >= slot.Amount);
            remainingAmount -= removeAmount;
        }

        return remainingAmount == 0;
    }

    private bool CanAddItem(Item item, int amount)
    {
        if (item == null
            || item.Data == null
            || string.IsNullOrWhiteSpace(item.Data.ItemId)
            || amount <= 0
            || storage.GetSlot(item) != null)
        {
            return false;
        }

        if (item.ItemType == ItemType.EQUIPMENT && item is not Equipment)
        {
            return false;
        }

        long availableAmount = 0;
        int amountPerEmptySlot = item.IsStackable ? item.MaxQuantity : 1;
        IReadOnlyList<InventorySlot> slots = storage.Slots;
        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.IsEmpty)
            {
                availableAmount += amountPerEmptySlot;
            }
            else if (item.IsStackable && slot.CanStack(item))
            {
                availableAmount += item.MaxQuantity - slot.Amount;
            }
        }

        return availableAmount >= amount;
    }

    private int FillExistingStacks(Item item, int remainingAmount)
    {
        IReadOnlyList<InventorySlot> slots = storage.Slots;
        for (int i = 0; i < slots.Count && remainingAmount > 0; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.CanStack(item))
            {
                remainingAmount -= slot.AddItem(item, remainingAmount);
            }
        }

        return remainingAmount;
    }

    private void RemoveFromSlot(InventorySlot slot, int amount, bool removeWholeSlot)
    {
        Item removedItem = slot.Item;
        if (removeWholeSlot)
        {
            // Trang bị phải được tháo trước khi mất khỏi inventory để modifier không còn tồn tại.
            equipmentHandler.UnequipBeforeRemoving(removedItem);
            removedItem.SetInventory(null);
        }

        slot.RemoveItem(amount);
    }
}
