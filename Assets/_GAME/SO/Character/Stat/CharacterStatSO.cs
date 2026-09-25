using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStat", menuName = "IdleGo/CharacterStatSO")]
public class CharacterStatSO : ScriptableObject
{
    [SerializeField, StatList] private List<float> baseStats = new List<float>();

    public IReadOnlyList<float> BaseStats => baseStats;

    public float GetBaseStat(StatType statType)
    {
        EnsureBaseStats();
        if (!StatTypeUtility.IsValid(statType))
        {
            return 0f;
        }

        return StatTypeUtility.NormalizeValue(statType, baseStats[(int)statType]);
    }

    private void OnValidate()
    {
        EnsureBaseStats();
        for (int i = 0; i < baseStats.Count; i++)
        {
            baseStats[i] = StatTypeUtility.NormalizeValue((StatType)i, baseStats[i]);
        }
    }

    private void EnsureBaseStats()
    {
        if (baseStats == null)
        {
            baseStats = new List<float>();
        }

        StatTypeUtility.EnsureListSize(baseStats);
    }
}
