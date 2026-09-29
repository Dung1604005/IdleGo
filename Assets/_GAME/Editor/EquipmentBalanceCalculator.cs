using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal sealed class EquipmentBalanceStatResult
{
    public StatType StatType { get; }
    public StatModifierOperation Operation { get; }
    public float Value { get; }
    public float AllocatedPower { get; }

    public EquipmentBalanceStatResult(
        StatType statType,
        StatModifierOperation operation,
        float value,
        float allocatedPower)
    {
        StatType = statType;
        Operation = operation;
        Value = value;
        AllocatedPower = allocatedPower;
    }
}

internal sealed class EquipmentBalancePreview
{
    public readonly List<EquipmentBalanceStatResult> Stats =
        new List<EquipmentBalanceStatResult>();

    public float LevelPower { get; set; }
    public float ItemPower { get; set; }
    public float BaseStatBudget { get; set; }
    public float EnhancementBudget { get; set; }
    public string Error { get; set; }
    public bool IsValid => string.IsNullOrEmpty(Error);
}

internal static class EquipmentBalanceCalculator
{
    public static EquipmentBalancePreview BuildPreview(
        EquipmentDataSO equipment,
        StatType mainStat,
        IReadOnlyList<StatType> subStats,
        float rollMultiplier)
    {
        EquipmentBalancePreview preview = new EquipmentBalancePreview();
        preview.Error = Validate(equipment, mainStat, subStats);
        if (!preview.IsValid)
        {
            return preview;
        }

        preview.LevelPower = EquipmentBalanceRules.GetLevelPower(equipment.LevelRequired);
        preview.ItemPower = preview.LevelPower
            * EquipmentBalanceRules.GetRarityMultiplier(equipment.RarityType)
            * EquipmentBalanceRules.GetSlotMultiplier(equipment.EquipmentType)
            * Mathf.Clamp(rollMultiplier, EquipmentBalanceRules.MinRoll,
                EquipmentBalanceRules.MaxRoll);
        preview.BaseStatBudget = preview.ItemPower
            * EquipmentBalanceRules.GetBaseBudgetShare(equipment.RarityType);
        preview.EnhancementBudget = preview.ItemPower - preview.BaseStatBudget;
        AllocateStats(preview, equipment.RarityType, mainStat, subStats);
        return preview;
    }

    public static void ApplyToAsset(
        EquipmentDataSO equipment,
        EquipmentBalancePreview preview)
    {
        if (equipment == null || preview == null || !preview.IsValid)
        {
            return;
        }

        Undo.RecordObject(equipment, "Generate balanced equipment stats");
        SerializedObject serializedEquipment = new SerializedObject(equipment);
        SerializedProperty statsProperty = serializedEquipment.FindProperty("stats");
        statsProperty.arraySize = preview.Stats.Count;
        for (int i = 0; i < preview.Stats.Count; i++)
        {
            WriteStat(statsProperty.GetArrayElementAtIndex(i), preview.Stats[i]);
        }

        serializedEquipment.ApplyModifiedProperties();
        EditorUtility.SetDirty(equipment);
        AssetDatabase.SaveAssets();
    }

    private static string Validate(
        EquipmentDataSO equipment,
        StatType mainStat,
        IReadOnlyList<StatType> subStats)
    {
        if (equipment == null)
        {
            return "Hãy chọn một EquipmentDataSO.";
        }

        if (!IsAvailableStat(mainStat))
        {
            return "Main stat không hợp lệ cho equipment.";
        }

        int requiredCount =
            EquipmentBalanceRules.GetRequiredSubStatCount(equipment.RarityType);
        if (subStats == null || subStats.Count != requiredCount)
        {
            return $"Rarity {equipment.RarityType} cần đúng {requiredCount} sub stat.";
        }

        return ValidateSubStats(mainStat, subStats);
    }

    private static string ValidateSubStats(
        StatType mainStat,
        IReadOnlyList<StatType> subStats)
    {
        HashSet<StatType> uniqueStats = new HashSet<StatType> { mainStat };
        for (int i = 0; i < subStats.Count; i++)
        {
            if (!IsAvailableStat(subStats[i]))
            {
                return "Danh sách có stat không được phép.";
            }

            if (!uniqueStats.Add(subStats[i]))
            {
                return "Main stat và sub stat không được trùng nhau.";
            }
        }

        return string.Empty;
    }

    private static void AllocateStats(
        EquipmentBalancePreview preview,
        RarityType rarity,
        StatType mainStat,
        IReadOnlyList<StatType> subStats)
    {
        float mainShare = subStats.Count > 0
            ? EquipmentBalanceRules.GetMainBudgetShare(rarity)
            : 1f;
        float mainPower = preview.BaseStatBudget * mainShare;
        AddStat(preview, mainStat, mainPower);

        float subPower = subStats.Count > 0
            ? (preview.BaseStatBudget - mainPower) / subStats.Count
            : 0f;
        for (int i = 0; i < subStats.Count; i++)
        {
            AddStat(preview, subStats[i], subPower);
        }
    }

    private static void AddStat(
        EquipmentBalancePreview preview,
        StatType statType,
        float allocatedPower)
    {
        float value = EquipmentBalanceRules.ConvertPowerToValue(
            statType,
            allocatedPower,
            preview.LevelPower);
        preview.Stats.Add(new EquipmentBalanceStatResult(
            statType,
            EquipmentBalanceRules.GetOperation(statType),
            value,
            allocatedPower));
    }

    private static void WriteStat(
        SerializedProperty statProperty,
        EquipmentBalanceStatResult result)
    {
        statProperty.FindPropertyRelative("statType").enumValueIndex =
            (int)result.StatType;
        statProperty.FindPropertyRelative("value").floatValue = result.Value;
        statProperty.FindPropertyRelative("operation").enumValueIndex =
            (int)result.Operation;
    }

    private static bool IsAvailableStat(StatType statType)
    {
        StatType[] availableStats = EquipmentBalanceRules.AvailableStats;
        for (int i = 0; i < availableStats.Length; i++)
        {
            if (availableStats[i] == statType)
            {
                return true;
            }
        }

        return false;
    }
}
