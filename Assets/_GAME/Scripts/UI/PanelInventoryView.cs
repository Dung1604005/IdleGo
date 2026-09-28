using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelInventoryView : PanelView
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private ItemSlotUI slotTemplate;
    [SerializeField] private InventoryContentLayout contentLayout = new InventoryContentLayout();

    private readonly List<ItemSlotUI> activeSlotViews = new List<ItemSlotUI>();
    private Inventory inventory;
    private bool isInitialized;

    public override void OnInit()
    {
        OnDespawn();
        contentLayout ??= new InventoryContentLayout();
        if (!HasRequiredReferences())
        {
            return;
        }

        PlayerManager playerManager = PlayerManager.Ins;
        inventory = playerManager != null ? playerManager.Inventory : null;
        if (inventory == null)
        {
            Debug.LogWarning("PanelInventoryView cannot access Inventory.");
            return;
        }

        contentLayout.OnInit(viewport, content, gridLayout);
        SimplePool.PreLoad(slotTemplate, inventory.Capacity, content);
        isInitialized = true;

    }

    public override void OnUpdate()
    {
        if (isInitialized)
        {
            contentLayout.RefreshIfViewportWidthChanged();
        }
    }

    public override void OnOpen()
    {
        if (isInitialized)
        {
            contentLayout.RefreshIfViewportWidthChanged();
        }
    }

    public override void OnDespawn()
    {
        DespawnAllSlotViews();
        inventory = null;
        isInitialized = false;
        contentLayout?.OnDespawn();
    }

    public void RefreshInventory(Inventory sourceInventory)
    {
        if (!CanRefresh(sourceInventory)
            || !SyncSlotViewCount(sourceInventory.Capacity))
        {
            return;
        }

        IReadOnlyList<InventorySlot> slots = sourceInventory.Slots;
        for (int i = 0; i < sourceInventory.Capacity; i++)
        {
            ItemSlotUI slotView = activeSlotViews[i];
            slotView.OnInit(i);
            slotView.SetData(slots[i]);
        }

        contentLayout.Refresh(sourceInventory.Capacity);
    }

    public bool RequestIncreaseCapacity(int additionalSlots)
    {
        // Inventory save data truoc, sau do tu goi RefreshInventory ve panel nay.
        return inventory != null && inventory.IncreaseCapacity(additionalSlots);
    }

    private bool HasRequiredReferences()
    {
        bool isValid = viewport != null
            && content != null
            && gridLayout != null
            && slotTemplate != null;
        if (!isValid)
        {
            Debug.LogError("PanelInventoryView is missing an inventory UI reference.");
        }
        return isValid;
    }

    private bool CanRefresh(Inventory sourceInventory)
    {
        return isInitialized && ReferenceEquals(inventory, sourceInventory);
    }

    private bool SyncSlotViewCount(int requiredCount)
    {
        while (activeSlotViews.Count < requiredCount)
        {
            if (!TrySpawnSlot())
            {
                return false;
            }
        }

        while (activeSlotViews.Count > requiredCount)
        {
            DespawnLastSlot();
        }
        return true;
    }

    private bool TrySpawnSlot()
    {
        ItemSlotUI newSlot = SimplePool.Spawn(
            slotTemplate,
            content.position,
            Quaternion.identity,
            content
        );
        if (newSlot == null)
        {
            Debug.LogError("SimplePool could not spawn an ItemSlotUI.");
            return false;
        }

        activeSlotViews.Add(newSlot);
        return true;
    }

    private void DespawnLastSlot()
    {
        int lastIndex = activeSlotViews.Count - 1;
        ItemSlotUI unusedSlot = activeSlotViews[lastIndex];
        activeSlotViews.RemoveAt(lastIndex);
        SimplePool.Despawn(unusedSlot);
    }

    private void DespawnAllSlotViews()
    {
        for (int i = activeSlotViews.Count - 1; i >= 0; i--)
        {
            SimplePool.Despawn(activeSlotViews[i]);
        }
        activeSlotViews.Clear();
    }
}
