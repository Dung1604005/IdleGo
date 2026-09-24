using System;
using UnityEngine;

[Serializable]
public class StatValue
{
    [SerializeField] private StatType statType;
    [SerializeField] private float value;

    public StatType StatType => statType;
    public float Value => value;
}
