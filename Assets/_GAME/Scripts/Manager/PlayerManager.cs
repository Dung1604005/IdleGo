using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public const int MaxTeamSize = 3;

    [SerializeField] private PlayerRoster roster = new PlayerRoster();
    [SerializeField] private PlayerTeam team = new PlayerTeam();
    [SerializeField] private Inventory inventory = new Inventory();
    [SerializeField] private PlayerProgression progression = new PlayerProgression();

    public IReadOnlyList<Player> AllPlayers => roster.Players;
    public IReadOnlyList<Player> Players => team.Players;
    public Inventory Inventory => inventory;
    public int TeamCount => team.Count;
    public int TeamProgressLevel => team.ProgressLevel;
    public int GlobalLevel => progression.GlobalLevel;
    public int UnlockedTeamSlotCount => team.UnlockedSlotCount;
    public bool IsInitialized { get; private set; }

    public void OnInit()
    {
        OnDespawn();
        roster ??= new PlayerRoster();
        team ??= new PlayerTeam();
        inventory ??= new Inventory();
        progression ??= new PlayerProgression();

        roster.OnInit();
        team.OnInit(roster);
        IsInitialized = true;
        inventory.OnInit(this);
        team.RefreshActiveStates();
    }

    public void OnDespawn()
    {
        inventory?.OnDespawn();
        IsInitialized = false;
    }

    public Player GetPlayer(int teamIndex)
    {
        return team.GetPlayer(teamIndex);
    }

    public String GetNextPlayerId(String characterId)
    {
        return team.GetNextPlayerId(characterId);
    }
    public String GetPrevPlayerId(String characterId)
    {
        return team.GetPrevPlayerId(characterId);
    }

    

    public Player GetCharacter(string characterId)
    {
        return roster.GetCharacter(characterId);
    }

    public bool ContainsPlayer(Player player)
    {
        return team.Contains(player);
    }

    public bool ContainsCharacter(Player player)
    {
        return roster.Contains(player);
    }

    public bool IsCharacterUnlocked(Player player)
    {
        return roster.IsUnlocked(player);
    }

    public bool IsTeamSlotUnlocked(int teamSlotIndex)
    {
        return team.IsSlotUnlocked(teamSlotIndex);
    }

    public bool CanAddToTeam(Player player)
    {
        return IsInitialized && team.CanAdd(player);
    }

    public bool UnlockCharacter(Player player)
    {
        if (!IsInitialized || !ContainsCharacter(player))
        {
            return false;
        }

        if (roster.IsUnlocked(player))
        {
            return true;
        }

        if (!roster.Unlock(player))
        {
            return false;
        }

        inventory.OnCharacterRosterChanged(true);
        return true;
    }

    public bool AddToTeam(Player player)
    {
        if (!IsInitialized || !team.Add(player, true))
        {
            return false;
        }

        inventory.OnCharacterRosterChanged(false);
        return true;
    }

    public bool RemoveFromTeam(Player player)
    {
        if (!IsInitialized || !team.Remove(player, true))
        {
            return false;
        }

        // Roi team chi thay doi trang thai chien dau, khong xoa data cua nhan vat.
        inventory.OnCharacterRosterChanged(false);
        return true;
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

    public bool EquipSkill(Player player, CombatSkill skill)
    {
        if (!CanChangeSkill(player) || !player.Combat.EquipSkill(skill))
        {
            return false;
        }

        inventory.OnCharacterRosterChanged(false);
        return true;
    }

    public bool UnequipSkill(Player player, CombatSkill skill)
    {
        if (!CanChangeSkill(player) || !player.Combat.UnequipSkill(skill))
        {
            return false;
        }

        inventory.OnCharacterRosterChanged(false);
        return true;
    }

    public bool SaveGame()
    {
        return inventory != null && inventory.SaveGame();
    }

    public bool SetGlobalLevel(int level)
    {
        if (!IsInitialized)
        {
            return false;
        }

        if (progression.SetLevel(level))
        {
            inventory.OnPlayerProgressChanged();
        }

        return true;
    }

    public Player GetTarget()
    {
        return team.GetTarget();
    }

    public Player GetNearestTarget(Vector3 position)
    {
        return team.GetNearestTarget(position);
    }

    internal void ResetRosterForLoad()
    {
        roster.ResetUnlocks();
        team.ResetForLoad();
    }

    internal bool UnlockCharacterForLoad(Player player)
    {
        return roster.IsUnlocked(player) || roster.Unlock(player);
    }

    internal bool AddToTeamForLoad(Player player)
    {
        return team.Add(player, false);
    }

    internal void RefreshTeamActiveStates()
    {
        team.RefreshActiveStates();
    }

    internal void RestoreAllCharactersHealth()
    {
        roster.RestoreAllHealth();
    }

    internal void RestoreGlobalLevel(int level)
    {
        progression ??= new PlayerProgression();
        progression.SetLevel(level);
    }

    private bool CanChangeSkill(Player player)
    {
        return IsInitialized && roster.IsUnlocked(player);
    }
}
