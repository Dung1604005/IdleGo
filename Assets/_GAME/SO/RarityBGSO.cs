using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RarityBGSO", menuName = "Scriptable Objects/RarityBGSO")]
public class RarityBGSO : ScriptableObject
{
    [SerializeField] private List<Sprite> listRarityBG = new List<Sprite>();


    public Sprite GetRarityBG(RarityType rarityType)
    {
        return listRarityBG[(int)rarityType];
    }
}
