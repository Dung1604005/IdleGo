using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
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

    public Enemy GetNearestTarget(Vector3 position)
    {
        if (enemies == null)
        {
            return null;
        }

        Enemy nearest = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.IsDead)
            {
                continue;
            }

            float distance = (enemy.transform.position - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = enemy;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}
