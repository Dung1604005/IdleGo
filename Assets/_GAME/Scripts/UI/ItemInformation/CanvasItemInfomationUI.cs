using UnityEngine;

public class CanvasItemInfomationUI : UICanvas, IInventoryView
{
    [SerializeField] private PanelItemInformation selectedItemPanel;
    [SerializeField] private PanelItemInformation equippedComparisonPanel;
    [SerializeField, Min(0f)] private float panelHorizontalOffset = 241f;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        RefreshInventory();
        DataManager.Ins.InventoryData?.RegisterView(this);
    }

    public void RefreshInventory()
    {
        Equipment selectedEquipment = DataManager.Ins.SelectedEquipment;
        Equipment equippedEquipment = GetEquippedComparison(selectedEquipment);
        selectedItemPanel?.OnInit(selectedEquipment);
        equippedComparisonPanel?.OnInit(equippedEquipment);
        RefreshPanelPositions(equippedEquipment?.Data != null);
    }

    public void OnDespawn()
    {
        DataManager.Ins.InventoryData?.UnregisterView(this);
        selectedItemPanel?.OnDespawn();
        equippedComparisonPanel?.OnDespawn();
        DataManager.Ins.ClearSelectedEquipment();
    }

    public void OnButtonClose()
    {
        CloseDirectly();
    }

    public void OnBackgroundClick()
    {
        CloseDirectly();
    }

    public override void CloseDirectly()
    {
        OnDespawn();
        base.CloseDirectly();
    }

    private static Equipment GetEquippedComparison(Equipment selectedEquipment)
    {
        if (selectedEquipment?.Data == null
            || DataManager.Ins.SelectedEquipmentSource
                != EquipmentSelectionSource.INVENTORY_SLOT)
        {
            return null;
        }

        // Panel so sanh chi hien khi nguoi choi mo item tu InventorySlot.
        return DataManager.Ins.SelectedCharacter?.Equipment?.GetEquipment(
            selectedEquipment.EquipmentType);
    }

    private void RefreshPanelPositions(bool hasEquippedComparison)
    {
        float offset = Mathf.Abs(panelHorizontalOffset);
        SetPanelPosition(selectedItemPanel,
            hasEquippedComparison ? offset : 0f);
        SetPanelPosition(equippedComparisonPanel, -offset);
    }

    private static void SetPanelPosition(
        PanelItemInformation panel,
        float positionX)
    {
        if (panel == null || panel.transform is not RectTransform rect)
        {
            return;
        }

        Vector2 position = rect.anchoredPosition;
        position.x = positionX;
        // Pivot nam o canh tren, nen dat Y bang nua chieu cao se dua tam panel ve Y = 0.
        position.y = rect.rect.height * 0.5f;
        rect.anchoredPosition = position;
    }
}
