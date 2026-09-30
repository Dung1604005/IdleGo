using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public partial class Inventory
{
    [SerializeField] private InventoryStorage storage = new InventoryStorage();
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private string saveKey = "PLAYER_INVENTORY";

    [NonSerialized] private PlayerManager playerManager;
    [NonSerialized] private InventoryItemHandler itemHandler;
    [NonSerialized] private InventoryEquipmentHandler equipmentHandler;
    [NonSerialized] private int dataVersion;
    [NonSerialized] private bool lastSaveSucceeded;
    [NonSerialized] private bool canSave;
    [NonSerialized] private IInventoryView inventoryView;

    public IReadOnlyList<InventorySlot> Slots => storage.Slots;
    public int Capacity => storage.Capacity;
    public int DataVersion => dataVersion;
    public bool LastSaveSucceeded => lastSaveSucceeded;
    public bool IsInitialized { get; private set; }

    public void OnInit(PlayerManager owner)
    {
        playerManager = owner;
        storage ??= new InventoryStorage();
        storage.OnInit(this);
        CreateHandlers();

        dataVersion = 0;
        lastSaveSucceeded = true;
        canSave = true;
        IsInitialized = true;

        if (InventorySaveSystem.HasSave(saveKey))
        {
            if (!LoadGame())
            {
                // Khong ghi de save cu neu ItemDatabase chua du de khoi phuc inventory.
                canSave = false;
                Debug.LogError("Inventory save exists but cannot be loaded.");
            }
            return;
        }

        equipmentHandler.ImportCurrentEquipment(this);
        CompleteDataFlow();
    }

    public void OnDespawn()
    {
        if (IsInitialized)
        {
            lastSaveSucceeded = SaveGame();
        }

        storage.OnDespawn();
        playerManager = null;
        itemHandler = null;
        equipmentHandler = null;
        canSave = false;
        IsInitialized = false;
    }

    public bool AddItem(ItemSO itemData, int amount)
    {
        return AddItem(Item.Create(itemData), amount);
    }

    public bool AddItem(Item item, int amount)
    {
        if (!IsInitialized || !itemHandler.AddItem(item, amount, this))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public bool CanAddItem(Item item, int amount = 1)
    {
        return IsInitialized && itemHandler.CanAddItem(item, amount);
    }

    public ItemSO GetItemData(string itemId)
    {
        return itemDatabase != null ? itemDatabase.GetItem(itemId) : null;
    }

    public bool RemoveItem(Item item, int amount)
    {
        if (!IsInitialized || !itemHandler.RemoveItem(item, amount))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public bool RemoveItem(ItemSO itemData, int amount)
    {
        if (!IsInitialized || !itemHandler.RemoveItem(itemData, amount))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public bool Equip(Player player, Item item)
    {
        if (!IsInitialized || !equipmentHandler.Equip(player, item))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public bool Unequip(Player player, Item item)
    {
        if (!IsInitialized || !equipmentHandler.Unequip(player, item))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public bool Unequip(Player player, EquipmentType equipmentType)
    {
        if (!IsInitialized || !equipmentHandler.Unequip(player, equipmentType))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public bool IncreaseCapacity(int additionalSlots)
    {
        if (!IsInitialized || !storage.IncreaseCapacity(additionalSlots))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public void SortByRarityDescending()
    {
        if (!IsInitialized)
        {
            return;
        }

        storage.SortByRarityDescending();
        CompleteDataFlow();
    }

    public int CountItem(ItemSO itemData)
    {
        return storage.CountItem(itemData);
    }

    public InventorySlot GetSlot(Item item)
    {
        return storage.GetSlot(item);
    }

}
