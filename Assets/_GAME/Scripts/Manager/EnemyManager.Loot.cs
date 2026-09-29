using System.Collections.Generic;

public partial class EnemyManager
{
    private readonly Dictionary<Enemy, EnemyDataSO> enemyLootData =
        new Dictionary<Enemy, EnemyDataSO>();

    private void TrackEnemyLootData(Enemy enemy, EnemyDataSO enemyData)
    {
        if (enemy != null && enemyData != null)
        {
            enemyLootData[enemy] = enemyData;
        }
    }

    private void DropLootForDefeatedEnemy(Enemy enemy)
    {
        if (enemy == null || !enemyLootData.TryGetValue(enemy, out EnemyDataSO enemyData))
        {
            return;
        }

        // Chi DespawnEnemy sau animation chet moi vao day; don level khong sinh loot.
        enemyLootData.Remove(enemy);
        LootManager.Ins.OnEnemyDefeated(enemyData);
    }

    private void ClearEnemyLootData()
    {
        enemyLootData.Clear();
    }
}
