using UnityEngine;

public class CanvasItemInfomationUI : UICanvas
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
        Equipment selectedEquipment = DataManager.Ins.SelectedEquipment;
        Equipment equippedEquipment = GetEquippedComparison(selectedEquipment);
        selectedItemPanel?.OnInit(selectedEquipment);
        equippedComparisonPanel?.OnInit(equippedEquipment);
        RefreshPanelPositions(equippedEquipment?.Data != null);
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
        if (selectedEquipment?.Data == null)
        {
            return null;
        }

        // Equipped panel chi doc dung slot cung loai cua hero dang duoc chon.
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
        rect.anchoredPosition = position;
    }
}
