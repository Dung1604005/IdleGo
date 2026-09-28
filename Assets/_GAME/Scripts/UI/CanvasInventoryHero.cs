using UnityEngine;

public class CanvasInventoryHero : UICanvas, IInventoryView
{
    [SerializeField] private PanelInventoryView panelInventoryView;
    [SerializeField] private PanelHeroView panelHeroView;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        OnDespawn();
        panelInventoryView?.OnInit();
        panelHeroView?.OnInit();

        // Inventory chi biet Canvas; Canvas chuyen data xuong dung panel phu trach hien thi.
        GetInventory()?.RegisterView(this);
    }

    public override void Open()
    {
        base.Open();
        panelInventoryView?.OnOpen();
        panelHeroView?.OnOpen();
    }

    public override void CloseDirectly()
    {
        OnDespawn();
        base.CloseDirectly();
    }

    public bool RequestIncreaseCapacity(int additionalSlots)
    {
        return panelInventoryView != null
            && panelInventoryView.RequestIncreaseCapacity(additionalSlots);
    }

    public void RefreshInventory(Inventory sourceInventory)
    {
        panelInventoryView?.RefreshInventory(sourceInventory);
    }

    public void RefreshEquipment()
    {
        panelHeroView?.RefreshEquipmentSlotUI();
    }

    public void OnDespawn()
    {
        GetInventory()?.UnregisterView(this);
        panelInventoryView?.OnDespawn();
        panelHeroView?.OnDespawn();
    }

    private void Update()
    {
        panelInventoryView?.OnUpdate();
        panelHeroView?.OnUpdate();
    }

    private static Inventory GetInventory()
    {
        PlayerManager playerManager = PlayerManager.Ins;
        return playerManager != null ? playerManager.Inventory : null;
    }
}
