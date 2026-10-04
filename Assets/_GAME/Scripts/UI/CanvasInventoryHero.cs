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

        // Canvas chi dang ky nhan lenh; panel se tu doc data qua DataManager.
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

    public void RefreshInventory()
    {
        panelInventoryView?.RefreshInventory();
        panelHeroView?.RefreshHeroView();
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
        return DataManager.Ins.InventoryData;
    }
}
