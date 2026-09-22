using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
   [SerializeField] private List<MapDataSO> listMapData = new List<MapDataSO>();

   public MapDataSO GetMapData(MapType mapType)
    {
        
        return mapType == MapType.NONE ? null: listMapData[(int)mapType];
    }
}
