using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnchantMaterialData",
    menuName = "IdleGo/Item/Enchant Material Data")]
public class EnchantMaterialDataSO : ItemSO
{
    [SerializeField] private BuffStatType buffStatType;
    [SerializeField] private List<StatValue> possibleStats = new List<StatValue>();

    public BuffStatType BuffStatType => buffStatType;
    public IReadOnlyList<StatValue> PossibleStats => possibleStats;

    public bool TryRollStat(out StatValue rolledStat)
    {
        rolledStat = null;
        int validCount = CountValidStats();
        if (validCount <= 0)
        {
            return false;
        }

        // Random tren cac dong hop le de null trong Inspector khong lam lech ti le.
        int selectedIndex = Random.Range(0, validCount);
        for (int i = 0; i < possibleStats.Count; i++)
        {
            StatValue stat = possibleStats[i];
            if (!IsValidStat(stat) || selectedIndex-- > 0)
            {
                continue;
            }

            rolledStat = new StatValue(stat.StatType, stat.Value, stat.Operation);
            return true;
        }
        return false;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        itemType = ItemType.ENCHANT_MATERIAL;
        if (!BuffStatTypeUtility.IsValid(buffStatType))
        {
            buffStatType = BuffStatType.SOCKET;
        }
        possibleStats ??= new List<StatValue>();
    }

    private int CountValidStats()
    {
        int count = 0;
        for (int i = 0; i < possibleStats.Count; i++)
        {
            if (IsValidStat(possibleStats[i]))
            {
                count++;
            }
        }
        return count;
    }

    private static bool IsValidStat(StatValue stat)
    {
        return stat != null && StatTypeUtility.CanHaveModifiers(stat.StatType);
    }
}
