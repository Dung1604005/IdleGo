using System.Collections.Generic;

public partial class EquipmentStatsContentUI
{
    public void Refresh(EnchantMaterialDataSO materialData)
    {
        ClearContent();
        IReadOnlyList<StatValue> possibleStats = materialData?.PossibleStats;
        int validStatCount = CountValidStats(possibleStats);
        SetSeparator(mainToSubStatLine, validStatCount > 0, 0);

        int siblingIndex = validStatCount > 0 ? 1 : 0;
        for (int i = 0; possibleStats != null && i < possibleStats.Count; i++)
        {
            StatValue stat = possibleStats[i];
            if (stat == null || !StatTypeUtility.CanHaveModifiers(stat.StatType))
            {
                continue;
            }

            StatLineUI line = SpawnStatLine(siblingIndex++);
            line?.OnInitEnhancement(stat, materialData.RarityType);
        }
        ResizePanel();
    }

    private static int CountValidStats(IReadOnlyList<StatValue> stats)
    {
        int count = 0;
        for (int i = 0; stats != null && i < stats.Count; i++)
        {
            if (stats[i] != null
                && StatTypeUtility.CanHaveModifiers(stats[i].StatType))
            {
                count++;
            }
        }
        return count;
    }
}
