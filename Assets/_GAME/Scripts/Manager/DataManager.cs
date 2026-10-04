using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
   [SerializeField] private List<MapDataSO> listMapData = new List<MapDataSO>();

   [SerializeField] private RarityBGSO rarityBGData;

   [SerializeField] private RarityColorSO rarityColorData;

   public MapDataSO GetMapData(MapType mapType)
    {
        
        return mapType == MapType.NONE ? null: listMapData[(int)mapType];
    }

    public Sprite GetRarityBGSprite(RarityType rarityType)
    {
        return rarityBGData.GetRarityBG(rarityType);
    }

    public Color GetRarityColor(RarityType rarityType)
    {
        return rarityColorData.GetRarityColor(rarityType);
    }
}
