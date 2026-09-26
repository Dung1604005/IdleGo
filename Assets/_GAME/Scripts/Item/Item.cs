using System;
using UnityEngine;

[Serializable]
public class Item
{
    [SerializeField] private string instanceId;
    [SerializeField] private ItemSO data;
    [NonSerialized] private Inventory inventory;

    public Item()
    {
    }

    public Item(ItemSO itemData)
    {
        data = itemData;
        EnsureInstanceId();
    }

    public string InstanceId
    {
        get
        {
            EnsureInstanceId();
            return instanceId;
        }
    }

    public ItemSO Data => data;
    public ItemType ItemType => data != null ? data.ItemType : default;
    public RarityType RarityType => data != null ? data.RarityType : default;
    public int MaxQuantity => data != null ? data.MaxQuantity : 1;
    public bool IsEquipment => this is Equipment;
    public bool IsStackable => !IsEquipment && MaxQuantity > 1;

    public static Item Create(ItemSO itemData)
    {
        if (itemData == null)
        {
            return null;
        }

        if (itemData is EquipmentDataSO equipmentData)
        {
            return new Equipment(equipmentData);
        }

        return new Item(itemData);
    }

    internal void RestoreInstanceId(string savedInstanceId)
    {
        instanceId = string.IsNullOrWhiteSpace(savedInstanceId)
            ? Guid.NewGuid().ToString("N")
            : savedInstanceId;
    }

    internal void SetInventory(Inventory ownerInventory)
    {
        inventory = ownerInventory;
    }

    protected void NotifyInventoryDataChanged()
    {
        inventory?.OnItemDataChanged(this);
    }

    private void EnsureInstanceId()
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            instanceId = Guid.NewGuid().ToString("N");
        }
    }
}
