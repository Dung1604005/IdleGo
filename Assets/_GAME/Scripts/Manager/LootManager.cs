using UnityEngine;

public class LootManager : Singleton<LootManager>
{
    [SerializeField] private LevelLootController lootController = new LevelLootController();

    private LevelDataSO currentLevelData;

    public bool IsInitialized { get; private set; }

    public void OnInit()
    {
        lootController ??= new LevelLootController();
        lootController.OnInit();
        currentLevelData = null;
        IsInitialized = true;
    }

    public void OnDespawn()
    {
        lootController?.OnDespawn();
        currentLevelData = null;
        IsInitialized = false;
    }

    public void SetLevelData(LevelDataSO levelData)
    {
        currentLevelData = levelData;
    }

    public void ClearLevelData()
    {
        currentLevelData = null;
    }

    public float GetChestDropRate(ChestType chestType)
    {
        return lootController.GetDropRate(chestType);
    }

    public void IncreaseChestDropRate(ChestType chestType, float amount)
    {
        lootController.IncreaseDropRate(chestType, amount);
    }

    public void OnEnemyDefeated(EnemyDataSO enemyData)
    {
        if (!IsInitialized || enemyData == null || currentLevelData == null)
        {
            return;
        }

        // Roll reward ngay luc drop; queue day thi ChestManager tu choi va reward bi bo.
        if (lootController.TryRollReward(
            enemyData.EnemyType,
            currentLevelData,
            out ChestReward reward))
        {
            ChestManager.Ins.TryEnqueue(reward);
        }
    }
}
