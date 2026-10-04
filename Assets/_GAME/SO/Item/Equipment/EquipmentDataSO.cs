using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "IdleGo/Item/Equipment/Equipment Data")]
public class EquipmentDataSO : ItemSO
{
    [SerializeField] private EquipmentType equipmentType;

    [Header("Requirement")]
    [SerializeField, Min(1)] private int levelRequired = 1;
    [SerializeField] private CharacterRequirementType characterRequirement =
        CharacterRequirementType.ALL;

    [Header("Slots")]
    [SerializeField, Min(0)] private int socketSlotCount;
    [SerializeField, Min(0)] private int enchantmentSlotCount;
    [SerializeField, Min(0)] private int decorationSlotCount;

    [Header("Stats")]
    [SerializeField] private List<StatValue> stats = new List<StatValue>();
    [SerializeField, Min(0.01f)] private float attacksPerSecond = 1f;

    public EquipmentType EquipmentType => equipmentType;
    public int LevelRequired => Mathf.Max(1, levelRequired);
    public CharacterRequirementType CharacterRequirement => characterRequirement;
    public int SocketSlotCount => Mathf.Max(0, socketSlotCount);
    public int EnchantmentSlotCount => Mathf.Max(0, enchantmentSlotCount);
    public int DecorationSlotCount => Mathf.Max(0, decorationSlotCount);
    public IReadOnlyList<StatValue> Stats => stats;
    public bool IsWeapon => equipmentType == EquipmentType.MAIN_WEAPON
        || equipmentType == EquipmentType.OFF_HAND_WEAPON;
    public float AttacksPerSecond => IsWeapon
        ? Mathf.Max(0.01f, attacksPerSecond)
        : 0f;

    public EquipmentMainStats GetMainStats()
    {
        return new EquipmentMainStats(
            GetMainStat(),
            IsWeapon,
            AttacksPerSecond);
    }

    public StatValue GetMainStat()
    {
        // Tool balance luon ghi main stat o index 0.
        return stats != null && stats.Count > 0 ? stats[0] : null;
    }

    public IReadOnlyList<StatValue> GetSubStats()
    {
        if (stats == null || stats.Count <= 1)
        {
            return Array.Empty<StatValue>();
        }

        List<StatValue> subStats = new List<StatValue>(stats.Count - 1);
        for (int i = 1; i < stats.Count; i++)
        {
            if (stats[i] != null)
            {
                subStats.Add(stats[i]);
            }
        }
        return subStats;
    }

    public bool CanEquip(Character character)
    {
        return character != null
            && character.Stats.CurrentLevel >= LevelRequired
            && CharacterTypeUtility.Matches(characterRequirement, character.CharacterType);
    }

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

public sealed class EquipmentMainStats
{
    public StatValue MainStat { get; }
    public bool HasAttacksPerSecond { get; }
    public float AttacksPerSecond { get; }

    public EquipmentMainStats(
        StatValue mainStat,
        bool hasAttacksPerSecond,
        float attacksPerSecond)
    {
        MainStat = mainStat;
        HasAttacksPerSecond = hasAttacksPerSecond;
        AttacksPerSecond = hasAttacksPerSecond ? attacksPerSecond : 0f;
    }
}
