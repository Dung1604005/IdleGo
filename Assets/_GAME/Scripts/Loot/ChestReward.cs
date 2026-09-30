using System;
using UnityEngine;

[Serializable]
public class ChestReward
{
    [SerializeField] private ChestType chestType;
    [SerializeField] private Equipment equipment;

    public ChestReward(ChestType type, Equipment rewardEquipment)
    {
        chestType = type;
        equipment = rewardEquipment;
    }

    public ChestType ChestType => chestType;
    public Equipment Equipment => equipment;
    public bool IsValid => equipment != null && equipment.Data != null;
}
