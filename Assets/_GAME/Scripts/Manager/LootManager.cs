using System.Collections.Generic;
using UnityEngine;

public class LootManager : Singleton<LootManager>
{
    [SerializeField] private LevelLootController lootController = new LevelLootController();

    private LevelDataSO currentLevelData;

    public IReadOnlyList<DroppedChest> DroppedChests => lootController.DroppedChests;
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

    public bool TryOpenChest(int chestIndex, out Equipment equipment)
    {
        if (!IsInitialized)
        {
            equipment = null;
            return false;
        }

        return lootController.TryOpenChest(
            chestIndex,
            PlayerManager.Ins.Inventory,
            out equipment
        );
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

        // LootManager tu quyet dinh source va drop rate tu data cua level dang choi.
        lootController.TryDropChest(enemyData.EnemyType, currentLevelData);
    }
}
