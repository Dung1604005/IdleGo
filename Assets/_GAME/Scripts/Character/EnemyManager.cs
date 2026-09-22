using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private Transform enemyContainer;

    [SerializeField] private int totalAliveEnemy;
    [SerializeField, Min(0f)] private float spawnInterval = 0.15f;

    private readonly HashSet<Enemy> levelPrefabs = new HashSet<Enemy>();
    private Coroutine spawnWaveCoroutine;
    private bool hasSpawnedInCurrentWave;
    private bool isSpawningWave;

    public IReadOnlyList<Enemy> Enemies => enemies;
    public int TotalAliveEnemy => Mathf.Max(0, totalAliveEnemy);
    public bool HasAliveEnemies => TotalAliveEnemy > 0 || isSpawningWave;
    public bool IsInitialized { get; private set; }

    public void OnInit()
    {
        if (enemies == null)
        {
            enemies = new List<Enemy>();
        }

        enemies.RemoveAll(enemy => enemy == null);
        totalAliveEnemy = 0;
        IsInitialized = true;
    }

    public void OnDespawn()
    {
        StopSpawnWave();

        if (enemies != null)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                if (enemy != null && enemy.gameObject.activeSelf)
                {
                    SimplePool.Despawn(enemy);
                }
            }

            enemies.Clear();
        }

        // Pool giữ reference tới prefab Addressable, vì vậy phải release trước khi unload level.
        foreach (Enemy prefab in levelPrefabs)
        {
            if (prefab != null)
            {
                SimplePool.Release(prefab);
            }
        }

        levelPrefabs.Clear();
        totalAliveEnemy = 0;
        IsInitialized = false;
    }

    public void DespawnEnemy(Enemy enemy)
    {
        if (enemy == null || !enemy.gameObject.activeSelf || !enemies.Contains(enemy))
        {
            return;
        }

        SimplePool.Despawn(enemy);
        totalAliveEnemy = Mathf.Max(0, totalAliveEnemy - 1);
    }

    public void SpawnWave(WaveData waveData)
    {
        if (!IsInitialized || waveData == null || waveData.Enemies == null || isSpawningWave)
        {
            return;
        }

        totalAliveEnemy = 0;
        hasSpawnedInCurrentWave = false;
        isSpawningWave = true;
        Coroutine startedCoroutine = StartCoroutine(SpawnWaveRoutine(waveData));
        spawnWaveCoroutine = isSpawningWave ? startedCoroutine : null;
    }

    private IEnumerator SpawnWaveRoutine(WaveData waveData)
    {
        WaitForSeconds spawnDelay = spawnInterval > 0f ? new WaitForSeconds(spawnInterval) : null;
        IReadOnlyList<WaveEnemyData> waveEnemies = waveData.Enemies;
        for (int i = 0; i < waveEnemies.Count; i++)
        {
            yield return SpawnEnemyGroup(waveData.SpawnPosition, waveEnemies[i], spawnDelay);
        }

        isSpawningWave = false;
        spawnWaveCoroutine = null;
    }

    private IEnumerator SpawnEnemyGroup(Vector3 spawnPos, WaveEnemyData waveEnemyData, WaitForSeconds spawnDelay)
    {
        if (waveEnemyData == null || waveEnemyData.EnemyData == null)
        {
            yield break;
        }
        
        Enemy prefab = waveEnemyData.EnemyData.EnemyPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"EnemyData '{waveEnemyData.EnemyData.name}' does not have an Enemy prefab.", this);
            yield break;
        }

        levelPrefabs.Add(prefab);

        // Tạo sẵn pool theo số lượng của nhóm ở lần đầu prefab xuất hiện.
        SimplePool.PreLoad(prefab, waveEnemyData.Amount, enemyContainer);

        for (int i = 0; i < waveEnemyData.Amount; i++)
        {
            if (hasSpawnedInCurrentWave && spawnDelay != null)
            {
                // Enemy đầu tiên xuất hiện ngay; từ enemy thứ hai trở đi mới chờ để tạo nhịp spawn trong wave.
                yield return spawnDelay;
            }

            SpawnEnemy(spawnPos, waveEnemyData.EnemyData);
            hasSpawnedInCurrentWave = true;
        }
    }

    private void StopSpawnWave()
    {
        if (spawnWaveCoroutine != null)
        {
            StopCoroutine(spawnWaveCoroutine);
            spawnWaveCoroutine = null;
        }

        hasSpawnedInCurrentWave = false;
        isSpawningWave = false;
    }

    private void SpawnEnemy(UnityEngine.Vector3 spawnPos, EnemyDataSO enemyData)
    {
        Enemy prefab = enemyData.EnemyPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"EnemyData '{enemyData.name}' does not have an Enemy prefab.", this);
            return;
        }

        Enemy enemy = SimplePool.Spawn(
            prefab,
            spawnPos,
            prefab.transform.rotation,
            enemyContainer
        );

        if (enemy != null && !enemies.Contains(enemy))
        {
            // Object tái sử dụng từ pool chỉ được xuất hiện một lần trong danh sách target.
            enemies.Add(enemy);
        }

        if (enemy != null)
        {
            // Chỉ enemy spawn thành công mới được tính vào điều kiện hoàn thành wave.
            totalAliveEnemy++;
        }
    }


    public Enemy GetTarget()
    {
        if (enemies == null)
        {
            return null;
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy != null && enemy.isActiveAndEnabled && !enemy.IsDead)
            {
                return enemy;
            }
        }

        return null;
    }

    public Enemy GetNearestTarget(UnityEngine.Vector3 position)
    {
        if (enemies == null)
        {
            return null;
        }

        Enemy nearest = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.IsDead)
            {
                continue;
            }

            float distance = (enemy.transform.position - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = enemy;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}
