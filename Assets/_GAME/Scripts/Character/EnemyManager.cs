using System.Collections.Generic;

using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private Transform enemyContainer;

    [SerializeField] private int totalAliveEnemy;

    private readonly HashSet<Enemy> levelPrefabs = new HashSet<Enemy>();

    public IReadOnlyList<Enemy> Enemies => enemies;
    public int TotalAliveEnemy => Mathf.Max(0, totalAliveEnemy);
    public bool HasAliveEnemies => TotalAliveEnemy > 0;
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
        if (!IsInitialized || waveData == null || waveData.Enemies == null)
        {
            return;
        }

        totalAliveEnemy = 0;
        IReadOnlyList<WaveEnemyData> waveEnemies = waveData.Enemies;
        for (int i = 0; i < waveEnemies.Count; i++)
        {
            SpawnEnemyGroup(waveData.SpawnPosition, waveEnemies[i]);
        }
    }

    private void SpawnEnemyGroup(Vector3 spawnPos, WaveEnemyData waveEnemyData)
    {
        if (waveEnemyData == null || waveEnemyData.EnemyData == null)
        {
            return;
        }
        
        Enemy prefab = waveEnemyData.EnemyData.EnemyPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"EnemyData '{waveEnemyData.EnemyData.name}' does not have an Enemy prefab.", this);
            return;
        }

        levelPrefabs.Add(prefab);

        // Tạo sẵn pool theo số lượng của nhóm ở lần đầu prefab xuất hiện.
        SimplePool.PreLoad(prefab, waveEnemyData.Amount, enemyContainer);

        for (int i = 0; i < waveEnemyData.Amount; i++)
        {
            SpawnEnemy(spawnPos, waveEnemyData.EnemyData);
        }
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
