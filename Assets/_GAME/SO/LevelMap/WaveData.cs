using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    [SerializeField] private int waveIndex;
    [SerializeField] private List<WaveEnemyData> enemies = new List<WaveEnemyData>();

    [SerializeField] private Vector3 spawnPosition;

    public IReadOnlyList<WaveEnemyData> Enemies => enemies;

    public int WaveIndex => waveIndex;

    public Vector3 SpawnPosition => spawnPosition;
}
