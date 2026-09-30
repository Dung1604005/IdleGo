using System;
using System.Collections.Generic;

[Serializable]
public class ChestManagerSaveData
{
    public List<ChestStateSaveData> states = new List<ChestStateSaveData>();
}

[Serializable]
public class ChestStateSaveData
{
    public ChestType chestType;
    public int maxStorage;
    public bool autoOpenUnlocked;
    public bool autoOpenEnabled;
    public float autoOpenInterval;
    public float remainingAutoOpenTime;
    public List<ChestRewardSaveData> rewards = new List<ChestRewardSaveData>();
}

[Serializable]
public class ChestRewardSaveData
{
    public string itemId;
    public string instanceId;
    public float qualityRoll = 1f;
}
