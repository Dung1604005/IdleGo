public partial class Inventory
{
    public void RegisterView(IInventoryView view)
    {
        inventoryView = view;
        RefreshView();
    }

    public void UnregisterView(IInventoryView view)
    {
        if (ReferenceEquals(inventoryView, view))
        {
            inventoryView = null;
        }
    }

    public bool SaveGame()
    {
        if (!IsInitialized
            || !canSave
            || !InventorySaveMapper.TryCreateSaveData(
                storage,
                playerManager,
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
                playerManager,
                this))
        {
            return false;
        }

        dataVersion++;
        lastSaveSucceeded = true;
        canSave = true;
        RefreshView();
        return true;
    }

    internal void OnItemDataChanged(Item item)
    {
        if (IsInitialized && storage.GetSlot(item) != null)
        {
            CompleteDataFlow();
        }
    }

    internal void OnCharacterRosterChanged(bool importStartingEquipment)
    {
        if (!IsInitialized)
        {
            return;
        }

        if (importStartingEquipment)
        {
            equipmentHandler.ImportCurrentEquipment(this);
        }

        CompleteDataFlow();
    }

    internal void OnPlayerProgressChanged()
    {
        if (IsInitialized)
        {
            CompleteDataFlow();
        }
    }

    private void CreateHandlers()
    {
        equipmentHandler = new InventoryEquipmentHandler(storage, playerManager);
        itemHandler = new InventoryItemHandler(storage, equipmentHandler);
    }

    private void CompleteDataFlow()
    {
        // Luong bat buoc: sua runtime data -> luu JSON -> data chu dong yeu cau UI ve lai.
        // UI van hien data moi neu save that bai; ket qua save duoc giu rieng.
        lastSaveSucceeded = SaveGame();
        dataVersion++;
        RefreshView();
    }

    private void RefreshView()
    {
        if (IsInitialized)
        {
            inventoryView?.RefreshInventory(this);
        }
    }
}
