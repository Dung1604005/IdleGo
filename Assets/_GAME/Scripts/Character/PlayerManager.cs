using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<Player> players = new List<Player>();

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
}
