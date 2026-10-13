using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StatIconData",
    menuName = "IdleGo/UI/Stat Icon Data")]
public sealed class StatIconSO : ScriptableObject
{
    [SerializeField] private List<StatIconEntry> statIcons =
        new List<StatIconEntry>();

    public Sprite GetIcon(StatType statType)
    {
        for (int i = 0; i < statIcons.Count; i++)
        {
            if (statIcons[i].StatType == statType)
            {
                return statIcons[i].Icon;
            }
        }

        return null;
    }

    public void EnsureAllStats()
    {
        statIcons ??= new List<StatIconEntry>();
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            AddMissingEntry((StatType)i);
        }
    }

    private void OnValidate()
    {
        EnsureAllStats();
    }

    private void AddMissingEntry(StatType statType)
    {
        for (int i = 0; i < statIcons.Count; i++)
        {
            if (statIcons[i].StatType == statType)
            {
                return;
            }
        }

        statIcons.Add(new StatIconEntry(statType));
    }
}

[Serializable]
public struct StatIconEntry
{
    [SerializeField] private StatType statType;
    [SerializeField] private Sprite icon;

    public StatIconEntry(StatType type)
    {
        statType = type;
        icon = null;
    }

    public StatType StatType => statType;
    public Sprite Icon => icon;
}
