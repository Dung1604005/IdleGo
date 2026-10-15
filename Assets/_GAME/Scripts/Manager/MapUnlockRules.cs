using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MapUnlockRules
{
    private readonly IReadOnlyList<MapDataSO> maps;
    private readonly MapProgressState progress;

    public MapUnlockRules(
        IReadOnlyList<MapDataSO> mapData,
        MapProgressState progressState)
    {
        maps = mapData;
        progress = progressState;
    }

    public bool IsDifficultyUnlocked(DifficultType difficulty)
    {
        if (!Enum.IsDefined(typeof(DifficultType), difficulty)) return false;
        if (difficulty == DifficultType.EASY) return true;
        DifficultType previous = (DifficultType)((int)difficulty - 1);
        return IsDifficultyUnlocked(previous) && AreAllMapsCompleted(previous);
    }

    public bool IsMapUnlocked(DifficultType difficulty, MapType mapType)
    {
        int mapIndex = GetMapIndex(mapType);
        if (!IsDifficultyUnlocked(difficulty) || mapIndex < 0) return false;
        if (mapIndex == 0) return true;
        MapDataSO previousMap = maps[mapIndex - 1];
        return previousMap != null
            && IsMapUnlocked(difficulty, previousMap.MapType)
            && IsMapCompleted(difficulty, previousMap.MapType);
    }

    public bool IsLevelUnlocked(
        DifficultType difficulty,
        MapType mapType,
        int levelIndex)
    {
        MapDataSO mapData = GetMapData(mapType);
        if (!IsMapUnlocked(difficulty, mapType)
            || mapData == null
            || levelIndex < 0
            || levelIndex >= mapData.TotalLevel)
        {
            return false;
        }
        return levelIndex == 0
            || progress.GetHighestClearedLevel(difficulty, mapType) >= levelIndex - 1;
    }

    public bool IsMapCompleted(DifficultType difficulty, MapType mapType)
    {
        MapDataSO mapData = GetMapData(mapType);
        return mapData != null
            && mapData.TotalLevel > 0
            && progress.GetHighestClearedLevel(difficulty, mapType)
                >= mapData.TotalLevel - 1;
    }

    public bool AreAllMapsCompleted(DifficultType difficulty)
    {
        bool foundMap = false;
        for (int i = 0; maps != null && i < maps.Count; i++)
        {
            if (maps[i] == null) continue;
            foundMap = true;
            if (!IsMapCompleted(difficulty, maps[i].MapType)) return false;
        }
        return foundMap;
    }

    public int GetHighestUnlockedLevel(
        DifficultType difficulty,
        MapType mapType)
    {
        MapDataSO mapData = GetMapData(mapType);
        if (mapData == null || mapData.TotalLevel <= 0) return -1;
        int cleared = progress.GetHighestClearedLevel(difficulty, mapType);
        return Mathf.Clamp(cleared + 1, 0, mapData.TotalLevel - 1);
    }

    public MapDataSO GetMapData(MapType mapType)
    {
        int index = GetMapIndex(mapType);
        return index >= 0 ? maps[index] : null;
    }

    public MapDataSO GetFirstMapData()
    {
        for (int i = 0; maps != null && i < maps.Count; i++)
        {
            if (maps[i] != null) return maps[i];
        }
        return null;
    }

    private int GetMapIndex(MapType mapType)
    {
        for (int i = 0; maps != null && i < maps.Count; i++)
        {
            if (maps[i] != null && maps[i].MapType == mapType) return i;
        }
        return -1;
    }
}
