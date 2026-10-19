using UnityEngine;
using UnityEngine.UI;

public class CanvasNavigation : UICanvas
{
    
   [SerializeField] private PanelChestView panelChestView;
    [SerializeField] private ChestRuntimeTestTool chestRuntimeTestTool;
    [SerializeField] private Button levelSelectButton;

    public override void SetUp()
    {
        base.SetUp();
        OnInit();
    }

    public void OnInit()
    {
        levelSelectButton?.onClick.RemoveListener(OnButtonOpenLevelSelect);
        levelSelectButton?.onClick.AddListener(OnButtonOpenLevelSelect);
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
        levelSelectButton?.onClick.RemoveListener(OnButtonOpenLevelSelect);
        chestRuntimeTestTool?.OnDespawn();
        panelChestView?.OnDespawn();
        base.CloseDirectly();
    }

    public void OnButtonOpenLevelSelect()
    {
        UIManager.Ins.CloseUIDirectly<CanvasInventoryHero>();
        UIManager.Ins.CloseUIDirectly<CanvasNavigation>();
        UIManager.Ins.OpenUI<CanvasLevelSelect>();
    }
}
