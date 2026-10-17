using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class LevelManager : Singleton<LevelManager>
{
    [Header("Addressables")]
    [SerializeField] private string levelAddressPrefix = "Level_";

    [Header("Wave")]
    [SerializeField, Min(0f)] private float waveTransitionDelay = 1f;

    [SerializeField] private LevelInfo levelInfo;

    private AsyncOperationHandle<LevelDataSO> currentLevelHandle;
    private Coroutine loadLevelCoroutine;
    private bool hasCurrentLevelHandle;
    private float nextWaveStartTime;
    private DifficultType activeDifficulty;
    private MapType activeMapType = MapType.NONE;

    public bool IsInitialized { get; private set; }
    public LevelPlayState State { get; private set; }
    
    public bool IsLevelCompleted => State == LevelPlayState.Completed;

    public void OnInit()
    {
        OnDespawn();
        if (!MapManager.Ins.IsInitialized) MapManager.Ins.OnInit();
        IsInitialized = true;
        StartLevel(MapManager.Ins.CurrentLevelIndex);
    }

    public void OnDespawn()
    {
        CancelPendingLevelTransition();
        if (loadLevelCoroutine != null)
        {
            StopCoroutine(loadLevelCoroutine);
            loadLevelCoroutine = null;
        }

        // Pool dùng prefab là dependency của LevelData nên phải dọn trước khi release Addressable.
        AttackProjectilePool.ReleaseAll();
        EnemyManager.Ins.OnDespawn();
        LootManager.Ins.ClearLevelData();
        ReleaseCurrentLevel();

        levelInfo.OnDespawn();
        activeDifficulty = DifficultType.EASY;
        activeMapType = MapType.NONE;
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

        if (CanResolveLevelResult() && PlayerManager.Ins.IsTeamDefeated)
        {
            OnLevelFail(levelFailDisplayDuration);
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
                OnLevelComplete(levelCompleteDisplayDuration);
            }
        }
    }

    public bool StartLevel(int levelIndex)
    {
        if (!IsInitialized
            || loadLevelCoroutine != null
            || !MapManager.Ins.IsLevelUnlocked(
                MapManager.Ins.CurrentDifficulty,
                MapManager.Ins.CurrentMapType,
                levelIndex))
        {
            return false;
        }

        loadLevelCoroutine = StartCoroutine(LoadLevelRoutine(levelIndex));
        return true;
    }

    public bool StartNextLevel()
    {
        if (!IsInitialized
            || State == LevelPlayState.Loading
            || activeDifficulty != MapManager.Ins.CurrentDifficulty
            || activeMapType != MapManager.Ins.CurrentMapType)
        {
            return false;
        }

        return StartLevel(levelInfo.CurrentLevelIndex + 1);
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

    private IEnumerator LoadLevelRoutine(int levelIndex)
    {
        State = LevelPlayState.Loading;
        UnloadCurrentLevel();

        DifficultType difficulty = MapManager.Ins.CurrentDifficulty;
        MapType mapType = MapManager.Ins.CurrentMapType;
        string levelAddress = GetLevelAddress(mapType, levelIndex);
        currentLevelHandle = Addressables.LoadAssetAsync<LevelDataSO>(levelAddress);
        hasCurrentLevelHandle = true;
        yield return currentLevelHandle;
        loadLevelCoroutine = null;

        if (!IsInitialized)
        {
            yield break;
        }

        if (!CanUseLoadedLevel(levelAddress, difficulty, mapType, levelIndex))
        {
            AbortLevelLoad();
            yield break;
        }

        if (!MapManager.Ins.CommitLevelSelection(levelIndex))
        {
            AbortLevelLoad();
            yield break;
        }
        levelInfo.SetLevelData(currentLevelHandle.Result);
        activeDifficulty = difficulty;
        activeMapType = mapType;
        levelInfo.SetWaveData(-1);
        LootManager.Ins.SetLevelData(currentLevelHandle.Result);
        OnLevelStart();
    }

    private void UnloadCurrentLevel()
    {
        // Projectile prefabs may be dependencies of the Addressable level too.
        AttackProjectilePool.ReleaseAll();
        EnemyManager.Ins.OnDespawn();
        LootManager.Ins.ClearLevelData();
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

}

public enum LevelPlayState
{
    None,
    Loading,
    PlayingWave,
    WaitingNextWave,
    Completed,
    Failed
}
