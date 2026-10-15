using System.Collections.Generic;
using UnityEngine;

public sealed class MapManager : Singleton<MapManager>
{
    [Header("Map Data In Progression Order")]
    [SerializeField] private List<MapDataSO> maps = new List<MapDataSO>();
    [SerializeField] private Transform mapContainer;

    [Header("Save")]
    [SerializeField] private string saveKey = "PLAYER_MAP_PROGRESS";
    [SerializeField] private MapProgressState progress = new MapProgressState();

    private MapUnlockRules unlockRules;
    private readonly MapProgressViewRegistry viewRegistry =
        new MapProgressViewRegistry();
    [SerializeField] private MapController currentMapInstance;
    [SerializeField] private MapType spawnedMapType = MapType.NONE;

    public bool IsInitialized { get; private set; }
    public bool LastSaveSucceeded { get; private set; }
    public DifficultType CurrentDifficulty => progress.CurrentDifficulty;
    public MapType CurrentMapType => progress.CurrentMapType;
    public int CurrentLevelIndex => progress.CurrentLevelIndex;
    public MapController CurrentMapInstance => currentMapInstance;
    public IReadOnlyList<MapDataSO> Maps => maps;

    public void OnInit()
    {
        if (IsInitialized) OnDespawn();
        progress ??= new MapProgressState();
        maps ??= new List<MapDataSO>();
        MapCatalogValidator.Validate(maps, this);
        unlockRules = new MapUnlockRules(maps, progress);
        MapDataSO firstMap = unlockRules.GetFirstMapData();
        progress.OnInit(firstMap != null ? firstMap.MapType : MapType.NONE);
        LoadProgress();
        NormalizeSelection();
        IsInitialized = true;
        LastSaveSucceeded = SaveGame();
    }

    public void OnDespawn()
    {
        DespawnCurrentMap();
        viewRegistry.Clear();
        unlockRules = null;
        IsInitialized = false;
    }

    public void RegisterView(IMapProgressView view)
    {
        viewRegistry.Register(view);
    }

    public void UnregisterView(IMapProgressView view)
    {
        viewRegistry.Unregister(view);
    }

    public MapDataSO GetMapData(MapType mapType)
    {
        return unlockRules != null
            ? unlockRules.GetMapData(mapType)
            : FindMapData(mapType);
    }

    public bool IsDifficultyUnlocked(DifficultType difficulty)
    {
        return unlockRules != null && unlockRules.IsDifficultyUnlocked(difficulty);
    }

    public bool IsMapUnlocked(DifficultType difficulty, MapType mapType)
    {
        return unlockRules != null && unlockRules.IsMapUnlocked(difficulty, mapType);
    }

    public bool IsLevelUnlocked(
        DifficultType difficulty,
        MapType mapType,
        int levelIndex)
    {
        return unlockRules != null
            && unlockRules.IsLevelUnlocked(difficulty, mapType, levelIndex);
    }

    public bool IsLevelCompleted(
        DifficultType difficulty,
        MapType mapType,
        int levelIndex)
    {
        return levelIndex >= 0
            && progress.GetHighestClearedLevel(difficulty, mapType) >= levelIndex;
    }

    public bool IsMapCompleted(DifficultType difficulty, MapType mapType)
    {
        return unlockRules != null && unlockRules.IsMapCompleted(difficulty, mapType);
    }

    public bool IsDifficultyCompleted(DifficultType difficulty)
    {
        return unlockRules != null && unlockRules.AreAllMapsCompleted(difficulty);
    }

    public bool SelectDifficulty(DifficultType difficulty)
    {
        if (!IsInitialized || !IsDifficultyUnlocked(difficulty)) return false;
        MapType targetMap = CurrentMapType;
        if (!IsMapUnlocked(difficulty, targetMap))
        {
            targetMap = unlockRules.GetFirstMapData()?.MapType ?? MapType.NONE;
        }
        int targetLevel = GetValidLevel(difficulty, targetMap, CurrentLevelIndex);
        progress.SetSelection(difficulty, targetMap, targetLevel);
        CompleteDataFlow();
        return true;
    }

