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

    public Color GetFeedbackColor(StatType statType)
    {
        foreach (StatIconEntry entry in statIcons)
        {
            if (entry.StatType == statType && entry.FeedbackColor.a > 0f)
            {
                return entry.FeedbackColor;
            }
        }
        return GetDefaultFeedbackColor(statType);
    }

    public static Color GetDefaultFeedbackColor(StatType statType)
    {
        return statType switch
        {
            StatType.MAX_HEALTH or StatType.LIFE_STEAL => new Color32(80, 255, 130, 255),
            StatType.DAMAGE or StatType.CRITICAL_CHANCE or StatType.CRITICAL_DAMAGE
                or StatType.DAMAGE_AMPLIFICATION => new Color32(255, 205, 70, 255),
            StatType.ARMOR or StatType.DODGE_CHANCE => new Color32(100, 205, 255, 255),
            StatType.COOLDOWN_REDUCTION => new Color32(190, 140, 255, 255),
            _ => new Color32(100, 255, 200, 255)
        };
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
    [SerializeField] private Color feedbackColor;

    public StatIconEntry(StatType type)
    {
        statType = type;
        icon = null;
        feedbackColor = StatIconSO.GetDefaultFeedbackColor(type);
    }

    public StatType StatType => statType;
    public Sprite Icon => icon;
    public Color FeedbackColor => feedbackColor;
}
