using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerRoster
{
    [SerializeField] private List<Player> players = new List<Player>();

    [NonSerialized] private HashSet<string> unlockedCharacterIds =
        new HashSet<string>(StringComparer.Ordinal);

    public IReadOnlyList<Player> Players => players;
    public Player Starter => players != null && players.Count > 0 ? players[0] : null;

    public void OnInit()
    {
        EnsureCollections();
        InitializeCharacters();
        ResetUnlocks();
        Unlock(Starter);
    }

    public Player GetCharacter(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            if (player != null && player.CharacterId == characterId)
            {
                return player;
            }
        }

        return null;
    }

    public bool Contains(Player player)
    {
        return player != null && players.Contains(player);
    }

    public bool IsUnlocked(Player player)
    {
        return Contains(player)
            && HasCharacterId(player)
            && unlockedCharacterIds.Contains(player.CharacterId);
    }

    public bool Unlock(Player player)
    {
        return Contains(player)
            && HasCharacterId(player)
            && unlockedCharacterIds.Add(player.CharacterId);
    }

    public void ResetUnlocks()
    {
        EnsureCollections();
        unlockedCharacterIds.Clear();
    }

    public void RestoreAllHealth()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i]?.Stats.RestoreHealthToMax();
        }
    }

    private void EnsureCollections()
    {
        players ??= new List<Player>();
        unlockedCharacterIds ??= new HashSet<string>(StringComparer.Ordinal);
        players.RemoveAll(player => player == null);
    }

    private void InitializeCharacters()
    {
        HashSet<string> characterIds = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < players.Count; i++)
        {
            InitializeCharacter(players[i], characterIds);
        }
    }

    private static void InitializeCharacter(Player player, HashSet<string> characterIds)
    {
        player.OnInit();
        if (!HasCharacterId(player))
        {
            Debug.LogError($"Player '{player.name}' needs a CharacterDataSO with a CharacterId.");
            return;
        }

        if (!characterIds.Add(player.CharacterId))
        {
            Debug.LogError($"Duplicate player CharacterId '{player.CharacterId}'.");
        }
    }

    private static bool HasCharacterId(Player player)
    {
        return player != null && !string.IsNullOrWhiteSpace(player.CharacterId);
    }
}
