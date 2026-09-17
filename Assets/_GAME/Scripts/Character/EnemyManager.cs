using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private Transform enemyContainer;

    private int nextSpawnPointIndex;

    public IReadOnlyList<Enemy> Enemies => enemies;
    public bool IsInitialized { get; private set; }

    public void OnInit()
    {
        if (enemies == null)
        {
            enemies = new List<Enemy>();
        }

        enemies.RemoveAll(enemy => enemy == null);
        nextSpawnPointIndex = 0;
        IsInitialized = true;
    }

    public void OnDespawn()
    {
        nextSpawnPointIndex = 0;
        IsInitialized = false;
    }

    public void SpawnWave(WaveData waveData)
    {
        if (!IsInitialized || waveData == null || waveData.Enemies == null)
        {
            return;
        }

        IReadOnlyList<WaveEnemyData> waveEnemies = waveData.Enemies;
        for (int i = 0; i < waveEnemies.Count; i++)
        {
            SpawnEnemyGroup(waveEnemies[i]);
        }
    }

    private void SpawnEnemyGroup(WaveEnemyData waveEnemyData)
    {
        if (waveEnemyData == null || waveEnemyData.EnemyData == null)
        {
            return;
        }

        for (int i = 0; i < waveEnemyData.Amount; i++)
        {
            SpawnEnemy(waveEnemyData.EnemyData);
        }
    }

    private void SpawnEnemy(EnemyDataSO enemyData)
    {
        Enemy prefab = enemyData.EnemyPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"EnemyData '{enemyData.name}' does not have an Enemy prefab.", this);
            return;
        }

        Enemy enemy = Instantiate(
            prefab,
            GetNextSpawnPosition(),
            prefab.transform.rotation,
            enemyContainer
        );

        enemies.Add(enemy);
    }

    private Vector3 GetNextSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            return transform.position;
        }

        // Spawn point chạy vòng tròn để một wave có thể dùng lại danh sách điểm spawn.
        int checkedPointCount = 0;
        while (checkedPointCount < spawnPoints.Count)
        {
            int pointIndex = nextSpawnPointIndex % spawnPoints.Count;
            nextSpawnPointIndex = (pointIndex + 1) % spawnPoints.Count;
            checkedPointCount++;

            Transform spawnPoint = spawnPoints[pointIndex];
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }
        }

        return transform.position;
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

    public Enemy GetNearestTarget(Vector3 position)
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
