using System.Collections.Generic;

public partial class DataManager
{
    public DifficultType CurrentDifficulty =>
        MapManager.Ins.CurrentDifficulty;

    public MapType CurrentMapType =>
        MapManager.Ins.CurrentMapType;

    public int CurrentLevelIndex =>
        MapManager.Ins.CurrentLevelIndex;

    public IReadOnlyList<MapDataSO> GetMaps()
    {
        return MapManager.Ins.Maps;
    }

    public MapDataSO GetMapData(MapType mapType)
    {
        return MapManager.Ins.GetMapData(mapType);
    }

    public bool IsDifficultyUnlocked(DifficultType difficulty)
    {
        return MapManager.Ins.IsDifficultyUnlocked(difficulty);
    }

    public bool IsMapUnlocked(DifficultType difficulty, MapType mapType)
    {
        return MapManager.Ins.IsMapUnlocked(difficulty, mapType);
    }

    public bool IsLevelUnlocked(
        DifficultType difficulty,
        MapType mapType,
        int levelIndex)
    {
        return MapManager.Ins.IsLevelUnlocked(difficulty, mapType, levelIndex);
    }

    public bool IsLevelCompleted(
        DifficultType difficulty,
        MapType mapType,
        int levelIndex)
    {
        return MapManager.Ins.IsLevelCompleted(difficulty, mapType, levelIndex);
    }

    public bool IsMapCompleted(DifficultType difficulty, MapType mapType)
    {
        return MapManager.Ins.IsMapCompleted(difficulty, mapType);
    }

    public bool IsDifficultyCompleted(DifficultType difficulty)
    {
        return MapManager.Ins.IsDifficultyCompleted(difficulty);
    }

    public bool SelectDifficulty(DifficultType difficulty)
    {
        return MapManager.Ins.SelectDifficulty(difficulty);
    }

    public bool SelectMap(MapType mapType)
    {
        return MapManager.Ins.SelectMap(mapType);
    }

    public bool SelectLevel(int levelIndex)
    {
        return LevelManager.Ins.StartLevel(levelIndex);
    }
}
