using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "IdleGo/Item/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    [SerializeField] private List<ItemSO> items = new List<ItemSO>();

    public ItemSO GetItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || items == null)
        {
            return null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            ItemSO item = items[i];
            if (item != null && item.ItemId == itemId)
            {
                return item;
            }
        }

        return null;
    }

    public bool ContainsDuplicateId(string itemId, ItemSO ignoredItem = null)
    {
        if (string.IsNullOrWhiteSpace(itemId) || items == null)
        {
            return false;
        }

        for (int i = 0; i < items.Count; i++)
        {
            ItemSO item = items[i];
            if (item != null && item != ignoredItem && item.ItemId == itemId)
            {
                return true;
            }
        }

        return false;
    }

    private void OnValidate()
    {
        if (items == null)
        {
            return;
        }

        // Save chi giu ItemId, vi vay moi ItemSO trong database bat buoc phai co ID duy nhat.
        for (int i = 0; i < items.Count; i++)
        {
            ItemSO item = items[i];
            if (item != null && ContainsDuplicateId(item.ItemId, item))
            {
                Debug.LogError($"Duplicate ItemId '{item.ItemId}' in ItemDatabase.", this);
                return;
            }
        }
    }
}
