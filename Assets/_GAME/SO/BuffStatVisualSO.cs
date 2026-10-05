using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffStatVisualSO", menuName = "Scriptable Objects/BuffStatVisualSO")]
public class BuffStatVisualSO : ScriptableObject
{
    [SerializeField] private List<Sprite> listIconBuffStat = new List<Sprite>();

    [SerializeField] private List<Color> listColorBuffStat = new List<Color>();


    public Sprite GetIconBuffStat(BuffStatType buffStatType)
    {
        return listIconBuffStat[(int)buffStatType];
    }

    public Color GetColorBuffStat(BuffStatType buffStatType)
    {
        return listColorBuffStat[(int)buffStatType];
    }
}
