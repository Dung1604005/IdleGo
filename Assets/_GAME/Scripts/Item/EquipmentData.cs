using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentData
{
    [SerializeReference] private List<Equipment> equipments =
        new List<Equipment>(EquipmentTypeUtility.EquipmentTypeCount);

    public IReadOnlyList<Equipment> Equipments => equipments;

    public void OnInit()
    {
        NormalizeSlots();
    }

    public Equipment GetEquipment(EquipmentType equipmentType)
    {
        if (!EquipmentTypeUtility.IsValid(equipmentType))
        {
            return null;
        }

        EnsureSlotCount();
        return equipments[(int)equipmentType];
    }

    internal Equipment SetEquipment(Equipment equipment)
    {
        if (equipment == null || !EquipmentTypeUtility.IsValid(equipment.EquipmentType))
        {
            return null;
        }

        EnsureSlotCount();
        int equipmentIndex = (int)equipment.EquipmentType;
        Equipment replacedEquipment = equipments[equipmentIndex];
        equipments[equipmentIndex] = equipment;

        
        return replacedEquipment;
    }

    internal Equipment RemoveEquipment(EquipmentType equipmentType)
    {
        if (!EquipmentTypeUtility.IsValid(equipmentType))
        {
            return null;
        }

        EnsureSlotCount();
        int equipmentIndex = (int)equipmentType;
        Equipment removedEquipment = equipments[equipmentIndex];
        equipments[equipmentIndex] = null;
        return removedEquipment;
    }

    internal void Clear()
    {
        EnsureSlotCount();
        for (int i = 0; i < equipments.Count; i++)
        {
            equipments[i] = null;
        }
    }

    private void NormalizeSlots()
    {
        List<Equipment> normalizedEquipments =
            new List<Equipment>(EquipmentTypeUtility.EquipmentTypeCount);
        for (int i = 0; i < EquipmentTypeUtility.EquipmentTypeCount; i++)
        {
            normalizedEquipments.Add(null);
        }

        if (equipments != null)
        {
            for (int i = 0; i < equipments.Count; i++)
            {
                Equipment equipment = equipments[i];
                if (equipment == null || !EquipmentTypeUtility.IsValid(equipment.EquipmentType))
                {
                    continue;
                }

                // EquipmentType quyết định ô chứa, thứ tự list cũ không ảnh hưởng kết quả.
                normalizedEquipments[(int)equipment.EquipmentType] = equipment;
            }
        }

        equipments = normalizedEquipments;
    }

    private void EnsureSlotCount()
    {
        if (equipments == null || equipments.Count != EquipmentTypeUtility.EquipmentTypeCount)
        {
            NormalizeSlots();
        }
    }
}
