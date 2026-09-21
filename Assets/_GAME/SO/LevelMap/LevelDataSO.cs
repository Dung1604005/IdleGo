using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "IdleGo/Level/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [SerializeField] private String nameLevel;
    [SerializeField] private List<WaveData> waves = new List<WaveData>();

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
}
