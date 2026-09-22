using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelManager : Singleton<LevelManager>
{
    [Header("Addressables")]
    [SerializeField] private string levelAddressPrefix = "Level_";
    [SerializeField] private MapType firstMapType;
    [SerializeField, Min(0)] private int firstLevelIndex;

    [Header("Wave")]
    [SerializeField, Min(0f)] private float waveTransitionDelay = 1f;

    [SerializeField] private LevelInfo levelInfo;

    private AsyncOperationHandle<LevelDataSO> currentLevelHandle;
    private Coroutine loadLevelCoroutine;
    private bool hasCurrentLevelHandle;
    private float nextWaveStartTime;

    public bool IsInitialized { get; private set; }
    public LevelPlayState State { get; private set; }
    
    public bool IsLevelCompleted => State == LevelPlayState.Completed;

    public void OnInit()
    {
        OnDespawn();
        IsInitialized = true;
        StartLevel(firstMapType, firstLevelIndex);
    }

    public void OnDespawn()
    {
        if (loadLevelCoroutine != null)
        {
            StopCoroutine(loadLevelCoroutine);
            loadLevelCoroutine = null;
        }

        // Pool dùng prefab là dependency của LevelData nên phải dọn trước khi release Addressable.
        EnemyManager.Ins.OnDespawn();
        ReleaseCurrentLevel();

        levelInfo.OnDespawn();
        nextWaveStartTime = 0f;
        State = LevelPlayState.None;
        IsInitialized = false;
    }

    private void Update()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (State == LevelPlayState.PlayingWave && !EnemyManager.Ins.HasAliveEnemies)
        {
            // Không dùng event: LevelManager kiểm tra bộ đếm enemy để bắt đầu thời gian nghỉ giữa wave.
            State = LevelPlayState.WaitingNextWave;
            nextWaveStartTime = Time.time + waveTransitionDelay;
            return;
        }

        if (State == LevelPlayState.WaitingNextWave && Time.time >= nextWaveStartTime)
        {
            if (!StartNextWave())
            {
                CompleteLevel();
            }
        }
    }

    public bool StartLevel(MapType mapType, int levelIndex)
    {
        if (!IsInitialized || mapType == MapType.NONE || levelIndex < 0 || loadLevelCoroutine != null)
        {
            return false;
        }

        loadLevelCoroutine = StartCoroutine(LoadLevelRoutine(mapType, levelIndex));
        return true;
    }

    public bool StartMap(MapType mapType)
    {
        return StartLevel(mapType, 0);
    }

    public bool StartNextLevel()
    {
        if (!IsInitialized || State == LevelPlayState.Loading)
        {
            return false;
        }

        return StartLevel(levelInfo.CurrentMapType, levelInfo.CurrentLevelIndex + 1);
    }

    public bool StartNextMap()
    {
        if (!IsInitialized || State == LevelPlayState.Loading)
        {
            return false;
        }

        return StartLevel(levelInfo.CurrentMapType, 0);
    }

    public bool StartNextWave()
    {
        if (!IsInitialized || levelInfo.IsLevelNull() || EnemyManager.Ins.HasAliveEnemies)
        {
            return false;
        }

        int nextWaveIndex = levelInfo.CurrentWaveIndex + 1;
        WaveData nextWave = levelInfo.GetLevelData().GetWave(nextWaveIndex);
        if (nextWave == null)
        {
            return false;
        }
        levelInfo.SetWaveData(nextWaveIndex);
  
        // LevelManager quyết định wave, EnemyManager singleton chịu trách nhiệm spawn và despawn.
        EnemyManager.Ins.SpawnWave(nextWave);
        State = LevelPlayState.PlayingWave;
        return true;
    }

    private IEnumerator LoadLevelRoutine(MapType mapType, int levelIndex)
    {
        State = LevelPlayState.Loading;
        UnloadCurrentLevel();

        string levelAddress = GetLevelAddress(mapType, levelIndex);
        currentLevelHandle = Addressables.LoadAssetAsync<LevelDataSO>(levelAddress);
        hasCurrentLevelHandle = true;
        yield return currentLevelHandle;
        loadLevelCoroutine = null;

        if (!IsInitialized)
        {
            yield break;
        }

        if (currentLevelHandle.Status != AsyncOperationStatus.Succeeded || currentLevelHandle.Result == null)
        {
            Debug.LogError($"Cannot load LevelDataSO with address '{levelAddress}'.", this);
            ReleaseCurrentLevel();
            State = LevelPlayState.None;
            yield break;
        }

        levelInfo.SetMapData(mapType);
        levelInfo.SetLevelData(currentLevelHandle.Result);
        levelInfo.SetWaveData(-1);
        EnemyManager.Ins.OnInit();

        if (!StartNextWave())
        {
            CompleteLevel();
        }
    }

    private void UnloadCurrentLevel()
    {
        EnemyManager.Ins.OnDespawn();
        ReleaseCurrentLevel();
        levelInfo.SetWaveData(-1);
    }

    private void ReleaseCurrentLevel()
    {
        levelInfo.SetLevelData(null);
        if (!hasCurrentLevelHandle)
        {
            return;
        }

        if (currentLevelHandle.IsValid())
        {
            Addressables.Release(currentLevelHandle);
        }

        hasCurrentLevelHandle = false;
    }

    private void CompleteLevel()
    {
        State = LevelPlayState.Completed;
    }

    private string GetLevelAddress(MapType mapType, int levelIndex)
    {
        // Ví dụ mapIndex 0, levelIndex 1 sẽ tạo address "Level_1-2".
        return $"{levelAddressPrefix}{(int)mapType + 1}-{levelIndex + 1}";
    }
}

public enum LevelPlayState
{
    None,
    Loading,
    PlayingWave,
    WaitingNextWave,
    Completed
}