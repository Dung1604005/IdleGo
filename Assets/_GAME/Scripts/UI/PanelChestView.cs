using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public partial class PanelChestView : PanelView, IChestView
{
    [SerializeField] private ChestType selectedChestType = ChestType.NORMAL;
    [SerializeField] private List<ChestSlotUI> chestSlots = new List<ChestSlotUI>();
    [FormerlySerializedAs("chestSlot"), HideInInspector]
    [SerializeField] private ChestSlotUI legacyChestSlot;
    [SerializeField] private Button previousChestButton;
    [SerializeField] private Button nextChestButton;
    [SerializeField] private GameObject previousNewChestIcon;
    [SerializeField] private GameObject nextNewChestIcon;

    [Header("Carousel")]
    [SerializeField, Min(0.05f)] private float transitionDuration = 0.25f;
    [SerializeField, Min(1f)] private float horizontalSlideDistance = 180f;
    [SerializeField, Range(0.05f, 1f)] private float sideScale = 0.5f;

    private ChestType[] orderedChestTypes;
    private int[] lastSeenReceivedVersions;
    private Vector2 centerPosition;

    public ChestType SelectedChestType => selectedChestType;
    public bool CanNavigate
    {
        get
        {
            ChestState state = ChestManager.Ins.GetState(selectedChestType);
            return !isTransitioning
                && orderedChestTypes != null
                && orderedChestTypes.Length > 1
                && (state == null || !state.IsOpening);
        }
    }

    public override void OnInit()
    {
        MigrateLegacySlot();
        BuildChestOrder();
        EnsureSelectedChestType();
        InitSlots();
        ResetCarousel();
        ChestManager.Ins.RegisterView(this);
    }

    public override void OnDespawn()
    {
        CancelTransition();
        for (int i = 0; i < chestSlots.Count; i++)
        {
            chestSlots[i]?.OnDespawn();
        }
        ChestManager.Ins.UnregisterView(this);
    }

    public void OnButtonPreviousChest()
    {
        TryStartTransition(-1);
    }

    public void OnButtonNextChest()
    {
        TryStartTransition(1);
    }

    public void RefreshChests(ChestManager chestManager)
    {
        if (chestManager == null || orderedChestTypes == null)
        {
            return;
        }

        RefreshSlots(chestManager);
        if (!isTransitioning)
        {
            MarkSelectedChestViewed(chestManager);
        }
        RefreshNavigation(chestManager);
    }

    public bool PlayChestOpen(ChestType chestType, Equipment equipment)
    {
        ChestSlotUI slot = GetSlot(chestType);
        return !isTransitioning
            && chestType == selectedChestType
            && slot != null
            && slot.PlayOpen(equipment);
    }

    private void InitSlots()
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            if (slot == null)
            {
                continue;
            }
            slot.OnInit();
            slot.SetInteractionLocked(slot.ChestType != selectedChestType);
        }
    }

    private void MigrateLegacySlot()
    {
        chestSlots ??= new List<ChestSlotUI>();
        if (chestSlots.Count == 0 && legacyChestSlot != null)
        {
            chestSlots.Add(legacyChestSlot);
        }
    }

    private void RefreshSlots(ChestManager manager)
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            if (slot != null)
            {
                slot.Refresh(manager.GetState(slot.ChestType));
            }
        }
    }

    private ChestSlotUI GetSlot(ChestType chestType)
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            if (slot != null && slot.ChestType == chestType)
            {
                return slot;
            }
        }
        return null;
    }

    private void RefreshNavigation(ChestManager manager)
    {
        if (previousChestButton != null)
        {
            previousChestButton.interactable = CanNavigate;
        }
        if (nextChestButton != null)
        {
            nextChestButton.interactable = CanNavigate;
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
            // Loai ruong o giua da duoc xem nen xoa moc thong bao moi.
            lastSeenReceivedVersions[index] = state.ReceivedVersion;
        }
    }

    private void BuildChestOrder()
    {
        // Gia tri so cua ChestType quyet dinh thu tu Prev/Next.
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
        if (GetSlot(selectedChestType) != null)
        {
            return;
        }

        for (int i = 0; i < orderedChestTypes.Length; i++)
        {
            if (GetSlot(orderedChestTypes[i]) != null)
            {
                selectedChestType = orderedChestTypes[i];
                return;
            }
        }
    }
}
