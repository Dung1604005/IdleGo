using UnityEngine;

public class CanvasMainMenu : UICanvas
{
    [SerializeField] private PanelChestView panelChestView;

    public override void SetUp()
    {
        base.SetUp();
        OnInit();
    }

    public void OnInit()
    {
        panelChestView?.OnDespawn();
        panelChestView?.OnInit();
    }

    public override void Open()
    {
        base.Open();
        panelChestView?.OnOpen();
    }

    public override void CloseDirectly()
    {
        panelChestView?.OnDespawn();
        base.CloseDirectly();
    }

    private void Update()
    {
        panelChestView?.OnUpdate();
    }
}
