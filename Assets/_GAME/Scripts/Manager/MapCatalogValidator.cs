using System.Collections.Generic;
using UnityEngine;

public static class MapCatalogValidator
{
    public static void Validate(
        IReadOnlyList<MapDataSO> maps,
        Object logContext)
    {
        if (maps == null || maps.Count == 0)
        {
            Debug.LogError("MapManager needs at least one MapDataSO.", logContext);
            return;
        }

        HashSet<MapType> mapTypes = new HashSet<MapType>();
        for (int i = 0; i < maps.Count; i++)
        {
            MapDataSO mapData = maps[i];
            if (mapData == null)
            {
                Debug.LogError($"MapManager map entry {i} is empty.", logContext);
            }
            else if (!mapTypes.Add(mapData.MapType))
            {
                Debug.LogError($"Duplicate MapType '{mapData.MapType}'.", logContext);
            }
        }
    }
}
