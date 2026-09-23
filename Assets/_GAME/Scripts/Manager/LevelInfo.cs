using System;
using UnityEngine;

[Serializable]
public class LevelInfo
{
    [SerializeField] private MapType currentMapType = MapType.NONE;
    [SerializeField] private int currentLevelIndex = -1;
    [SerializeField] private int currentWaveIndex = -1;

    [SerializeField] private MapDataSO currentMapData;

    [SerializeField] private MapController currentMapInstance;

    [SerializeField] private LevelDataSO currentLevelData;

    [SerializeField] private WaveData currentWaveData;


    public MapType CurrentMapType => currentMapType;
    public int CurrentLevelNumber => currentLevelIndex + 1;

    public int CurrentLevelIndex => currentLevelIndex;
    public int CurrentWaveNumber => currentWaveIndex + 1;

    public int CurrentWaveIndex => currentWaveIndex;

    public void OnDespawn()
    {
        SetLevelData(null);
        SetMapData(MapType.NONE);
        SetMapInstance(null);
        SetWaveData(-1);
    }

    public LevelDataSO GetLevelData()
    {
        return currentLevelData;
    }

    public void SetLevelData(LevelDataSO _levelData)
    {
        currentLevelIndex = _levelData == null ? -1: _levelData.LevelIndex;
        currentLevelData = _levelData;
    }
    public WaveData GetWaveData()
    {
        return currentWaveData;
    }

    public void SetWaveData(int waveIndex)
    {
        
        currentWaveIndex = waveIndex;
        currentWaveData = waveIndex == -1 ? null : currentLevelData.GetWave(waveIndex);
    }

    public void SetMapData(MapType mapType)
    {
        currentMapData = DataManager.Ins.GetMapData(mapType);
        currentMapType= mapType ;
    }

    public MapController GetMapInstance()
    {
        return currentMapInstance;
    }

    public void SetMapInstance(MapController mapInstance)
    {
        currentMapInstance = mapInstance;
    }

    public bool IsLevelNull()
    {
        return currentLevelData == null;
    }
}
