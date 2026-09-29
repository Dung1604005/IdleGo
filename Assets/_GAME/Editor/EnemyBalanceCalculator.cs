using UnityEditor;
using UnityEngine;

internal sealed class EnemyBalancePreview
{
    private readonly float[] stats = new float[StatTypeUtility.StatCount];

    public int Level { get; set; }
    public float ProgressionMultiplier { get; set; }
    public float Adjustment { get; set; }

    public float GetStat(StatType statType)
    {
        return stats[(int)statType];
    }

    public void SetStat(StatType statType, float value)
    {
        stats[(int)statType] = StatTypeUtility.NormalizeValue(statType, value);
    }
}

internal static class EnemyBalanceCalculator
{
    public static EnemyBalancePreview Build(
        int level,
        EnemyType enemyType,
        float adjustment)
    {
        int normalizedLevel = Mathf.Max(1, level);
        float normalizedAdjustment = Mathf.Clamp(
            adjustment,
            EnemyBalanceRules.MinAdjustment,
            EnemyBalanceRules.MaxAdjustment);
        float progression = EnemyBalanceRules.GetProgressionMultiplier(normalizedLevel);
        EnemyTypeBalance type = EnemyBalanceRules.GetTypeBalance(enemyType);

        EnemyBalancePreview result = new EnemyBalancePreview
        {
            Level = normalizedLevel,
            ProgressionMultiplier = progression,
            Adjustment = normalizedAdjustment
        };
        SetCoreStats(result, type, progression, normalizedAdjustment);
        SetStableStats(result, type);
        return result;
    }

    public static void Apply(
        CharacterStatSO statData,
        EnemyDataSO enemyData,
        EnemyType enemyType,
        EnemyBalancePreview preview)
    {
        if (statData == null || preview == null)
        {
            return;
        }

        Undo.RecordObject(statData, "Generate enemy base stats");
        WriteStats(statData, preview);
        if (enemyData != null)
        {
            WriteEnemyType(enemyData, enemyType);
        }

        AssetDatabase.SaveAssets();
    }

    private static void SetCoreStats(
        EnemyBalancePreview result,
        EnemyTypeBalance type,
        float progression,
        float adjustment)
    {
        result.SetStat(StatType.MAX_HEALTH,
            Mathf.Round(EnemyBalanceRules.BaseHealth * progression * type.Health * adjustment));
        result.SetStat(StatType.DAMAGE,
            Mathf.Round(EnemyBalanceRules.BaseDamage * progression * type.Damage * adjustment));
        result.SetStat(StatType.ARMOR,
            Round(EnemyBalanceRules.GetBaseArmor(result.Level) * type.Armor * adjustment, 1));
        result.SetStat(StatType.EXPERIENCE,
            Mathf.Round(EnemyBalanceRules.BaseExperience * progression
                * type.Experience * adjustment));
        result.SetStat(StatType.LEVEL, result.Level);
    }

    private static void SetStableStats(
        EnemyBalancePreview result,
        EnemyTypeBalance type)
    {
        result.SetStat(StatType.RUN_SPEED, Round(5f * type.RunSpeed, 2));
        result.SetStat(StatType.ATTACK_SPEED, Round(type.AttackSpeed, 2));
        result.SetStat(StatType.CRITICAL_CHANCE, type.CriticalChance);
        result.SetStat(StatType.CRITICAL_DAMAGE, type.CriticalDamage);
        result.SetStat(StatType.DODGE_CHANCE, type.DodgeChance);
        result.SetStat(StatType.COOLDOWN_REDUCTION, 0f);
        result.SetStat(StatType.LIFE_STEAL, 0f);
        result.SetStat(StatType.DAMAGE_AMPLIFICATION, 0f);
    }

    private static void WriteStats(CharacterStatSO statData, EnemyBalancePreview preview)
    {
        SerializedObject serializedStats = new SerializedObject(statData);
        SerializedProperty statsProperty = serializedStats.FindProperty("baseStats");
        statsProperty.arraySize = StatTypeUtility.StatCount;
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            statsProperty.GetArrayElementAtIndex(i).floatValue =
                preview.GetStat((StatType)i);
        }

        serializedStats.ApplyModifiedProperties();
        EditorUtility.SetDirty(statData);
    }

    private static void WriteEnemyType(EnemyDataSO enemyData, EnemyType enemyType)
    {
        Undo.RecordObject(enemyData, "Set enemy type");
        SerializedObject serializedEnemy = new SerializedObject(enemyData);
        serializedEnemy.FindProperty("enemyType").enumValueIndex = (int)enemyType;
        serializedEnemy.ApplyModifiedProperties();
        EditorUtility.SetDirty(enemyData);
    }

    private static float Round(float value, int digits)
    {
        float multiplier = Mathf.Pow(10f, digits);
        return Mathf.Round(value * multiplier) / multiplier;
    }
}
