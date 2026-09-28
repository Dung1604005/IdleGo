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
    [SerializeField] private List<ItemSlotUI> slotViews = new List<ItemSlotUI>();
    [SerializeField] private InventoryContentLayout contentLayout = new InventoryContentLayout();

    private Inventory inventory;
    private bool isInitialized;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        OnDespawn();

        Player player = PlayerManager.Ins.GetTarget();
        if (player == null)
        {
            Debug.LogWarning("CanvasInventoryHero cannot find an active player in PlayerManager.");
            return;
        }

        OnInit(player.Inventory);
    }

    public void OnInit(Inventory targetInventory)
    {
        contentLayout ??= new InventoryContentLayout();
        slotViews ??= new List<ItemSlotUI>();

        if (viewport == null || content == null || gridLayout == null)
        {
            Debug.LogError("CanvasInventoryHero is missing Viewport, Content or GridLayoutGroup.");
            return;
        }

        inventory = targetInventory;
        contentLayout.OnInit(viewport, content, gridLayout);
        isInitialized = inventory != null;

        // RegisterView goi render tu Inventory de data luon la noi bat dau cap nhat UI.
        inventory?.RegisterView(this);
    }

    public void OnDespawn()
    {
        inventory?.UnregisterView(this);
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

        if (!EnsureSlotViewCount(sourceInventory.Capacity))
        {
            return;
        }

        IReadOnlyList<InventorySlot> slots = sourceInventory.Slots;
        for (int i = 0; i < sourceInventory.Capacity; i++)
        {
            ItemSlotUI slotView = slotViews[i];
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

    private bool EnsureSlotViewCount(int requiredCount)
    {
        slotViews.RemoveAll(slotView => slotView == null);

        // Neu template la slot dau tien nam san trong Content thi dung luon, khong clone trung no.
        if (slotViews.Count == 0
            && slotTemplate != null
            && slotTemplate.transform.parent == content)
        {
            slotViews.Add(slotTemplate);
        }

        ItemSlotUI template = slotTemplate != null
            ? slotTemplate
            : slotViews.Count > 0 ? slotViews[0] : null;

        if (template == null)
        {
            Debug.LogError("CanvasInventoryHero needs an ItemSlotUI template.");
            return false;
        }

        while (slotViews.Count < requiredCount)
        {
            ItemSlotUI newSlot = Instantiate(template, content);
            slotViews.Add(newSlot);
        }

        for (int i = 0; i < slotViews.Count; i++)
        {
            slotViews[i].gameObject.SetActive(i < requiredCount);
        }

        return true;
    }
}
