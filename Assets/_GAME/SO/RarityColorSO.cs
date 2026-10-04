using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "RarityColorSO", menuName = "Scriptable Objects/RarityColorSO")]
public class RarityColorSO : ScriptableObject
{
    [SerializeField] private List<Color> listRarityColor = new List<Color>();


    public Color GetRarityColor(RarityType rarityType)
    {
        return listRarityColor[(int)rarityType];
    }
}
