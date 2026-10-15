using System;
using System.Collections.Generic;

[Serializable]
public sealed class MapProgressSaveData
{
    public int version = 1;
    public DifficultType currentDifficulty = DifficultType.EASY;
    public MapType currentMapType = MapType.NONE;
    public int currentLevelIndex;
    public List<MapClearSaveData> clearedLevels = new List<MapClearSaveData>();
}

[Serializable]
public sealed class MapClearSaveData
{
    public DifficultType difficulty;
    public MapType mapType;
    public int highestClearedLevelIndex = -1;
}
