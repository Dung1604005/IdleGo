using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "IdleGo/Item/Equipment/Equipment Data")]
public class EquipmentDataSO : ItemSO
{
    [SerializeField] private EquipmentType equipmentType;

    [Header("Requirement")]
    [SerializeField, Min(1)] private int levelRequired = 1;

    [Header("Slots")]
    [SerializeField, Min(0)] private int socketSlotCount;
    [SerializeField, Min(0)] private int enchantmentSlotCount;
    [SerializeField, Min(0)] private int decorationSlotCount;

    [Header("Stats")]
    [SerializeField] private List<StatValue> stats = new List<StatValue>();

    public EquipmentType EquipmentType => equipmentType;
    public int LevelRequired => Mathf.Max(1, levelRequired);
    public int SocketSlotCount => Mathf.Max(0, socketSlotCount);
    public int EnchantmentSlotCount => Mathf.Max(0, enchantmentSlotCount);
    public int DecorationSlotCount => Mathf.Max(0, decorationSlotCount);
    public IReadOnlyList<StatValue> Stats => stats;

    public float GetStatValue(
        StatType statType,
        StatModifierOperation operation = StatModifierOperation.FLAT)
    {
        if (stats == null)
        {
            return 0f;
        }

        float totalValue = 0f;
        for (int i = 0; i < stats.Count; i++)
        {
            StatValue stat = stats[i];
            if (stat != null && stat.StatType == statType && stat.Operation == operation)
            {
                totalValue += stat.Value;
            }
        }

        return totalValue;
    }
}
