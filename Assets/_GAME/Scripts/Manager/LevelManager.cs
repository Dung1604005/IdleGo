using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private List<LevelDataSO> levels = new List<LevelDataSO>();

    private int currentLevelIndex = -1;
    private int currentWaveIndex = -1;

    public bool IsInitialized { get; private set; }
    public int CurrentLevelNumber => currentLevelIndex + 1;
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public LevelDataSO CurrentLevelData => GetLevel(currentLevelIndex);
    public WaveData CurrentWaveData => CurrentLevelData?.GetWave(currentWaveIndex);

    public void OnInit()
    {
        OnDespawn();
        if (enemyManager == null)
        {
            Debug.LogError("LevelManager needs an EnemyManager before OnInit().", this);
            return;
        }

        enemyManager.OnInit();
        IsInitialized = true;

        if (levels != null && levels.Count > 0)
        {
            StartLevel(0);
        }
    }

    public void OnDespawn()
    {
        if (IsInitialized && enemyManager != null)
        {
            enemyManager.OnDespawn();
        }

        currentLevelIndex = -1;
        currentWaveIndex = -1;
        IsInitialized = false;
    }

    public bool StartLevel(int levelIndex)
    {
        if (!IsInitialized || GetLevel(levelIndex) == null)
        {
            return false;
        }

        currentLevelIndex = levelIndex;
        currentWaveIndex = -1;

        // Bắt đầu level cũng chính là chuyển vào wave đầu tiên của level đó.
        return StartNextWave();
    }

    public bool StartNextLevel()
    {
        return StartLevel(currentLevelIndex + 1);
    }

    public bool StartNextWave()
    {
        if (!IsInitialized || CurrentLevelData == null)
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

        // LevelManager chỉ quyết định wave; toàn bộ việc tạo enemy thuộc EnemyManager.
        enemyManager.SpawnWave(nextWave);
        return true;
    }

    private LevelDataSO GetLevel(int levelIndex)
    {
        if (levels == null || levelIndex < 0 || levelIndex >= levels.Count)
        {
            return null;
        }

        return levels[levelIndex];
    }
}
