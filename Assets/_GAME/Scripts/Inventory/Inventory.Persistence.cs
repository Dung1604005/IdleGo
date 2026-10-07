public partial class Inventory
{
    public void RegisterView(IInventoryView view)
    {
        if (view == null)
        {
            return;
        }

        inventoryViews ??= new System.Collections.Generic.List<IInventoryView>();
        if (!inventoryViews.Contains(view))
        {
            inventoryViews.Add(view);
        }

        RefreshView();
    }

    public void UnregisterView(IInventoryView view)
    {
        inventoryViews?.Remove(view);
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
        bool isStored = storage.GetSlot(item) != null;
        bool isEquipped = item is Equipment equipment && equipment.IsEquipped;
        if (IsInitialized && (isStored || isEquipped))
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
        equipmentHandler = new InventoryEquipmentHandler(storage, playerManager, this);
        itemHandler = new InventoryItemHandler(storage, equipmentHandler);
        enchantHandler = new InventoryEnchantHandler(storage);
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
        if (!IsInitialized || inventoryViews == null)
        {
            return;
        }

        // Moi view tu doc Inventory qua DataManager, khong nhan data truc tiep tu callback.
        for (int i = inventoryViews.Count - 1; i >= 0; i--)
        {
            inventoryViews[i]?.RefreshInventory();
        }
    }

    internal void RefreshViews()
    {
        RefreshView();
    }
}
