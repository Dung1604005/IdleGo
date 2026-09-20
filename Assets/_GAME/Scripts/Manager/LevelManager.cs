using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelManager : Singleton<LevelManager>
{
    [Header("Addressables")]
    [SerializeField] private string levelAddressPrefix = "Level_";
    [SerializeField, Min(0)] private int firstMapIndex;
    [SerializeField, Min(0)] private int firstLevelIndex;

    [Header("Wave")]
    [SerializeField, Min(0f)] private float waveTransitionDelay = 1f;

    private AsyncOperationHandle<LevelDataSO> currentLevelHandle;
    private Coroutine loadLevelCoroutine;
    private bool hasCurrentLevelHandle;
    private int currentMapIndex = -1;
    private int currentLevelIndex = -1;
    private int currentWaveIndex = -1;
    private float nextWaveStartTime;

    public bool IsInitialized { get; private set; }
    public LevelPlayState State { get; private set; }
    public int CurrentMapNumber => currentMapIndex + 1;
    public int CurrentLevelNumber => currentLevelIndex + 1;
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public LevelDataSO CurrentLevelData { get; private set; }
    public WaveData CurrentWaveData => CurrentLevelData?.GetWave(currentWaveIndex);
    public bool IsLevelCompleted => State == LevelPlayState.Completed;

    public void OnInit()
    {
        OnDespawn();
        IsInitialized = true;
        StartLevel(firstMapIndex, firstLevelIndex);
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

        currentMapIndex = -1;
        currentLevelIndex = -1;
        currentWaveIndex = -1;
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

    public bool StartLevel(int mapIndex, int levelIndex)
    {
        if (!IsInitialized || mapIndex < 0 || levelIndex < 0 || loadLevelCoroutine != null)
        {
            return false;
        }

        loadLevelCoroutine = StartCoroutine(LoadLevelRoutine(mapIndex, levelIndex));
        return true;
    }

    public bool StartMap(int mapIndex)
    {
        return StartLevel(mapIndex, 0);
    }

    public bool StartNextLevel()
    {
        if (!IsInitialized || State == LevelPlayState.Loading)
        {
            return false;
        }

        return StartLevel(currentMapIndex, currentLevelIndex + 1);
    }

    public bool StartNextMap()
    {
        if (!IsInitialized || State == LevelPlayState.Loading)
        {
            return false;
        }

        return StartLevel(currentMapIndex + 1, 0);
    }

    public bool StartNextWave()
    {
        if (!IsInitialized || CurrentLevelData == null || EnemyManager.Ins.HasAliveEnemies)
        {
            return false;
        }

        int nextWaveIndex = currentWaveIndex + 1;
        WaveData nextWave = CurrentLevelData.GetWave(nextWaveIndex);
        if (nextWave == null)
        {
            return false;
        }

        currentWaveIndex = nextWaveIndex;

        // LevelManager quyết định wave, EnemyManager singleton chịu trách nhiệm spawn và despawn.
        EnemyManager.Ins.SpawnWave(nextWave);
        State = LevelPlayState.PlayingWave;
        return true;
    }

    private IEnumerator LoadLevelRoutine(int mapIndex, int levelIndex)
    {
        State = LevelPlayState.Loading;
        UnloadCurrentLevel();

        string levelAddress = GetLevelAddress(mapIndex, levelIndex);
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

        currentMapIndex = mapIndex;
        currentLevelIndex = levelIndex;
        currentWaveIndex = -1;
        CurrentLevelData = currentLevelHandle.Result;
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
        currentWaveIndex = -1;
    }

    private void ReleaseCurrentLevel()
    {
        CurrentLevelData = null;
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

    private string GetLevelAddress(int mapIndex, int levelIndex)
    {
        // Ví dụ mapIndex 0, levelIndex 1 sẽ tạo address "Level_1-2".
        return $"{levelAddressPrefix}{mapIndex + 1}-{levelIndex + 1}";
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
