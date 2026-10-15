using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class MapProgressState
{
    [SerializeField] private DifficultType currentDifficulty;
    [SerializeField] private MapType currentMapType = MapType.NONE;
    [SerializeField] private int currentLevelIndex;
    [SerializeField] private List<MapClearSaveData> clearedLevels =
        new List<MapClearSaveData>();

    public DifficultType CurrentDifficulty => currentDifficulty;
    public MapType CurrentMapType => currentMapType;
    public int CurrentLevelIndex => Mathf.Max(0, currentLevelIndex);

    public void OnInit(MapType defaultMapType)
    {
        currentDifficulty = DifficultType.EASY;
        currentMapType = defaultMapType;
        currentLevelIndex = 0;
        clearedLevels ??= new List<MapClearSaveData>();
        clearedLevels.Clear();
    }

    public void Restore(MapProgressSaveData saveData)
    {
        if (saveData == null) return;
        currentDifficulty = saveData.currentDifficulty;
        currentMapType = saveData.currentMapType;
        currentLevelIndex = Mathf.Max(0, saveData.currentLevelIndex);
        clearedLevels.Clear();
        MergeClearedLevels(saveData.clearedLevels);
    }

    public MapProgressSaveData CreateSaveData()
    {
        MapProgressSaveData output = new MapProgressSaveData
        {
            currentDifficulty = currentDifficulty,
            currentMapType = currentMapType,
            currentLevelIndex = CurrentLevelIndex
        };
        for (int i = 0; i < clearedLevels.Count; i++)
        {
            output.clearedLevels.Add(Clone(clearedLevels[i]));
        }
        return output;
    }

    public void SetSelection(
        DifficultType difficulty,
        MapType mapType,
        int levelIndex)
    {
        currentDifficulty = difficulty;
        currentMapType = mapType;
        currentLevelIndex = Mathf.Max(0, levelIndex);
    }

    public bool CompleteLevel(
        DifficultType difficulty,
        MapType mapType,
        int levelIndex)
    {
        MapClearSaveData progress = GetProgress(difficulty, mapType);
        if (progress == null)
        {
            progress = new MapClearSaveData
            {
                difficulty = difficulty,
                mapType = mapType
            };
            clearedLevels.Add(progress);
        }
        if (levelIndex <= progress.highestClearedLevelIndex) return false;
        progress.highestClearedLevelIndex = levelIndex;
        return true;
    }

    public int GetHighestClearedLevel(
        DifficultType difficulty,
        MapType mapType)
    {
        MapClearSaveData progress = GetProgress(difficulty, mapType);
        return progress != null ? progress.highestClearedLevelIndex : -1;
    }

    private void MergeClearedLevels(IReadOnlyList<MapClearSaveData> saved)
    {
        for (int i = 0; saved != null && i < saved.Count; i++)
        {
            MapClearSaveData source = saved[i];
            if (source == null || source.mapType == MapType.NONE) continue;
            int levelIndex = Mathf.Max(-1, source.highestClearedLevelIndex);
            if (levelIndex < 0) continue;
            CompleteLevel(source.difficulty, source.mapType, levelIndex);
        }
    }

    private MapClearSaveData GetProgress(
        DifficultType difficulty,
        MapType mapType)
    {
        for (int i = 0; i < clearedLevels.Count; i++)
        {
            MapClearSaveData progress = clearedLevels[i];
            if (progress.difficulty == difficulty && progress.mapType == mapType)
            {
                return progress;
            }
        }
        return null;
    }

    private static MapClearSaveData Clone(MapClearSaveData source)
    {
        return new MapClearSaveData
        {
            difficulty = source.difficulty,
            mapType = source.mapType,
            highestClearedLevelIndex = source.highestClearedLevelIndex
        };
    }
}
