using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "IdleGo/Level/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [SerializeField] private int levelIndex;
    [SerializeField] private String nameLevel;
    [SerializeField, Min(1)] private int levelRequired = 1;
    [SerializeField] private List<WaveData> waves = new List<WaveData>();

    [Header("Equipment Sources")]
    [SerializeField] private EquipmentSourceSO normalEquipmentSource;
    [SerializeField] private EquipmentSourceSO eliteEquipmentSource;
    [SerializeField] private EquipmentSourceSO bossEquipmentSource;

    public int LevelIndex => levelIndex;
    public int LevelRequired => Mathf.Max(1, levelRequired);

    public int WaveCount => waves != null ? waves.Count : 0;

    public WaveData GetWave(int waveIndex)
    {
        if (waves == null || waveIndex < 0 || waveIndex >= waves.Count)
        {
            return null;
        }

        return waves[waveIndex];
    }

    public int GetTotalWave()
    {
        return waves.Count;
    }

    public bool CanEnter(int globalPlayerLevel)
    {
        return globalPlayerLevel >= LevelRequired;
    }

    public EquipmentSourceSO GetEquipmentSource(ChestType chestType)
    {
        return chestType switch
        {
            ChestType.NORMAL => normalEquipmentSource,
            ChestType.ELITE => eliteEquipmentSource,
            ChestType.BOSS => bossEquipmentSource,
            _ => null
        };
    }
}
