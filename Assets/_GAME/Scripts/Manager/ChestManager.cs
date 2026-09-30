using System;
using System.Collections.Generic;
using UnityEngine;

public partial class ChestManager : Singleton<ChestManager>
{
    [SerializeField] private List<ChestState> chestStates = new List<ChestState>
    {
        new ChestState(ChestType.NORMAL, 10, 10f),
        new ChestState(ChestType.ELITE, 5, 30f),
        new ChestState(ChestType.BOSS, 3, 60f)
    };
    [SerializeField] private string saveKey = "PLAYER_CHESTS";

    [NonSerialized] private int dataVersion;
    [NonSerialized] private bool lastSaveSucceeded;
    [NonSerialized] private IChestView chestView;

    public IReadOnlyList<ChestState> ChestStates => chestStates;
    public int DataVersion => dataVersion;
    public bool LastSaveSucceeded => lastSaveSucceeded;
    public bool IsInitialized { get; private set; }

    public void OnInit()
    {
        if (IsInitialized)
        {
            OnDespawn();
        }

        EnsureStates();
        for (int i = 0; i < chestStates.Count; i++)
        {
            chestStates[i].OnInit();
        }

        dataVersion = 0;
        lastSaveSucceeded = true;
        IsInitialized = true;
        if (!LoadGame())
        {
            lastSaveSucceeded = SaveGame();
        }
        RefreshInventoryFullStates(PlayerManager.Ins.Inventory);
        RefreshView();
    }

    public void OnDespawn()
    {
        if (IsInitialized)
        {
            lastSaveSucceeded = SaveGame();
        }

        IsInitialized = false;
        chestView = null;
    }

    private void Update()
    {
        if (!IsInitialized)
        {
            return;
        }

        Inventory inventory = PlayerManager.Ins.Inventory;
        RefreshInventoryFullStates(inventory);
        for (int i = 0; i < chestStates.Count; i++)
        {
            ChestState state = chestStates[i];
            if (!state.TickAutoTimer(Time.deltaTime))
            {
                continue;
            }

            if (!TryOpenChest(state.ChestType))
            {
                state.DelayAutoRetry(1f);
            }
        }
    }

    public ChestState GetState(ChestType chestType)
    {
        for (int i = 0; i < chestStates.Count; i++)
        {
            ChestState state = chestStates[i];
            if (state != null && state.ChestType == chestType)
            {
                return state;
            }
        }
        return null;
    }

    public bool TryEnqueue(ChestReward reward)
    {
        ChestState state = reward != null ? GetState(reward.ChestType) : null;
        if (!IsInitialized || state == null || !state.TryEnqueue(reward))
        {
            // Queue day thi reward da roll bi bo, dung nhu luong drop da thiet ke.
            return false;
        }

        RefreshInventoryFullState(state, PlayerManager.Ins.Inventory);
        CompleteDataFlow();
        return true;
    }

    public bool IncreaseMaxStorage(ChestType chestType, int amount)
    {
        if (!IsInitialized)
        {
            return false;
        }

        ChestState state = GetState(chestType);
        return CompleteStateChange(state != null && state.IncreaseMaxStorage(amount));
    }

    public bool SetAutoOpenUnlocked(ChestType chestType, bool unlocked)
    {
        if (!IsInitialized)
        {
            return false;
        }

        ChestState state = GetState(chestType);
        return CompleteStateChange(state != null && state.SetAutoOpenUnlocked(unlocked));
    }

    public bool SetAutoOpenEnabled(ChestType chestType, bool enabled)
    {
        if (!IsInitialized)
        {
            return false;
        }

        ChestState state = GetState(chestType);
        return CompleteStateChange(state != null && state.SetAutoOpenEnabled(enabled));
    }

    public bool SetAutoOpenInterval(ChestType chestType, float seconds)
    {
        if (!IsInitialized)
        {
            return false;
        }

        ChestState state = GetState(chestType);
        return CompleteStateChange(state != null && state.SetAutoOpenInterval(seconds));
    }

    private bool CompleteStateChange(bool changed)
    {
        if (!IsInitialized || !changed)
        {
            return false;
        }

        CompleteDataFlow();
        return true;
    }

    private void RefreshInventoryFullStates(Inventory inventory)
    {
        for (int i = 0; i < chestStates.Count; i++)
        {
            RefreshInventoryFullState(chestStates[i], inventory);
        }
    }

    private void RefreshInventoryFullState(ChestState state, Inventory inventory)
    {
        ChestReward reward = state?.Peek();
        bool isFull = reward != null
            && (inventory == null || !inventory.CanAddItem(reward.Equipment));
        SetInventoryFullState(state, isFull);
    }

    private void SetInventoryFullState(ChestState state, bool isFull)
    {
        if (state != null && state.SetInventoryFull(isFull))
        {
            dataVersion++;
            RefreshView();
        }
    }

    private void EnsureStates()
    {
        chestStates ??= new List<ChestState>();
        List<ChestState> normalized = new List<ChestState>(3)
        {
            FindOrCreateState(ChestType.NORMAL, 10, 10f),
            FindOrCreateState(ChestType.ELITE, 5, 30f),
            FindOrCreateState(ChestType.BOSS, 3, 60f)
        };
        chestStates = normalized;
    }

    private ChestState FindOrCreateState(
        ChestType chestType,
        int defaultStorage,
        float defaultInterval)
    {
        ChestState state = GetState(chestType);
        return state ?? new ChestState(chestType, defaultStorage, defaultInterval);
    }
}
