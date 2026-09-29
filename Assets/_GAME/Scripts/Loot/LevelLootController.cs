using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelLootController
{
    [SerializeField] private ChestDropRates chestDropRates = new ChestDropRates();
    [SerializeField] private List<DroppedChest> droppedChests = new List<DroppedChest>();

    public IReadOnlyList<DroppedChest> DroppedChests => droppedChests;

    public void OnInit()
    {
        droppedChests ??= new List<DroppedChest>();
        droppedChests.Clear();
        chestDropRates ??= new ChestDropRates();
        chestDropRates.ResetBonuses();
    }

    public void OnDespawn()
    {
        droppedChests?.Clear();
        chestDropRates?.ResetBonuses();
    }

    public bool TryDropChest(EnemyType enemyType, LevelDataSO levelData)
    {
        if (levelData == null || !TryGetChestType(enemyType, out ChestType chestType))
        {
            return false;
        }

        // Mapping mot-cham-mot dam bao boss chest khong the roi tu Normal hoac Elite.
        if (!chestDropRates.Roll(chestType))
        {
            return false;
        }

        EquipmentSourceSO source = levelData.GetEquipmentSource(chestType);
        if (source == null)
        {
            return false;
        }

        droppedChests.Add(new DroppedChest(chestType, source));
        return true;
    }

    public bool TryOpenChest(int chestIndex, Inventory inventory, out Equipment equipment)
    {
        equipment = null;
        if (inventory == null
            || chestIndex < 0
            || chestIndex >= droppedChests.Count
            || !droppedChests[chestIndex].TryCreateEquipment(out Equipment rolledEquipment))
        {
            return false;
        }

        // Inventory tu save JSON roi moi refresh UI; chi xoa chest khi item da them thanh cong.
        if (!inventory.AddItem(rolledEquipment, 1))
        {
            return false;
        }

        equipment = rolledEquipment;
        droppedChests.RemoveAt(chestIndex);
        return true;
    }

    public float GetDropRate(ChestType chestType)
    {
        return chestDropRates.GetCurrentRate(chestType);
    }

    public void IncreaseDropRate(ChestType chestType, float amount)
    {
        chestDropRates.Increase(chestType, amount);
    }

    private static bool TryGetChestType(EnemyType enemyType, out ChestType chestType)
    {
        chestType = enemyType switch
        {
            EnemyType.NORMAL => ChestType.NORMAL,
            EnemyType.ELITE => ChestType.ELITE,
            EnemyType.BOSS => ChestType.BOSS,
            _ => default
        };
        return enemyType == EnemyType.NORMAL
            || enemyType == EnemyType.ELITE
            || enemyType == EnemyType.BOSS;
    }
}
