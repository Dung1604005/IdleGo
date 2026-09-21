using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    [SerializeField] private List<WaveEnemyData> enemies = new List<WaveEnemyData>();

    public IReadOnlyList<WaveEnemyData> Enemies => enemies;
}
