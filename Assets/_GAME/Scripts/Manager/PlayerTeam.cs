using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerTeam
{
    [SerializeField] private List<Player> teamPlayers = new List<Player>();
    [SerializeField] private Player teamSlotLevelOwner;
    [SerializeField, Min(1)] private int secondMemberUnlockLevel = 10;
    [SerializeField, Min(1)] private int thirdMemberUnlockLevel = 20;

    [NonSerialized] private PlayerRoster roster;

    public IReadOnlyList<Player> Players => teamPlayers;
    public int Count => teamPlayers != null ? teamPlayers.Count : 0;
    public int SecondMemberUnlockLevel => Mathf.Max(1, secondMemberUnlockLevel);
    public int ThirdMemberUnlockLevel => Mathf.Max(SecondMemberUnlockLevel, thirdMemberUnlockLevel);
    public int ProgressLevel => GetProgressLevel();
    public int UnlockedSlotCount => GetUnlockedSlotCount();

    public void OnInit(PlayerRoster playerRoster)
    {
        roster = playerRoster;
        teamPlayers ??= new List<Player>();
        teamPlayers.Clear();

        if (roster?.Starter != null)
        {
            teamPlayers.Add(roster.Starter);
        }
    }

    public Player GetPlayer(int teamIndex)
    {
        return teamIndex >= 0 && teamIndex < Count ? teamPlayers[teamIndex] : null;
    }
    public String GetNextPlayerId(String characterId)
    {
        if (Count == 0)
        {
            return null;
        }

        for(int i = 0; i < teamPlayers.Count; i++)
        {
            if (teamPlayers[i].CharacterId.Equals(characterId))
            {
                return teamPlayers[(i + 1) % (teamPlayers.Count)].CharacterId;
            }
        }
        return null;
    }
    public String GetPrevPlayerId(String characterId)
    {
        if (Count == 0)
        {
            return null;
        }

        for(int i = 0; i < teamPlayers.Count; i++)
        {
            if (teamPlayers[i].CharacterId.Equals(characterId))
            {
                return teamPlayers[(i - 1 + teamPlayers.Count) % (teamPlayers.Count)].CharacterId;
            }
        }
        return null;
    }

    public bool Contains(Player player)
    {
        return player != null && teamPlayers.Contains(player);
    }

    public bool IsSlotUnlocked(int teamSlotIndex)
    {
        return teamSlotIndex >= 0
            && teamSlotIndex < PlayerManager.MaxTeamSize
            && teamSlotIndex < UnlockedSlotCount;
    }

    public bool CanAdd(Player player)
    {
        return roster != null
            && roster.IsUnlocked(player)
            && !Contains(player)
            && Count < UnlockedSlotCount;
    }

    public bool Add(Player player, bool updateActiveState)
    {
        if (!CanAdd(player))
        {
            return false;
        }

        teamPlayers.Add(player);
        if (updateActiveState)
        {
            player.gameObject.SetActive(true);
        }
        return true;
    }

    public bool Remove(Player player, bool updateActiveState)
    {
        if (player == null || !teamPlayers.Remove(player))
        {
            return false;
        }

        if (updateActiveState)
        {
            player.gameObject.SetActive(false);
        }
        return true;
    }

    public void ResetForLoad()
    {
        teamPlayers.Clear();
    }

    public void RefreshActiveStates()
    {
        if (roster == null)
        {
            return;
        }

        IReadOnlyList<Player> characters = roster.Players;
        for (int i = 0; i < characters.Count; i++)
        {
            Player player = characters[i];
            if (player != null)
            {
                player.gameObject.SetActive(Contains(player));
            }
        }
    }

    public Player GetTarget()
    {
        for (int i = 0; i < Count; i++)
        {
            Player player = teamPlayers[i];
            if (CanBeTargeted(player))
            {
                return player;
            }
        }

        return null;
    }

    public Player GetNearestTarget(Vector3 position)
    {
        Player nearest = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < Count; i++)
        {
            Player player = teamPlayers[i];
            if (!CanBeTargeted(player))
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

    private int GetProgressLevel()
    {
        Player levelOwner = teamSlotLevelOwner;
        if (levelOwner == null || roster == null || !roster.Contains(levelOwner))
        {
            levelOwner = roster?.Starter;
        }

        return levelOwner != null && levelOwner.Stats != null
            ? Mathf.Max(1, levelOwner.Stats.CurrentLevel)
            : 1;
    }

    private int GetUnlockedSlotCount()
    {
        if (ProgressLevel >= ThirdMemberUnlockLevel)
        {
            return 3;
        }

        return ProgressLevel >= SecondMemberUnlockLevel ? 2 : 1;
    }

    private static bool CanBeTargeted(Player player)
    {
        return player != null && player.isActiveAndEnabled && !player.IsDead;
    }
}
