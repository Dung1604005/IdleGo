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
        Equipment selectedEquipment = DataManager.Ins.SelectedEquipment;
        selectedItemPanel?.OnInit(selectedEquipment);
        equippedComparisonPanel?.OnInit(
            GetEquippedComparison(selectedEquipment));
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
}
