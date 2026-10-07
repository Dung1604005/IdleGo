using System.Collections.Generic;
using UnityEngine;

public static class PlayerRosterSaveMapper
{
    public static bool TryCreate(
        PlayerManager playerManager,
        out PlayerRosterSaveData saveData)
    {
        saveData = new PlayerRosterSaveData();
        if (playerManager == null)
        {
            return false;
        }

        HashSet<string> savedCharacterIds = new HashSet<string>();
        IReadOnlyList<Player> characters = playerManager.AllPlayers;
        for (int i = 0; i < characters.Count; i++)
        {
            if (!TryCreateCharacterSave(
                characters[i],
                playerManager,
                savedCharacterIds,
                out PlayerCharacterSaveData characterSave,
                out bool isUnlocked))
            {
                saveData = null;
                return false;
            }

            saveData.characters.Add(characterSave);
            if (isUnlocked)
            {
                saveData.unlockedCharacterIds.Add(characterSave.characterId);
            }
        }

        AddTeamIds(playerManager.Players, saveData.teamCharacterIds);
        return true;
    }

    public static bool ApplyProgress(PlayerRosterSaveData saveData, PlayerManager playerManager)
    {
        if (!HasData(saveData) || playerManager == null)
        {
            return false;
        }

        playerManager.ResetRosterForLoad();
        RestoreCharacterProgress(saveData.characters, playerManager);
        RestoreUnlockedCharacters(saveData.unlockedCharacterIds, playerManager);
        RestoreTeam(saveData.teamCharacterIds, playerManager);
        EnsureTeamHasACharacter(playerManager);
        return true;
    }

    public static bool HasData(PlayerRosterSaveData saveData)
    {
        return saveData != null
            && saveData.characters != null
            && saveData.characters.Count > 0;
    }

    private static bool TryCreateCharacterSave(
        Player player,
        PlayerManager playerManager,
        HashSet<string> savedIds,
        out PlayerCharacterSaveData saveData,
        out bool isUnlocked)
    {
        saveData = null;
        isUnlocked = false;
        if (player == null || string.IsNullOrWhiteSpace(player.CharacterId))
        {
            Debug.LogError("Every saved Player needs a stable CharacterId.");
            return false;
        }

        if (!savedIds.Add(player.CharacterId))
        {
            Debug.LogError($"Duplicate CharacterId '{player.CharacterId}' cannot be saved.");
            return false;
        }

        if (!TryCreateCharacterSaveData(player, out saveData))
        {
            return false;
        }

        isUnlocked = playerManager.IsCharacterUnlocked(player);
        return true;
    }

    private static bool TryCreateCharacterSaveData(
        Player player,
        out PlayerCharacterSaveData saveData)
    {
        saveData = new PlayerCharacterSaveData
        {
            characterId = player.CharacterId
        };
        player.Stats.CopyProgressTo(saveData.currentStats);
        player.StatProgress.CopyTo(saveData.statProgress);
        IReadOnlyList<Equipment> equipments = player.Equipment.EquippedItems;
        for (int i = 0; i < equipments.Count; i++)
        {
            if (equipments[i] == null)
            {
                continue;
            }

            if (!EquipmentInstanceSaveMapper.TryCreate(
                equipments[i],
                out EquippedEquipmentSaveData equipmentSave))
            {
                return false;
            }

            saveData.equippedEquipments.Add(equipmentSave);
        }

        PlayerSkillSaveMapper.AddLoadout(player, saveData.equippedSkills);
        return true;
    }

    private static void AddTeamIds(
        IReadOnlyList<Player> team,
        List<string> output)
    {
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i] != null)
            {
                output.Add(team[i].CharacterId);
            }
        }
    }

    private static void RestoreCharacterProgress(
        IReadOnlyList<PlayerCharacterSaveData> savedCharacters,
        PlayerManager playerManager)
    {
        for (int i = 0; i < savedCharacters.Count; i++)
        {
            PlayerCharacterSaveData savedCharacter = savedCharacters[i];
            Player player = savedCharacter != null
                ? playerManager.GetCharacter(savedCharacter.characterId)
                : null;
            if (player == null)
            {
                continue;
            }

            // Stat phai co truoc skill vi dieu kien trang bi skill phu thuoc level.
            player.Stats.RestoreProgress(savedCharacter.currentStats);
            player.StatProgress.Restore(
                savedCharacter.statProgress,
                player.Stats.CurrentLevel);
            PlayerSkillSaveMapper.RestoreLoadout(player, savedCharacter.equippedSkills);
        }
    }

    private static void RestoreUnlockedCharacters(
        IReadOnlyList<string> characterIds,
        PlayerManager playerManager)
    {
        if (characterIds == null)
        {
            return;
        }

        for (int i = 0; i < characterIds.Count; i++)
        {
            Player player = playerManager.GetCharacter(characterIds[i]);
            if (player != null)
            {
                playerManager.UnlockCharacterForLoad(player);
            }
        }
    }

    private static void RestoreTeam(
        IReadOnlyList<string> characterIds,
        PlayerManager playerManager)
    {
        if (characterIds == null)
        {
            return;
        }

        for (int i = 0; i < characterIds.Count; i++)
        {
            Player player = playerManager.GetCharacter(characterIds[i]);
            if (player != null)
            {
                playerManager.AddToTeamForLoad(player);
            }
        }
    }

    private static void EnsureTeamHasACharacter(PlayerManager playerManager)
    {
        if (playerManager.TeamCount > 0)
        {
            return;
        }

        IReadOnlyList<Player> characters = playerManager.AllPlayers;
        for (int i = 0; i < characters.Count; i++)
        {
            Player player = characters[i];
            if (playerManager.IsCharacterUnlocked(player))
            {
                playerManager.AddToTeamForLoad(player);
                return;
            }
        }
    }
}
