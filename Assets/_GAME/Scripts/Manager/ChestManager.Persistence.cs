public partial class ChestManager
{
    public void RegisterView(IChestView view)
    {
        chestView = view;
        RefreshView();
    }

    public void UnregisterView(IChestView view)
    {
        if (ReferenceEquals(chestView, view))
        {
            chestView = null;
        }
    }

    public bool SaveGame()
    {
        if (!IsInitialized)
        {
            return false;
        }

        return ChestSaveSystem.TrySave(saveKey, ChestSaveMapper.Create(chestStates));
    }

    public bool LoadGame()
    {
        if (!IsInitialized || !ChestSaveSystem.HasSave(saveKey))
        {
            return false;
        }

        if (!ChestSaveSystem.TryLoad(saveKey, out ChestManagerSaveData saveData))
        {
            return false;
        }

        ChestSaveMapper.Apply(saveData, this, DataManager.Ins.InventoryData);
        dataVersion++;
        lastSaveSucceeded = true;
        return true;
    }

    private void CompleteDataFlow()
    {
        // Luong chung: state doi -> save JSON -> UI doc lai state moi.
        lastSaveSucceeded = SaveGame();
        dataVersion++;
        RefreshView();
    }

    private void RefreshView()
    {
        if (IsInitialized)
        {
            // View tu doc ChestManager hien tai qua DataManager.
            chestView?.RefreshChests();
        }
    }
}
