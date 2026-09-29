using System.Collections.Generic;
using UnityEngine;

public partial class Equipment
{
    [SerializeField] private float qualityRoll = 1f;

    public float QualityRoll => NormalizeQualityRoll(qualityRoll);

    public void SetQualityRoll(float value)
    {
        float normalizedValue = NormalizeQualityRoll(value);
        if (Mathf.Approximately(QualityRoll, normalizedValue))
        {
            return;
        }

        qualityRoll = normalizedValue;
        equippedBy?.OnEquipmentDataChanged(this);
        NotifyInventoryDataChanged();
    }

    internal void RestoreQualityRoll(float savedQualityRoll)
    {
        // Save cu khong co field nay se doc ra 0; quy ve 1 de giu nguyen suc manh cu.
        qualityRoll = savedQualityRoll > 0f ? savedQualityRoll : 1f;
    }

    private float GetRolledBaseStatValue(
        StatType statType,
        StatModifierOperation operation)
    {
        return Data.GetStatValue(statType, operation) * QualityRoll;
    }

    private void AddRolledBaseStatModifiers(List<StatModifier> output)
    {
        IReadOnlyList<StatValue> baseStats = Data.Stats;
        if (baseStats == null)
        {
            return;
        }

        for (int i = 0; i < baseStats.Count; i++)
        {
            StatValue stat = baseStats[i];
            if (stat == null || !StatTypeUtility.CanHaveModifiers(stat.StatType))
            {
                continue;
            }

            output.Add(new StatModifier(
                this,
                stat.StatType,
                stat.Value * QualityRoll,
                stat.Operation));
        }
    }

    private static float NormalizeQualityRoll(float value)
    {
        return Mathf.Max(0.01f, value);
    }
}
