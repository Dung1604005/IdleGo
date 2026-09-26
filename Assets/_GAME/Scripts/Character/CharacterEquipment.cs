using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterEquipment
{
    [SerializeField] private List<Equipment> listEquipment =
        new List<Equipment>(EquipmentTypeUtility.EquipmentTypeCount);

    [NonSerialized] private Character character;
    [NonSerialized] private List<StatModifier> modifierBuffer;
    [NonSerialized] private List<IStatModifierSource> appliedModifierSources;
    [NonSerialized] private List<IStatModifierSource> currentModifierSources;

    public IReadOnlyList<Equipment> EquippedItems =>
        listEquipment ?? (IReadOnlyList<Equipment>)Array.Empty<Equipment>();

    public void OnInit(Character owner)
    {
        EnsureRuntimeState();
        character = owner;
        ClaimCurrentEquipment();
        ApplyStats();
    }

    public void OnDespawn()
    {
        EnsureRuntimeState();

        if (character != null)
        {
            character.Stats.ReplaceModifiersFromSources(
                appliedModifierSources,
                Array.Empty<StatModifier>()
            );
        }

        appliedModifierSources.Clear();
        character = null;
    }

    public bool CanEquip(Equipment equipment)
    {
        return character != null
            && equipment != null
            && equipment.Data != null
            && EquipmentTypeUtility.IsValid(equipment.EquipmentType)
            && equipment.CanBeEquippedBy(this)
            && character.Stats.CurrentLevel >= equipment.Data.LevelRequired;
    }

    public bool Equip(Equipment equipment)
    {
        return TryEquip(equipment, out _);
    }

    public bool TryEquip(Equipment equipment, out Equipment replacedEquipment)
    {
        replacedEquipment = null;
        if (!CanEquip(equipment))
        {
            return false;
        }

        EquipmentType equipmentType = equipment.EquipmentType;
        Equipment equippedItem = GetEquipment(equipmentType);
        if (ReferenceEquals(equippedItem, equipment))
        {
            return true;
        }

        replacedEquipment = equippedItem;
        equippedItem?.SetEquippedBy(null);

        SetEquipment(equipmentType, equipment);
        equipment.SetEquippedBy(this);
        ApplyStats();
        return true;
    }

    public Equipment Unequip(EquipmentType equipmentType)
    {
        if (!EquipmentTypeUtility.IsValid(equipmentType))
        {
            return null;
        }

        Equipment equipment = GetEquipment(equipmentType);
        if (equipment == null)
        {
            return null;
        }

        for (int i = listEquipment.Count - 1; i >= 0; i--)
        {
            Equipment item = listEquipment[i];
            if (item == null || item.Data == null || item.EquipmentType != equipmentType)
            {
                continue;
            }

            if (ReferenceEquals(item.EquippedBy, this))
            {
                item.SetEquippedBy(null);
            }
            listEquipment.RemoveAt(i);
        }

        ApplyStats();
        return equipment;
    }

    public bool Unequip(Equipment equipment)
    {
        if (equipment == null || !ReferenceEquals(GetEquipment(equipment.EquipmentType), equipment))
        {
            return false;
        }

        return Unequip(equipment.EquipmentType) != null;
    }

    public void UnequipAll()
    {
        EnsureRuntimeState();
        for (int i = 0; i < listEquipment.Count; i++)
        {
            Equipment equipment = listEquipment[i];
            if (equipment != null && ReferenceEquals(equipment.EquippedBy, this))
            {
                equipment.SetEquippedBy(null);
            }
        }

        listEquipment.Clear();
        ApplyStats();
    }

    public Equipment GetEquipment(EquipmentType equipmentType)
    {
        if (!EquipmentTypeUtility.IsValid(equipmentType) || listEquipment == null)
        {
            return null;
        }

        for (int i = 0; i < listEquipment.Count; i++)
        {
            Equipment equipment = listEquipment[i];
            if (equipment != null
                && equipment.Data != null
                && equipment.EquipmentType == equipmentType)
            {
                return equipment;
            }
        }

        return null;
    }

    public void ApplyStats()
    {
        if (character == null)
        {
            return;
        }

        EnsureRuntimeState();
        modifierBuffer.Clear();
        currentModifierSources.Clear();

        for (int equipmentIndex = 0;
             equipmentIndex < EquipmentTypeUtility.EquipmentTypeCount;
             equipmentIndex++)
        {
            Equipment equipment = GetEquipment((EquipmentType)equipmentIndex);
            if (equipment == null || !equipment.CanBeEquippedBy(this))
            {
                continue;
            }

            equipment.SetEquippedBy(this);
            equipment.CollectStatModifiers(modifierBuffer);
            currentModifierSources.Add(equipment);
        }

        character.Stats.ReplaceModifiersFromSources(appliedModifierSources, modifierBuffer);

        appliedModifierSources.Clear();
        for (int i = 0; i < currentModifierSources.Count; i++)
        {
            appliedModifierSources.Add(currentModifierSources[i]);
        }
    }

    public void ApplyStat()
    {
        ApplyStats();
    }

    private void SetEquipment(EquipmentType equipmentType, Equipment equipment)
    {
        for (int i = listEquipment.Count - 1; i >= 0; i--)
        {
            Equipment item = listEquipment[i];
            if (item == null || item.Data == null || item.EquipmentType != equipmentType)
            {
                continue;
            }

            if (ReferenceEquals(item.EquippedBy, this))
            {
                item.SetEquippedBy(null);
            }
            listEquipment.RemoveAt(i);
        }

        listEquipment.Add(equipment);
    }

    private void ClaimCurrentEquipment()
    {
        for (int i = 0; i < listEquipment.Count; i++)
        {
            Equipment equipment = listEquipment[i];
            if (equipment != null && ReferenceEquals(equipment.EquippedBy, this))
            {
                equipment.SetEquippedBy(null);
            }
        }

        for (int equipmentIndex = 0;
             equipmentIndex < EquipmentTypeUtility.EquipmentTypeCount;
             equipmentIndex++)
        {
            Equipment equipment = GetEquipment((EquipmentType)equipmentIndex);
            if (equipment == null)
            {
                continue;
            }

            equipment.EnsureRuntimeState();
            if (equipment.CanBeEquippedBy(this))
            {
                equipment.SetEquippedBy(this);
            }
            else
            {
                Debug.LogWarning($"Equipment {equipment.InstanceId} is already equipped by another character.");
            }
        }
    }

    private void EnsureRuntimeState()
    {
        if (listEquipment == null)
        {
            listEquipment = new List<Equipment>(EquipmentTypeUtility.EquipmentTypeCount);
        }

        for (int i = listEquipment.Count - 1; i >= 0; i--)
        {
            if (listEquipment[i] == null)
            {
                listEquipment.RemoveAt(i);
            }
        }

        if (modifierBuffer == null)
        {
            modifierBuffer = new List<StatModifier>();
        }

        if (appliedModifierSources == null)
        {
            appliedModifierSources = new List<IStatModifierSource>();
        }

        if (currentModifierSources == null)
        {
            currentModifierSources = new List<IStatModifierSource>();
        }
    }
}
