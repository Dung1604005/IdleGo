using System.Collections.Generic;
using UnityEngine;

public class PanelChestView : PanelView, IChestView
{
    [SerializeField] private List<ChestSlotUI> chestSlots = new List<ChestSlotUI>();

    public override void OnInit()
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            chestSlots[i]?.OnInit();
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

        ChestManager.Ins.UnregisterView(this);
    }

    public void RefreshChests(ChestManager chestManager)
    {
        if (chestManager == null)
        {
            return;
        }

        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            if (slot != null)
            {
                slot.Refresh(chestManager.GetState(slot.ChestType));
            }
        }
    }

    public bool PlayChestOpen(ChestType chestType, Equipment equipment)
    {
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
}
