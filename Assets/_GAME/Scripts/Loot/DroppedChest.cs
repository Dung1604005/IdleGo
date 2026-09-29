using System;
using UnityEngine;

[Serializable]
public class DroppedChest
{
    [SerializeField] private ChestType chestType;
    [SerializeField] private EquipmentSourceSO equipmentSource;

    public DroppedChest(ChestType type, EquipmentSourceSO source)
    {
        chestType = type;
        equipmentSource = source;
    }

    public ChestType ChestType => chestType;
    public EquipmentSourceSO EquipmentSource => equipmentSource;

    public bool TryCreateEquipment(out Equipment equipment)
    {
        equipment = null;
        if (equipmentSource == null
            || !equipmentSource.TryRollEquipment(out EquipmentDataSO data))
        {
            return false;
        }

        // Quality Roll thuoc instance; EquipmentDataSO va cac main/sub stat van giu nguyen.
        equipment = new Equipment(data);
        equipment.SetQualityRoll(EquipmentQualityRoll.Roll(data.RarityType));
        return true;
    }
}
