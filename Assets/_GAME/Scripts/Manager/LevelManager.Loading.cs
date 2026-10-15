using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class LevelManager
{
    private bool CanUseLoadedLevel(
        string levelAddress,
        DifficultType difficulty,
        MapType mapType,
        int requestedLevelIndex)
    {
        bool loadSucceeded = currentLevelHandle.Status == AsyncOperationStatus.Succeeded
            && currentLevelHandle.Result != null;
        if (!loadSucceeded)
        {
            Debug.LogError($"Cannot load LevelDataSO with address '{levelAddress}'.", this);
            return false;
        }

        if (MapManager.Ins.CurrentDifficulty != difficulty
            || MapManager.Ins.CurrentMapType != mapType
            || currentLevelHandle.Result.LevelIndex != requestedLevelIndex)
        {
            Debug.LogWarning(
                $"Loaded level '{levelAddress}' no longer matches the selected map/level.",
                this);
            return false;
        }

        return CanEnterLoadedLevel(currentLevelHandle.Result)
            && MapManager.Ins.EnsureCurrentMapSpawned();
    }

    private void AbortLevelLoad()
    {
        ReleaseCurrentLevel();
        State = LevelPlayState.None;
    }

    private bool CanEnterLoadedLevel(LevelDataSO levelData)
    {
        int globalLevel = DataManager.Ins.GetGlobalLevel();
        if (levelData.CanEnter(globalLevel))
        {
            return true;
        }

        Debug.LogWarning(
            $"Level '{levelData.name}' requires global player level "
            + $"{levelData.LevelRequired}, current level is {globalLevel}.",
            this
        );
        return false;
    }
}
