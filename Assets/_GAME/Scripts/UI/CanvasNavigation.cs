using UnityEngine;

public class CanvasNavigation : UICanvas
{
    
   [SerializeField] private PanelChestView panelChestView;
    [SerializeField] private ChestRuntimeTestTool chestRuntimeTestTool;

    public override void SetUp()
    {
        base.SetUp();
        OnInit();
    }

    public void OnInit()
    {
        chestRuntimeTestTool?.OnDespawn();
        panelChestView?.OnDespawn();
        panelChestView?.OnInit();
        chestRuntimeTestTool?.OnInit();
    }

    public override void Open()
    {
        base.Open();
        panelChestView?.OnOpen();
    }

    public override void CloseDirectly()
    {
        chestRuntimeTestTool?.OnDespawn();
        panelChestView?.OnDespawn();
        base.CloseDirectly();
    }
}
