using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    [SerializeField] private InventoryStorage storage = new InventoryStorage();
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private string saveKey = "PLAYER_INVENTORY";

    [NonSerialized] private CharacterEquipment characterEquipment;
    [NonSerialized] private InventoryItemHandler itemHandler;
    [NonSerialized] private InventoryEquipmentHandler equipmentHandler;
    [NonSerialized] private int dataVersion;
    [NonSerialized] private bool lastSaveSucceeded;
    [NonSerialized] private bool canSave;

    public IReadOnlyList<InventorySlot> Slots => storage.Slots;
    public int Capacity => storage.Capacity;
    public int DataVersion => dataVersion;
    public bool LastSaveSucceeded => lastSaveSucceeded;
    public bool IsInitialized { get; private set; }

    public void OnInit(Player owner)
    {
        characterEquipment = owner != null ? owner.Equipment : null;
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
        characterEquipment = null;
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

    public bool Equip(Item item)
    {
        if (!IsInitialized || !equipmentHandler.Equip(item))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public bool Unequip(Item item)
    {
        if (!IsInitialized || !equipmentHandler.Unequip(item))
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    public bool Unequip(EquipmentType equipmentType)
    {
        if (!IsInitialized || !equipmentHandler.Unequip(equipmentType))
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

    public bool SaveGame()
    {
        if (!IsInitialized
            || !canSave
            || !InventorySaveMapper.TryCreateSaveData(
                storage,
                characterEquipment,
                out InventorySaveData saveData))
        {
            return false;
        }

        return InventorySaveSystem.TrySave(saveKey, saveData);
    }

    public bool LoadGame()
    {
        if (!IsInitialized
            || !InventorySaveSystem.TryLoad(saveKey, out InventorySaveData saveData)
            || !InventorySaveMapper.ApplySaveData(
                saveData,
                itemDatabase,
                storage,
                equipmentHandler,
                this))
        {
            return false;
        }

        // UI sau nay chi can so sanh DataVersion, khong can dang ky event.
        dataVersion++;
        lastSaveSucceeded = true;
        canSave = true;
        return true;
    }

    internal void OnItemDataChanged(Item item)
    {
        if (IsInitialized && storage.GetSlot(item) != null)
        {
            CompleteDataFlow();
        }
    }

    private void CreateHandlers()
    {
        equipmentHandler = new InventoryEquipmentHandler(storage, characterEquipment);
        itemHandler = new InventoryItemHandler(storage, equipmentHandler);
    }

    private void CompleteDataFlow()
    {
        // Moi thay doi deu ket thuc theo thu tu: runtime data -> PlayerPrefs JSON -> DataVersion.
        lastSaveSucceeded = SaveGame();
        if (lastSaveSucceeded)
        {
            dataVersion++;
        }
    }
}
