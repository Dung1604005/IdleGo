using System;
using UnityEngine;

[Serializable]
public class LevelLootController
{
    [SerializeField] private ChestDropRates chestDropRates = new ChestDropRates();

    public void OnInit()
    {
        chestDropRates ??= new ChestDropRates();
        chestDropRates.ResetBonuses();
    }

    public void OnDespawn()
    {
        chestDropRates?.ResetBonuses();
    }

    public bool TryRollReward(
        EnemyType enemyType,
        LevelDataSO levelData,
        out ChestReward reward)
    {
        reward = null;
        if (levelData == null || !TryGetChestType(enemyType, out ChestType chestType))
        {
            return false;
        }

        // Mapping mot-cham-mot dam bao boss chest khong the roi tu Normal hoac Elite.
        if (!chestDropRates.Roll(chestType))
        {
            return false;
        }

        return TryCreateReward(chestType, levelData, out reward);
    }

    public bool TryCreateReward(
        ChestType chestType,
        LevelDataSO levelData,
        out ChestReward reward)
    {
        reward = null;
        EquipmentSourceSO source = levelData != null
            ? levelData.GetEquipmentSource(chestType)
            : null;
        if (source == null)
        {
            return false;
        }

        if (!source.TryRollEquipment(out EquipmentDataSO equipmentData))
        {
            return false;
        }

        // Reward duoc roll mot lan khi roi va giu nguyen den luc mo chest.
        Equipment equipment = new Equipment(equipmentData);
        equipment.SetQualityRoll(EquipmentQualityRoll.Roll(equipmentData.RarityType));
        reward = new ChestReward(chestType, equipment);
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
