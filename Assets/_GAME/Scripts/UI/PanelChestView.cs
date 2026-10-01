using System.Collections.Generic;
using UnityEngine;

public class PanelChestView : PanelView, IChestView
{
    [SerializeField] private ChestType selectedChestType = ChestType.NORMAL;
    [SerializeField] private List<ChestTypeTabUI> chestTabs = new List<ChestTypeTabUI>();
    [SerializeField] private List<ChestSlotUI> chestSlots = new List<ChestSlotUI>();

    public ChestType SelectedChestType => selectedChestType;

    public override void OnInit()
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            if (slot != null)
            {
                slot.OnInit();
                slot.SetVisible(slot.ChestType == selectedChestType);
            }
        }

        for (int i = 0; i < chestTabs.Count; i++)
        {
            chestTabs[i]?.OnInit(this);
        }

        ChestManager.Ins.RegisterView(this);
    }

    public override void OnDespawn()
    {
        // Slot dang chay phai mo khoa state truoc khi panel ngung nhan refresh.
        for (int i = 0; i < chestSlots.Count; i++)
        {
            chestSlots[i]?.OnDespawn();
        }

        for (int i = 0; i < chestTabs.Count; i++)
        {
            chestTabs[i]?.OnDespawn();
        }

        ChestManager.Ins.UnregisterView(this);
    }

    public bool SelectChestType(ChestType chestType)
    {
        ChestManager manager = ChestManager.Ins;
        ChestState currentState = manager.GetState(selectedChestType);
        if (currentState != null && currentState.IsOpening)
        {
            return false;
        }

        if (manager.GetState(chestType) == null)
        {
            return false;
        }

        selectedChestType = chestType;
        RefreshChests(manager);
        return true;
    }

    public void RefreshChests(ChestManager chestManager)
    {
        if (chestManager == null)
        {
            return;
        }

        bool selectionLocked = IsSelectionLocked(chestManager);
        RefreshSlots(chestManager);
        RefreshTabs(chestManager, selectionLocked);
    }

    public bool PlayChestOpen(ChestType chestType, Equipment equipment)
    {
        if (chestType != selectedChestType)
        {
            return false;
        }

        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            if (slot != null && slot.ChestType == chestType)
            {
                return slot.PlayOpen(equipment);
            }
        }

        return false;
    }

    private void RefreshSlots(ChestManager manager)
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            if (slot == null)
            {
                continue;
            }

            bool isSelected = slot.ChestType == selectedChestType;
            slot.SetVisible(isSelected);
            slot.Refresh(manager.GetState(slot.ChestType));
        }
    }

    private void RefreshTabs(ChestManager manager, bool selectionLocked)
    {
        for (int i = 0; i < chestTabs.Count; i++)
        {
            ChestTypeTabUI tab = chestTabs[i];
            if (tab == null)
            {
                continue;
            }

            bool isSelected = tab.ChestType == selectedChestType;
            tab.Refresh(manager.GetState(tab.ChestType), isSelected, selectionLocked);
        }
    }

    private bool IsSelectionLocked(ChestManager manager)
    {
        ChestState state = manager.GetState(selectedChestType);
        return state != null && state.IsOpening;
    }
}
