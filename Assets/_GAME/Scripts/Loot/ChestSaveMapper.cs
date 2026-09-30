using System.Collections.Generic;
using UnityEngine;

public static class ChestSaveMapper
{
    public static ChestManagerSaveData Create(IReadOnlyList<ChestState> states)
    {
        ChestManagerSaveData saveData = new ChestManagerSaveData();
        if (states == null)
        {
            return saveData;
        }

        for (int i = 0; i < states.Count; i++)
        {
            ChestState state = states[i];
            if (state != null)
            {
                saveData.states.Add(CreateState(state));
            }
        }
        return saveData;
    }

    public static void Apply(
        ChestManagerSaveData saveData,
        ChestManager chestManager,
        Inventory inventory)
    {
        if (saveData?.states == null || chestManager == null || inventory == null)
        {
            return;
        }

        for (int i = 0; i < saveData.states.Count; i++)
        {
            ChestStateSaveData savedState = saveData.states[i];
            ChestState state = chestManager.GetState(savedState.chestType);
            if (state == null)
            {
                continue;
            }

            RestoreState(state, savedState, inventory);
        }
    }

    private static ChestStateSaveData CreateState(ChestState state)
    {
        ChestStateSaveData result = new ChestStateSaveData
        {
            chestType = state.ChestType,
            maxStorage = state.MaxStorage,
            autoOpenUnlocked = state.AutoOpenUnlocked,
            autoOpenEnabled = state.AutoOpenEnabled,
            autoOpenInterval = state.AutoOpenInterval,
            remainingAutoOpenTime = state.RemainingAutoOpenTime
        };

        IReadOnlyList<ChestReward> rewards = state.Rewards;
        for (int i = 0; i < rewards.Count; i++)
        {
            Equipment equipment = rewards[i]?.Equipment;
            if (equipment?.Data == null || string.IsNullOrWhiteSpace(equipment.Data.ItemId))
            {
                continue;
            }

            result.rewards.Add(new ChestRewardSaveData
            {
                itemId = equipment.Data.ItemId,
                instanceId = equipment.InstanceId,
                qualityRoll = equipment.QualityRoll
            });
        }
        return result;
    }

    private static void RestoreState(
        ChestState state,
        ChestStateSaveData savedState,
        Inventory inventory)
    {
        state.RestoreSettings(
            savedState.maxStorage,
            savedState.autoOpenUnlocked,
            savedState.autoOpenEnabled,
            savedState.autoOpenInterval,
            savedState.remainingAutoOpenTime);

        List<ChestReward> rewards = new List<ChestReward>();
        if (savedState.rewards != null)
        {
            RestoreRewards(savedState.chestType, savedState.rewards, inventory, rewards);
        }
        state.RestoreRewards(rewards);
    }

    private static void RestoreRewards(
        ChestType chestType,
        IReadOnlyList<ChestRewardSaveData> savedRewards,
        Inventory inventory,
        List<ChestReward> output)
    {
        HashSet<string> instanceIds = new HashSet<string>();
        for (int i = 0; i < savedRewards.Count; i++)
        {
            ChestRewardSaveData savedReward = savedRewards[i];
            ItemSO itemData = inventory.GetItemData(savedReward.itemId);
            if (itemData is not EquipmentDataSO equipmentData)
            {
                continue;
            }

            Equipment equipment = new Equipment(equipmentData);
            equipment.RestoreInstanceId(savedReward.instanceId);
            if (!instanceIds.Add(equipment.InstanceId))
            {
                continue;
            }

            equipment.RestoreQualityRoll(savedReward.qualityRoll);
            output.Add(new ChestReward(chestType, equipment));
        }
    }
}
