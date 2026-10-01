using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChestState
{
    [SerializeField] private ChestType chestType;
    [SerializeField, Min(1)] private int maxStorage = 10;
    [SerializeField] private bool autoOpenUnlocked;
    [SerializeField] private bool autoOpenEnabled;
    [SerializeField, Min(0.1f)] private float autoOpenInterval = 10f;
    [SerializeField] private float remainingAutoOpenTime;
    [SerializeField] private List<ChestReward> rewards = new List<ChestReward>();

    [NonSerialized] private bool isInventoryFull;
    [NonSerialized] private bool isOpening;
    [NonSerialized] private int receivedVersion;

    public ChestState(ChestType type, int storage, float interval)
    {
        chestType = type;
        maxStorage = Mathf.Max(1, storage);
        autoOpenInterval = Mathf.Max(0.1f, interval);
        remainingAutoOpenTime = autoOpenInterval;
    }

    public ChestType ChestType => chestType;
    public int Count => rewards != null ? rewards.Count : 0;
    public int MaxStorage => Mathf.Max(1, maxStorage);
    public bool IsStorageFull => Count >= MaxStorage;
    public bool AutoOpenUnlocked => autoOpenUnlocked;
    public bool AutoOpenEnabled => autoOpenUnlocked && autoOpenEnabled;
    public float AutoOpenInterval => Mathf.Max(0.1f, autoOpenInterval);
    public float RemainingAutoOpenTime => Mathf.Max(0f, remainingAutoOpenTime);
    public bool IsInventoryFull => isInventoryFull;
    public bool IsOpening => isOpening;
    public int ReceivedVersion => receivedVersion;
    public bool CanOpen => Count > 0 && !isInventoryFull && !isOpening;
    public bool CanAutoOpen => AutoOpenEnabled && CanOpen;
    public IReadOnlyList<ChestReward> Rewards => rewards;

    public void OnInit()
    {
        rewards ??= new List<ChestReward>();
        maxStorage = Mathf.Max(1, maxStorage);
        autoOpenInterval = Mathf.Max(0.1f, autoOpenInterval);
        remainingAutoOpenTime = remainingAutoOpenTime > 0f
            ? Mathf.Min(remainingAutoOpenTime, autoOpenInterval)
            : autoOpenInterval;
        isInventoryFull = false;
        isOpening = false;
        receivedVersion = 0;
    }

    public bool TryEnqueue(ChestReward reward)
    {
        if (reward == null || !reward.IsValid || reward.ChestType != chestType || IsStorageFull)
        {
            return false;
        }

        bool wasEmpty = Count == 0;
        rewards.Add(reward);
        // Chi reward vua nhan trong gameplay moi tang moc thong bao UI.
        receivedVersion++;
        if (wasEmpty)
        {
            ResetAutoTimer();
        }
        return true;
    }

    public ChestReward Peek()
    {
        return Count > 0 ? rewards[0] : null;
    }

    public bool RemoveFront()
    {
        if (Count == 0)
        {
            return false;
        }

        rewards.RemoveAt(0);
        isInventoryFull = false;
        return true;
    }

    public bool BeginOpening()
    {
        if (isOpening || !CanOpen)
        {
            return false;
        }

        isOpening = true;
        return true;
    }

    public bool EndOpening()
    {
        if (!isOpening)
        {
            return false;
        }

        isOpening = false;
        return true;
    }

    public bool IncreaseMaxStorage(int amount)
    {
        if (amount <= 0 || maxStorage > int.MaxValue - amount)
        {
            return false;
        }

        maxStorage += amount;
        return true;
    }

    public bool SetAutoOpenUnlocked(bool unlocked)
    {
        if (autoOpenUnlocked == unlocked)
        {
            return false;
        }

        autoOpenUnlocked = unlocked;
        if (!unlocked)
        {
            autoOpenEnabled = false;
        }
        ResetAutoTimer();
        return true;
    }

    public bool SetAutoOpenEnabled(bool enabled)
    {
        bool nextValue = autoOpenUnlocked && enabled;
        if (autoOpenEnabled == nextValue)
        {
            return false;
        }

        autoOpenEnabled = nextValue;
        ResetAutoTimer();
        return true;
    }

    public bool SetAutoOpenInterval(float seconds)
    {
        float nextValue = Mathf.Max(0.1f, seconds);
        if (Mathf.Approximately(autoOpenInterval, nextValue))
        {
            return false;
        }

        autoOpenInterval = nextValue;
        ResetAutoTimer();
        return true;
    }

    public bool TickAutoTimer(float deltaTime)
    {
        if (!CanAutoOpen)
        {
            return false;
        }

        remainingAutoOpenTime = Mathf.Max(0f, remainingAutoOpenTime - deltaTime);
        return remainingAutoOpenTime <= 0f;
    }

    public void DelayAutoRetry(float seconds)
    {
        remainingAutoOpenTime = Mathf.Max(0.1f, seconds);
    }

    public void ResetAutoTimer()
    {
        remainingAutoOpenTime = AutoOpenInterval;
    }

    public bool SetInventoryFull(bool value)
    {
        if (isInventoryFull == value)
        {
            return false;
        }

        isInventoryFull = value;
        return true;
    }

    internal void RestoreRewards(IReadOnlyList<ChestReward> loadedRewards)
    {
        rewards.Clear();
        if (loadedRewards == null)
        {
            return;
        }

        for (int i = 0; i < loadedRewards.Count && rewards.Count < MaxStorage; i++)
        {
            ChestReward reward = loadedRewards[i];
            if (reward != null && reward.IsValid && reward.ChestType == chestType)
            {
                rewards.Add(reward);
            }
        }
    }

    internal void RestoreSettings(
        int storage,
        bool autoUnlocked,
        bool autoEnabled,
        float interval,
        float remainingTime)
    {
        maxStorage = Mathf.Max(1, storage);
        autoOpenUnlocked = autoUnlocked;
        autoOpenEnabled = autoUnlocked && autoEnabled;
        autoOpenInterval = Mathf.Max(0.1f, interval);
        remainingAutoOpenTime = Mathf.Clamp(remainingTime, 0f, autoOpenInterval);
    }
}
