using System.Collections.Generic;
using UnityEngine;

public class InventoryEquipmentHandler
{
    private readonly InventoryStorage storage;
    private readonly CharacterEquipment characterEquipment;

    public InventoryEquipmentHandler(
        InventoryStorage inventoryStorage,
        CharacterEquipment ownerEquipment)
    {
        storage = inventoryStorage;
        characterEquipment = ownerEquipment;
    }

    public bool Equip(Item item)
    {
        return characterEquipment != null
            && item is Equipment equipment
            && storage.GetSlot(item) != null
            && characterEquipment.TryEquip(equipment, out _);
    }

    public bool Unequip(Item item)
    {
        return characterEquipment != null
            && item is Equipment equipment
            && storage.GetSlot(item) != null
            && characterEquipment.Unequip(equipment);
    }

    public bool Unequip(EquipmentType equipmentType)
    {
        return characterEquipment != null
            && characterEquipment.Unequip(equipmentType) != null;
    }

    public void UnequipBeforeRemoving(Item item)
    {
        if (item is Equipment equipment && equipment.IsEquipped)
        {
            characterEquipment?.Unequip(equipment);
        }
    }

    public void ImportCurrentEquipment(Inventory owner)
    {
        if (characterEquipment == null)
        {
            return;
        }

        List<Equipment> currentEquipment = new List<Equipment>(characterEquipment.EquippedItems);
        for (int i = 0; i < currentEquipment.Count; i++)
        {
            Equipment equipment = currentEquipment[i];
            if (equipment == null || storage.GetSlot(equipment) != null)
            {
                continue;
            }

            InventorySlot emptySlot = storage.GetFirstEmptySlot();
            if (emptySlot == null)
            {
                Debug.LogWarning("Inventory does not have room for the character's starting equipment.");
                return;
            }

            emptySlot.AddItem(equipment, 1);
            equipment.SetInventory(owner);
        }
    }

    public void RestoreEquippedItems(IReadOnlyList<string> equippedInstanceIds)
    {
        if (characterEquipment == null)
        {
            return;
        }

        // Save giữ InstanceId; tháo trạng thái cũ rồi nối lại đúng instance vừa được load.
        characterEquipment.UnequipAll();
        if (equippedInstanceIds == null)
        {
            return;
        }

        for (int i = 0; i < equippedInstanceIds.Count; i++)
        {
            Item item = storage.GetItemByInstanceId(equippedInstanceIds[i]);
            if (item is Equipment equipment)
            {
                characterEquipment.Equip(equipment);
            }
        }
    }
}
