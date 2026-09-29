using System;
using UnityEngine;

[Serializable]
public class EquipmentDropEntry
{
    [SerializeField] private EquipmentDataSO equipment;
    [SerializeField, Min(0f)] private float dropRate = 1f;

    public EquipmentDataSO Equipment => equipment;
    public float DropRate => Mathf.Max(0f, dropRate);

    public bool CanRoll(int sourceLevel)
    {
        return equipment != null
            && equipment.LevelRequired == sourceLevel
            && DropRate > 0f;
    }
}
