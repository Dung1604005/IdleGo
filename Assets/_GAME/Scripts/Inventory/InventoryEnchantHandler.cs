public sealed class InventoryEnchantHandler
{
    private readonly InventoryStorage storage;

    public InventoryEnchantHandler(InventoryStorage inventoryStorage)
    {
        storage = inventoryStorage;
    }

    public bool Apply(EnchantMaterial material, Equipment targetEquipment)
    {
        if (!CanApply(material, targetEquipment, out InventorySlot materialSlot)
            || !material.TryRollStat(out StatValue rolledStat))
        {
            return false;
        }

        BuffStatType buffStatType = material.Data.BuffStatType;
        if (!targetEquipment.TryApplyEnchantStat(
            buffStatType,
            rolledStat,
            material.RarityType))
        {
            return false;
        }

        // Chi tru material sau khi stat da duoc gan thanh cong.
        materialSlot.RemoveItem(1);
        if (materialSlot.IsEmpty)
        {
            material.SetInventory(null);
        }
        return true;
    }

    public bool CanApply(
        EnchantMaterial material,
        Equipment targetEquipment,
        out InventorySlot materialSlot)
    {
        materialSlot = storage?.GetSlot(material);
        return material?.Data != null
            && targetEquipment?.Data != null
            && materialSlot != null
            && !materialSlot.IsEmpty
            && targetEquipment.HasEmptyBuffSlot(material.Data.BuffStatType);
    }
}
