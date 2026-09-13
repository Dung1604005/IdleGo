using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();

    public Enemy GetTarget()
    {
        if (enemies == null)
        {
            return null;
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy != null && enemy.isActiveAndEnabled && !enemy.IsDead)
            {
                return enemy;
            }
        }

        return null;
    }
}
