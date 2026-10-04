using UnityEngine;

public class CanvasItemInfomationUI : UICanvas
{
    [SerializeField] private PanelItemInformation selectedItemPanel;
    [SerializeField] private PanelItemInformation equippedComparisonPanel;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        selectedItemPanel?.OnInit();
        equippedComparisonPanel?.OnInit();
    }

    public void OnDespawn()
    {
        selectedItemPanel?.OnDespawn();
        equippedComparisonPanel?.OnDespawn();
        DataManager.Ins.ClearSelectedEquipment();
    }

    public void OnButtonClose()
    {
        CloseDirectly();
    }

    public override void CloseDirectly()
    {
        OnDespawn();
        base.CloseDirectly();
    }
}