    public bool SelectMap(MapType mapType)
    {
        if (!IsInitialized || !IsMapUnlocked(CurrentDifficulty, mapType)) return false;
        int levelIndex = mapType == CurrentMapType ? CurrentLevelIndex : 0;
        levelIndex = GetValidLevel(CurrentDifficulty, mapType, levelIndex);
        progress.SetSelection(CurrentDifficulty, mapType, levelIndex);
        CompleteDataFlow();
        return true;
    }

    public bool CommitLevelSelection(int levelIndex)
    {
        if (!IsInitialized
            || !IsLevelUnlocked(CurrentDifficulty, CurrentMapType, levelIndex))
        {
            return false;
        }
        progress.SetSelection(CurrentDifficulty, CurrentMapType, levelIndex);
        CompleteDataFlow();
        return true;
    }

    public bool CompleteLevel(
        DifficultType difficulty,
        MapType mapType,
        int levelIndex)
    {
        if (!IsInitialized
            || !IsLevelUnlocked(difficulty, mapType, levelIndex))
        {
            return false;
        }
        progress.CompleteLevel(difficulty, mapType, levelIndex);
        CompleteDataFlow();
        return true;
    }

    public bool EnsureCurrentMapSpawned()
    {
        return IsInitialized && SpawnMapIfNeeded(CurrentMapType);
    }

    public bool SaveGame()
    {
        return MapProgressSaveSystem.TrySave(saveKey, progress.CreateSaveData());
    }

    private void LoadProgress()
    {
        if (MapProgressSaveSystem.TryLoad(saveKey, out MapProgressSaveData saveData))
        {
            progress.Restore(saveData);
        }
    }

    private void NormalizeSelection()
    {
        DifficultType difficulty = CurrentDifficulty;
        if (!IsDifficultyUnlocked(difficulty)) difficulty = DifficultType.EASY;
        MapType mapType = CurrentMapType;
        if (!IsMapUnlocked(difficulty, mapType))
        {
            mapType = unlockRules.GetFirstMapData()?.MapType ?? MapType.NONE;
        }
        int levelIndex = GetValidLevel(difficulty, mapType, CurrentLevelIndex);
        progress.SetSelection(difficulty, mapType, levelIndex);
    }

    private int GetValidLevel(
        DifficultType difficulty,
        MapType mapType,
        int requestedLevel)
    {
        int highest = unlockRules.GetHighestUnlockedLevel(difficulty, mapType);
        return highest < 0 ? 0 : Mathf.Clamp(requestedLevel, 0, highest);
    }

    private bool SpawnMapIfNeeded(MapType mapType)
    {
        if (currentMapInstance != null && spawnedMapType == mapType) return true;
        MapDataSO mapData = GetMapData(mapType);
        MapController prefab = mapData != null ? mapData.GetMapPrefab() : null;
        if (prefab == null)
        {
            Debug.LogError($"Map '{mapType}' does not have a map prefab.", this);
            return false;
        }

        // Chi huy map cu sau khi da xac nhan map moi co du data de spawn.
        DespawnCurrentMap();
        currentMapInstance = Instantiate(
            prefab, mapData.SpawnPos, prefab.transform.rotation, mapContainer);
        spawnedMapType = mapType;
        return true;
    }

    private void DespawnCurrentMap()
    {
        if (currentMapInstance != null) Destroy(currentMapInstance.gameObject);
        currentMapInstance = null;
        spawnedMapType = MapType.NONE;
    }

    private void CompleteDataFlow()
    {
        // MapManager owns progress: mutate, save, then let views read through DataManager.
        LastSaveSucceeded = SaveGame();
        viewRegistry.RefreshViews();
    }

    private MapDataSO FindMapData(MapType mapType)
    {
        for (int i = 0; i < maps.Count; i++)
        {
            if (maps[i] != null && maps[i].MapType == mapType) return maps[i];
        }
        return null;
    }

}
