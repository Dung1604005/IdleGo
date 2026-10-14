public partial class EnemyManager
{
    public void ResetForLevelStart()
    {
        // Coroutine phai dung truoc khi clear de khong spawn them enemy sau khi reset.
        StopSpawnWave();
        DespawnTrackedEnemies();
        CollectRemainingLevelEnemies();
        enemies.Clear();
        ClearEnemyLootData();
        totalAliveEnemy = 0;
        IsInitialized = true;
    }

    private void DespawnTrackedEnemies()
    {
        enemies ??= new System.Collections.Generic.List<Enemy>();
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                SimplePool.Despawn(enemy);
            }
        }
    }

    private void CollectRemainingLevelEnemies()
    {
        // Pool la lop bao ve cuoi cung neu mot enemy active bi lech khoi danh sach manager.
        foreach (Enemy prefab in levelPrefabs)
        {
            if (prefab != null)
            {
                SimplePool.Collect(prefab);
            }
        }
    }
}
