using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    [SerializeReference] private Item item;
    [SerializeField, Min(0)] private int amount;

    public Item Item => item;
    public int Amount => amount;
    public bool IsEmpty => item == null || amount <= 0;

    public bool CanStack(Item otherItem)
    {
        if (IsEmpty || otherItem == null || !item.IsStackable || !otherItem.IsStackable)
        {
            return false;
        }

        return item.Data == otherItem.Data
            || item.Data.ItemId == otherItem.Data.ItemId;
    }

    internal int AddItem(Item newItem, int addAmount)
    {
        if (newItem == null || addAmount <= 0)
        {
            return 0;
        }

        if (IsEmpty)
        {
            item = newItem;
            amount = 0;
        }
        else if (!CanStack(newItem))
        {
            return 0;
        }

        int maxAmount = item.IsStackable ? item.MaxQuantity : 1;
        int addedAmount = Mathf.Min(addAmount, maxAmount - amount);
        amount += addedAmount;
        return addedAmount;
    }

    internal int RemoveItem(int removeAmount)
    {
        if (IsEmpty || removeAmount <= 0)
        {
            return 0;
        }

        int removedAmount = Mathf.Min(removeAmount, amount);
        amount -= removedAmount;
        if (amount <= 0)
        {
            Clear();
        }

        return removedAmount;
    }

    internal void Restore(Item savedItem, int savedAmount)
    {
        Clear();
        AddItem(savedItem, savedAmount);
    }

    internal void Clear()
    {
        item = null;
        amount = 0;
    }
}
