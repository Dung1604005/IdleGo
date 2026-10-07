using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class LevelManager
{
    private bool CanUseLoadedLevel(string levelAddress, MapType mapType)
    {
        bool loadSucceeded = currentLevelHandle.Status == AsyncOperationStatus.Succeeded
            && currentLevelHandle.Result != null;
        if (!loadSucceeded)
        {
            Debug.LogError($"Cannot load LevelDataSO with address '{levelAddress}'.", this);
            return false;
        }

        return CanEnterLoadedLevel(currentLevelHandle.Result)
            && SpawnMapIfNeeded(mapType);
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
