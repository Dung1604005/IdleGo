using System;
using System.Collections.Generic;

public partial class Equipment
{
    public EquipmentMainStats GetMainStats()
    {
        EquipmentMainStats source = Data?.GetMainStats();
        return source == null
            ? null
            : new EquipmentMainStats(
                CreateRolledStat(source.MainStat),
                source.HasAttacksPerSecond,
                source.AttacksPerSecond);
    }

    public StatValue GetMainStat()
    {
        return CreateRolledStat(Data?.GetMainStat());
    }

    public IReadOnlyList<StatValue> GetSubStats()
    {
        IReadOnlyList<StatValue> source = Data?.GetSubStats();
        if (source == null || source.Count == 0)
        {
            return Array.Empty<StatValue>();
        }

        List<StatValue> result = new List<StatValue>(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            StatValue rolledStat = CreateRolledStat(source[i]);
            if (rolledStat != null)
            {
                result.Add(rolledStat);
            }
        }
        return result;
    }

    private StatValue CreateRolledStat(StatValue stat)
    {
        return stat == null
            ? null
            : new StatValue(stat.StatType, stat.Value * QualityRoll, stat.Operation);
    }
}
