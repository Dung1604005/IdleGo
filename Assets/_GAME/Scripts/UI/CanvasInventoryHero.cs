using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasInventoryHero : UICanvas, IInventoryView
{
    [Header("Inventory Grid")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private ItemSlotUI slotTemplate;
    [SerializeField] private InventoryContentLayout contentLayout = new InventoryContentLayout();

    private readonly List<ItemSlotUI> activeSlotViews = new List<ItemSlotUI>();
    private Inventory inventory;
    private bool isInitialized;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        OnDespawn();

        PlayerManager playerManager = PlayerManager.Ins;
        if (playerManager == null)
        {
            Debug.LogWarning("CanvasInventoryHero cannot access PlayerManager.");
            return;
        }

        OnInit(playerManager.Inventory);
    }

    public void OnInit(Inventory targetInventory)
    {
        contentLayout ??= new InventoryContentLayout();

        if (viewport == null || content == null || gridLayout == null || slotTemplate == null)
        {
            Debug.LogError(
                "CanvasInventoryHero is missing Viewport, Content, GridLayoutGroup or ItemSlotUI template."
            );
            return;
        }

        inventory = targetInventory;
        contentLayout.OnInit(viewport, content, gridLayout);
        isInitialized = inventory != null;

        if (isInitialized)
        {
            // Pool phai duoc tao truoc khi Inventory dang ky view va yeu cau ve cac slot dau tien.
            SimplePool.PreLoad(slotTemplate, inventory.Capacity, content);
        }

        // RegisterView goi render tu Inventory de data luon la noi bat dau cap nhat UI.
        inventory?.RegisterView(this);
    }

    public void OnDespawn()
    {
        inventory?.UnregisterView(this);
        DespawnAllSlotViews();
        inventory = null;
        isInitialized = false;
        contentLayout?.OnDespawn();
    }

    public override void Open()
    {
        base.Open();
        contentLayout.RefreshIfViewportWidthChanged();
    }

    public override void CloseDirectly()
    {
        OnDespawn();
        base.CloseDirectly();
    }

    public void RefreshInventory(Inventory sourceInventory)
    {
        if (!isInitialized || !ReferenceEquals(inventory, sourceInventory))
        {
            return;
        }

        if (!SyncSlotViewCount(sourceInventory.Capacity))
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
        // Khong render tai day. Inventory se save xong roi goi RefreshInventory.
        return inventory != null && inventory.IncreaseCapacity(additionalSlots);
    }

    private void Update()
    {
        if (isInitialized)
        {
            // Viewport co the doi rong khi doi ti le man hinh, xoay may hoac resize Game view.
            contentLayout.RefreshIfViewportWidthChanged();
        }
    }

    private bool SyncSlotViewCount(int requiredCount)
    {
        if (slotTemplate == null)
        {
            Debug.LogError("CanvasInventoryHero needs an ItemSlotUI template.");
            return false;
        }

        while (activeSlotViews.Count < requiredCount)
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
        }

        while (activeSlotViews.Count > requiredCount)
        {
            int lastIndex = activeSlotViews.Count - 1;
            ItemSlotUI unusedSlot = activeSlotViews[lastIndex];
            activeSlotViews.RemoveAt(lastIndex);
            SimplePool.Despawn(unusedSlot);
        }

        return true;
    }

    private void DespawnAllSlotViews()
    {
        // Xoa khoi danh sach truoc khi tra ve pool de lan mo UI sau spawn lai dung luong.
        for (int i = activeSlotViews.Count - 1; i >= 0; i--)
        {
            SimplePool.Despawn(activeSlotViews[i]);
        }

        activeSlotViews.Clear();
    }
}
