public partial class ChestManager
{
    public bool TryOpenChest(ChestType chestType)
    {
        ChestState state = GetState(chestType);
        ChestReward reward = state?.Peek();
        Inventory inventory = PlayerManager.Ins.Inventory;
        if (!IsInitialized || reward == null || inventory == null || !state.CanOpen)
        {
            return false;
        }

        if (!inventory.CanAddItem(reward.Equipment))
        {
            SetInventoryFullState(state, true);
            return false;
        }

        // Khoa loai ruong nay truoc khi cap nhat UI de tranh click lap trong animation.
        if (!state.BeginOpening())
        {
            return false;
        }

        if (!inventory.AddItem(reward.Equipment, 1))
        {
            state.EndOpening();
            SetInventoryFullState(state, true);
            return false;
        }

        Equipment openedEquipment = reward.Equipment;
        state.RemoveFront();
        state.ResetAutoTimer();
        CompleteDataFlow();
        StartOpenVisual(chestType, openedEquipment);
        return true;
    }

    public void CompleteOpenAnimation(ChestType chestType)
    {
        ChestState state = GetState(chestType);
        if (state == null || !state.EndOpening())
        {
            return;
        }

        // IsOpening la state runtime; UI doc lai state sau khi animation ket thuc.
        dataVersion++;
        RefreshView();
    }

    private void StartOpenVisual(ChestType chestType, Equipment equipment)
    {
        bool isAnimationPlaying = chestView != null
            && chestView.PlayChestOpen(chestType, equipment);
        if (!isAnimationPlaying)
        {
            // Khong co UI/Animator thi bo khoa ngay de auto-open khong bi ket.
            CompleteOpenAnimation(chestType);
        }
    }
}
