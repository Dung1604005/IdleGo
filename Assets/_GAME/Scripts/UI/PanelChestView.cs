using System;
using UnityEngine;
using UnityEngine.UI;

public class PanelChestView : PanelView, IChestView
{
    [SerializeField] private ChestType selectedChestType = ChestType.NORMAL;
    [SerializeField] private ChestSlotUI chestSlot;
    [SerializeField] private Button previousChestButton;
    [SerializeField] private Button nextChestButton;
    [SerializeField] private GameObject previousNewChestIcon;
    [SerializeField] private GameObject nextNewChestIcon;

    private ChestType[] orderedChestTypes;
    private int[] lastSeenReceivedVersions;

    public ChestType SelectedChestType => selectedChestType;

    public override void OnInit()
    {
        BuildChestOrder();
        EnsureSelectedChestType();
        chestSlot?.OnInit();
        chestSlot?.SetChestType(selectedChestType);
        ChestManager.Ins.RegisterView(this);
    }

    public override void OnDespawn()
    {
        // Slot dang chay phai mo khoa state truoc khi panel ngung nhan refresh.
        chestSlot?.OnDespawn();
        ChestManager.Ins.UnregisterView(this);
    }

    public void OnButtonPreviousChest()
    {
        MoveSelection(-1);
    }

    public void OnButtonNextChest()
    {
        MoveSelection(1);
    }

    public bool SelectChestType(ChestType chestType)
    {
        if (orderedChestTypes == null || orderedChestTypes.Length == 0)
        {
            return false;
        }

        ChestManager manager = ChestManager.Ins;
        ChestState currentState = manager.GetState(selectedChestType);
        if (currentState != null && currentState.IsOpening)
        {
            return false;
        }

        if (Array.IndexOf(orderedChestTypes, chestType) < 0
            || manager.GetState(chestType) == null)
        {
            return false;
        }

        selectedChestType = chestType;
        chestSlot?.SetChestType(selectedChestType);
        RefreshChests(manager);
        return true;
    }

    public void RefreshChests(ChestManager chestManager)
    {
        if (chestManager == null || orderedChestTypes == null)
        {
            return;
        }

        MarkSelectedChestViewed(chestManager);
        ChestState selectedState = chestManager.GetState(selectedChestType);
        chestSlot?.Refresh(selectedState);
        RefreshNavigation(chestManager, selectedState);
    }

    public bool PlayChestOpen(ChestType chestType, Equipment equipment)
    {
        return chestType == selectedChestType
            && chestSlot != null
            && chestSlot.PlayOpen(equipment);
    }

    private void MoveSelection(int direction)
    {
        if (orderedChestTypes == null || orderedChestTypes.Length < 2)
        {
            return;
        }

        int currentIndex = Array.IndexOf(orderedChestTypes, selectedChestType);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        // Cong them Length giup Previous/Next quay vong o hai dau danh sach.
        int nextIndex = (currentIndex + direction + orderedChestTypes.Length)
            % orderedChestTypes.Length;
        SelectChestType(orderedChestTypes[nextIndex]);
    }

    private void RefreshNavigation(ChestManager manager, ChestState selectedState)
    {
        bool canNavigate = orderedChestTypes.Length > 1
            && (selectedState == null || !selectedState.IsOpening);
        if (previousChestButton != null)
        {
            previousChestButton.interactable = canNavigate;
        }
        if (nextChestButton != null)
        {
            nextChestButton.interactable = canNavigate;
        }

        ChestType previousType = GetRelativeChestType(-1);
        ChestType nextType = GetRelativeChestType(1);
        previousNewChestIcon?.SetActive(HasUnseenChest(manager, previousType));
        nextNewChestIcon?.SetActive(HasUnseenChest(manager, nextType));
    }

    private bool HasUnseenChest(ChestManager manager, ChestType chestType)
    {
        int index = Array.IndexOf(orderedChestTypes, chestType);
        ChestState state = manager.GetState(chestType);
        if (index < 0 || state == null || state.Count == 0)
        {
            return false;
        }

        if (state.ReceivedVersion < lastSeenReceivedVersions[index])
        {
            lastSeenReceivedVersions[index] = 0;
        }
        return state.ReceivedVersion > lastSeenReceivedVersions[index];
    }

    private void MarkSelectedChestViewed(ChestManager manager)
    {
        int index = Array.IndexOf(orderedChestTypes, selectedChestType);
        ChestState state = manager.GetState(selectedChestType);
        if (index >= 0 && state != null)
        {
            // Dang hien thi nghia la thong bao moi cua loai ruong nay da duoc xem.
            lastSeenReceivedVersions[index] = state.ReceivedVersion;
        }
    }

    private ChestType GetRelativeChestType(int offset)
    {
        int index = Array.IndexOf(orderedChestTypes, selectedChestType);
        index = index < 0 ? 0 : index;
        int targetIndex = (index + offset + orderedChestTypes.Length)
            % orderedChestTypes.Length;
        return orderedChestTypes[targetIndex];
    }

    private void BuildChestOrder()
    {
        // Gia tri so cua ChestType quyet dinh thu tu carousel.
        orderedChestTypes = (ChestType[])Enum.GetValues(typeof(ChestType));
        Array.Sort(orderedChestTypes, (left, right) =>
            ((int)left).CompareTo((int)right));
        if (lastSeenReceivedVersions == null
            || lastSeenReceivedVersions.Length != orderedChestTypes.Length)
        {
            lastSeenReceivedVersions = new int[orderedChestTypes.Length];
        }
    }

    private void EnsureSelectedChestType()
    {
        if (orderedChestTypes.Length > 0
            && Array.IndexOf(orderedChestTypes, selectedChestType) < 0)
        {
            selectedChestType = orderedChestTypes[0];
        }
    }
}
