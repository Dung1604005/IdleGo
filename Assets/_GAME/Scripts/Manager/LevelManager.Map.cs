using UnityEngine;

public partial class LevelManager
{
    private bool SpawnMapIfNeeded(MapType mapType)
    {
        if (levelInfo.GetMapInstance() != null && levelInfo.CurrentMapType == mapType)
        {
            return true;
        }

        MapDataSO mapData = DataManager.Ins.GetMapData(mapType);
        MapController mapPrefab = mapData != null ? mapData.GetMapPrefab() : null;
        if (mapPrefab == null)
        {
            Debug.LogError($"MapData of '{mapType}' does not have a MapController prefab.", this);
            return false;
        }

        // Chi thay map sau khi level moi load thanh cong de giu map cu neu Addressables loi.
        DespawnCurrentMap();
        MapController mapInstance = Instantiate(
            mapPrefab,
            mapData.SpawnPos,
            mapPrefab.transform.rotation,
            mapContainer
        );
        levelInfo.SetMapInstance(mapInstance);
        return true;
    }

    private void DespawnCurrentMap()
    {
        MapController mapInstance = levelInfo.GetMapInstance();
        if (mapInstance == null)
        {
            return;
        }

        Destroy(mapInstance.gameObject);
        levelInfo.SetMapInstance(null);
    }

    private string GetLevelAddress(MapType mapType, int levelIndex)
    {
        // Vi du mapIndex 0, levelIndex 1 tao address "Level_1-2".
        return $"{levelAddressPrefix}{(int)mapType + 1}-{levelIndex + 1}";
    }
}
