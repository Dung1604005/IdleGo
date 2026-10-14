using System.Collections.Generic;
using UnityEngine;

public partial class PlayerManager
{
    public bool IsTeamDefeated
    {
        get
        {
            if (!IsInitialized || Players.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i] != null && !Players[i].IsDead)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public void PrepareTeamForLevelStart(Vector3 spawnPosition)
    {
        IReadOnlyList<Player> players = Players;
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            if (player == null)
            {
                continue;
            }

            // Moi lan bat dau level, toan bo team quay lai dung diem xuat phat cua level.
            player.transform.position = spawnPosition;
            player.PrepareForLevelStart();
        }
    }
}
