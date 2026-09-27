using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterEquipment
{
    [SerializeField] private EquipmentData equipmentData = new EquipmentData();

    [NonSerialized] private Character character;
    [NonSerialized] private EquipmentStatHandler statHandler;
    [NonSerialized] private int dataVersion;

    public EquipmentData Data => equipmentData;
    public IReadOnlyList<Equipment> EquippedItems => equipmentData.Equipments;
    public int DataVersion => dataVersion;
    public bool IsInitialized { get; private set; }

    public void OnInit(Character owner)
    {
        character = owner;
        equipmentData ??= new EquipmentData();
        equipmentData.OnInit();
        ClaimCurrentEquipment();

        statHandler = new EquipmentStatHandler(character, equipmentData, this);
        dataVersion = 0;
        IsInitialized = true;
        RefreshCharacterAndView();
    }

    public void OnDespawn()
    {
        statHandler?.Clear();
        statHandler = null;
        character = null;
        IsInitialized = false;
    }

    public bool CanEquip(Equipment equipment)
    {
        return IsInitialized
            && character != null
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

        Equipment currentEquipment = equipmentData.GetEquipment(equipment.EquipmentType);
        if (ReferenceEquals(currentEquipment, equipment))
        {
            return true;
        }

        // Data là nguồn chính: thay ô trước, sau đó mới cập nhật ownership, stat và phiên bản UI.
        replacedEquipment = equipmentData.SetEquipment(equipment);
        ReleaseOwnership(replacedEquipment);
        equipment.SetEquippedBy(this);
        RefreshCharacterAndView();
        return true;
    }

    public Equipment Unequip(EquipmentType equipmentType)
    {
        if (!IsInitialized || !EquipmentTypeUtility.IsValid(equipmentType))
        {
            return null;
        }

        Equipment removedEquipment = equipmentData.RemoveEquipment(equipmentType);
        if (removedEquipment == null)
        {
            return null;
        }

        ReleaseOwnership(removedEquipment);
        RefreshCharacterAndView();
        return removedEquipment;
    }

    public bool Unequip(Equipment equipment)
    {
        if (equipment == null
            || !ReferenceEquals(GetEquipment(equipment.EquipmentType), equipment))
        {
            return false;
        }

        return Unequip(equipment.EquipmentType) != null;
    }

    public void UnequipAll()
    {
        IReadOnlyList<Equipment> equipments = equipmentData.Equipments;
        for (int i = 0; i < equipments.Count; i++)
        {
            ReleaseOwnership(equipments[i]);
        }

        equipmentData.Clear();
        RefreshCharacterAndView();
    }

    public Equipment GetEquipment(EquipmentType equipmentType)
    {
        return equipmentData.GetEquipment(equipmentType);
    }

    public void ApplyStats()
    {
        statHandler?.Apply();
    }

    public void ApplyStat()
    {
        ApplyStats();
    }

    internal void OnEquipmentDataChanged(Equipment equipment)
    {
        if (!IsInitialized
            || equipment == null
            || !ReferenceEquals(GetEquipment(equipment.EquipmentType), equipment))
        {
            return;
        }

        RefreshCharacterAndView();
    }

    private void RefreshCharacterAndView()
    {
        // Character đọc EquipmentData mới trước; UI chỉ đọc lại sau khi DataVersion tăng.
        statHandler?.Apply();
        dataVersion++;
    }

    private void ClaimCurrentEquipment()
    {
        IReadOnlyList<Equipment> equipments = equipmentData.Equipments;
        for (int i = 0; i < equipments.Count; i++)
        {
            Equipment equipment = equipments[i];
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
                Debug.LogWarning(
                    $"Equipment {equipment.InstanceId} is already equipped by another character."
                );
            }
        }
    }

    private void ReleaseOwnership(Equipment equipment)
    {
        if (equipment != null && ReferenceEquals(equipment.EquippedBy, this))
        {
            equipment.SetEquippedBy(null);
        }
    }
}
