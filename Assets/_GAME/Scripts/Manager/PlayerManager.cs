using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public const int MaxTeamSize = 3;

    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private Inventory inventory = new Inventory();

    public IReadOnlyList<Player> Players => players;
    public Inventory Inventory => inventory;
    public int TeamCount => Mathf.Min(players != null ? players.Count : 0, MaxTeamSize);
    public bool IsInitialized { get; private set; }

    public void OnInit()
    {
        OnDespawn();
        inventory ??= new Inventory();
        inventory.OnInit(this);
        IsInitialized = true;

        if (players != null && players.Count > MaxTeamSize)
        {
            Debug.LogWarning($"PlayerManager only uses the first {MaxTeamSize} players.");
        }
    }

    public void OnDespawn()
    {
        inventory?.OnDespawn();
        IsInitialized = false;
    }

    public Player GetPlayer(int teamIndex)
    {
        return teamIndex >= 0 && teamIndex < TeamCount ? players[teamIndex] : null;
    }

    public bool ContainsPlayer(Player player)
    {
        if (player == null)
        {
            return false;
        }

        for (int i = 0; i < TeamCount; i++)
        {
            if (ReferenceEquals(players[i], player))
            {
                return true;
            }
        }

        return false;
    }

    public bool Equip(Player player, Item item)
    {
        return inventory != null && inventory.Equip(player, item);
    }

    public bool Unequip(Player player, Item item)
    {
        return inventory != null && inventory.Unequip(player, item);
    }

    public bool Unequip(Player player, EquipmentType equipmentType)
    {
        return inventory != null && inventory.Unequip(player, equipmentType);
    }

    public Player GetTarget()
    {
        if (players == null)
        {
            return null;
        }

        for (int i = 0; i < TeamCount; i++)
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

        for (int i = 0; i < TeamCount; i++)
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
