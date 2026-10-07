public partial class DataManager
{
    public bool CanApplySelectedEnchantMaterial(Equipment targetEquipment)
    {
        return selectedItem is EnchantMaterial material
            && inventoryData != null
            && inventoryData.CanApplyEnchantMaterial(material, targetEquipment);
    }

    public bool ApplySelectedEnchantMaterial(Equipment targetEquipment)
    {
        if (selectedItem is not EnchantMaterial material
            || inventoryData == null)
        {
            return false;
        }

        InventorySlot materialSlot = inventoryData.GetSlot(material);
        bool consumesLastMaterial = materialSlot != null && materialSlot.Amount == 1;
        if (consumesLastMaterial)
        {
            // Xoa selection truoc khi Inventory refresh de UI khong hien item da bi tieu thu.
            ClearSelectedItem();
        }

        if (inventoryData.ApplyEnchantMaterial(material, targetEquipment))
        {
            return true;
        }

        if (consumesLastMaterial)
        {
            SetSelectedItem(material);
        }
        return false;
    }
}
