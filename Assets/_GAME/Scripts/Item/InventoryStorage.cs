using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryStorage
{
    [SerializeField, Min(1)] private int capacity = 30;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    public IReadOnlyList<InventorySlot> Slots => slots;
    public int Capacity => capacity;
    internal List<InventorySlot> MutableSlots => slots;

    public void OnInit(Inventory owner)
    {
        EnsureSlotCount();
        SetItemOwner(owner);
    }

    public void OnDespawn()
    {
        SetItemOwner(null);
    }

    public int CountItem(ItemSO itemData)
    {
        if (itemData == null)
        {
            return 0;
        }

        int totalAmount = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            if (HasSameData(slot.Item, itemData))
            {
                totalAmount += slot.Amount;
            }
        }

        return totalAmount;
    }

    public InventorySlot GetSlot(Item item)
    {
        if (item == null)
        {
            return null;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            Item slotItem = slots[i].Item;
            if (ReferenceEquals(slotItem, item)
                || (slotItem != null && slotItem.InstanceId == item.InstanceId))
            {
                return slots[i];
            }
        }

        return null;
    }

    public Item GetItemByInstanceId(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return null;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            Item item = slots[i].Item;
            if (item != null && item.InstanceId == instanceId)
            {
                return item;
            }
        }

        return null;
    }

    public InventorySlot GetFirstEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                return slots[i];
            }
        }

        return null;
    }

    public void SortByRarityDescending()
    {
        slots.Sort(CompareSlotsByRarity);
    }

    internal void Replace(int newCapacity, List<InventorySlot> newSlots, Inventory owner)
    {
        // Bỏ liên kết ở item cũ trước khi thay toàn bộ dữ liệu bằng kết quả load.
        SetItemOwner(null);
        capacity = Mathf.Max(1, newCapacity);
        slots = newSlots ?? CreateEmptySlots(capacity);
        EnsureSlotCount();
        SetItemOwner(owner);
    }

    internal static List<InventorySlot> CreateEmptySlots(int slotCount)
    {
        List<InventorySlot> result = new List<InventorySlot>(slotCount);
        for (int i = 0; i < slotCount; i++)
        {
            result.Add(new InventorySlot());
        }
        return result;
    }

    internal static int GetRestoreSlotIndex(IReadOnlyList<InventorySlot> targetSlots, int preferredIndex)
    {
        if (preferredIndex >= 0
            && preferredIndex < targetSlots.Count
            && targetSlots[preferredIndex].IsEmpty)
        {
            return preferredIndex;
        }

        for (int i = 0; i < targetSlots.Count; i++)
        {
            if (targetSlots[i].IsEmpty)
            {
                return i;
            }
        }

        return -1;
    }

    internal static bool HasSameData(Item item, ItemSO itemData)
    {
        return item != null
            && item.Data != null
            && (item.Data == itemData || item.Data.ItemId == itemData.ItemId);
    }

    private void EnsureSlotCount()
    {
        capacity = Mathf.Max(1, capacity);
        slots ??= new List<InventorySlot>();

        while (slots.Count < capacity)
        {
            slots.Add(new InventorySlot());
        }

        if (slots.Count > capacity)
        {
            slots.RemoveRange(capacity, slots.Count - capacity);
        }

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i] ??= new InventorySlot();
        }
    }

    private void SetItemOwner(Inventory owner)
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i]?.Item?.SetInventory(owner);
        }
    }

    private static int CompareSlotsByRarity(InventorySlot first, InventorySlot second)
    {
        bool firstEmpty = first == null || first.IsEmpty;
        bool secondEmpty = second == null || second.IsEmpty;
        if (firstEmpty || secondEmpty)
        {
            return firstEmpty == secondEmpty ? 0 : firstEmpty ? 1 : -1;
        }

        int rarityCompare = ((int)second.Item.RarityType).CompareTo((int)first.Item.RarityType);
        if (rarityCompare != 0)
        {
            return rarityCompare;
        }

        return string.Compare(
            first.Item.Data.NameItem,
            second.Item.Data.NameItem,
            StringComparison.Ordinal
        );
    }
}
