using UnityEngine;

public partial class PlayerManager
{
    internal bool CanResetCharacterLevel(Player player, out string reason)
    {
        if (!Application.isPlaying || !IsInitialized
            || inventory == null || !inventory.IsInitialized
            || player == null || !ContainsCharacter(player)
            || !IsCharacterUnlocked(player) || !player.IsInitialized)
        {
            reason = "Select an initialized hero in Inventory/Hero during Play Mode.";
            return false;
        }

        if (player.Stats.CurrentLevel == 1 && player.Stats.CurrentExperience == 0)
        {
            reason = "This hero is already level 1 with 0 EXP.";
            return false;
        }

        // Save loading checks requirements again; reject an invalid future loadout
        // before lowering the level so equipped items cannot disappear on reload.
        return CanKeepLoadoutAtLevelOne(player, out reason);
    }

    internal bool ResetCharacterLevel(Player player, out string message)
    {
        if (!CanResetCharacterLevel(player, out message))
        {
            return false;
        }

        // Change only this hero's level/EXP, then let the owner save and refresh views.
        player.Stats.SetCurrentStat(StatType.LEVEL, 1f);
        player.Stats.SetCurrentStat(StatType.EXPERIENCE, 0f);
        CompleteProgressChange();
        message = inventory.LastSaveSucceeded
            ? "Hero reset to level 1 / 0 EXP. Saved and views refreshed."
            : "Hero reset and views refreshed, but saving failed. Check the Console.";
        return true;
    }

    private bool CanKeepLoadoutAtLevelOne(Player player, out string reason)
    {
        var equipments = player.Equipment.EquippedItems;
        for (int i = 0; i < equipments.Count; i++)
        {
            if (equipments[i]?.Data != null && equipments[i].Data.LevelRequired > 1)
            {
                reason = "Unequip equipment requiring level > 1 before resetting.";
                return false;
            }
        }

        var skills = player.Combat.EquippedSkillStates;
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i]?.Skill != null && skills[i].Skill.LevelRequire > 1)
            {
                reason = "Unequip skills requiring level > 1 before resetting.";
                return false;
            }
        }

        reason = string.Empty;
        return true;
    }
}
