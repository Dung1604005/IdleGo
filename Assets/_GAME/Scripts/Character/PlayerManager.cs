using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private List<Player> players = new List<Player>();

    public IReadOnlyList<Player> Players => players;

    public Player GetTarget()
    {
        if (players == null)
        {
            return null;
        }

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            if (player != null && player.isActiveAndEnabled && !player.IsDead)
            {
                return player;
            }
        }

        return null;
    }

    public Player GetNearestTarget(Vector3 position)
    {
        if (players == null)
        {
            return null;
        }

        Player nearest = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            if (player == null || !player.isActiveAndEnabled || player.IsDead)
            {
                continue;
            }

            float distance = (player.transform.position - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = player;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}
