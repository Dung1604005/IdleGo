using System;
using UnityEngine;

[Serializable]
public class WaveEnemyData
{
    [SerializeField] private EnemyDataSO enemyData;
    [SerializeField, Min(1)] private int amount = 1;

    public EnemyDataSO EnemyData => enemyData;
    public int Amount => Mathf.Max(1, amount);
}
