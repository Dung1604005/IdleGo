using System.Collections.Generic;
using UnityEngine;

public partial class DataManager
{
    public string SelectedCharacterId => selectedCharacterId;

    public string GetTeamCharacterId(int teamIndex)
    {
        return playerData?.GetPlayer(teamIndex)?.CharacterId ?? string.Empty;
    }

    public string GetCharacterName(string characterId)
    {
        return GetProgressCharacter(characterId)?.CharacterName
            ?? string.Empty;
    }

    public Sprite GetCharacterPortrait(string characterId)
    {
        return GetProgressCharacter(characterId)?.Portrait;
    }

    public float GetCharacterStat(string characterId, StatType statType)
    {
        return GetProgressCharacter(characterId)?.Stats
            .GetCurrentStat(statType) ?? 0f;
    }

    public int GetCharacterExperienceRequired(string characterId)
    {
        Player player = GetProgressCharacter(characterId);
        return player != null
            ? player.Stats.GetExpToNextLevel(player.Stats.CurrentLevel)
            : 0;
    }

    public IReadOnlyList<CharacterStatUpgradeRule> GetCharacterStatRules(
        string characterId)
    {
        return GetCharacterStatProgressData(characterId)?.StatUpgradeRules;
    }

    public IReadOnlyList<CombatSkill> GetCharacterProgressSkills(
        string characterId)
    {
        return GetCharacterStatProgressData(characterId)?.CombatSkills;
    }

    public bool TryGetCharacterStatRule(
        string characterId,
        StatType statType,
        out CharacterStatUpgradeRule rule)
    {
        CharacterStatProgressSO data = GetCharacterStatProgressData(characterId);
        if (data != null)
        {
            return data.TryGetRule(statType, out rule);
        }

        rule = default;
        return false;
    }

    public bool TryGetCharacterSkillUpgrade(
        string characterId,
        CombatSkill skill,
        out int nextLevel,
        out int pointCost)
    {
        Player player = GetTeamProgressCharacter(characterId);
        nextLevel = 0;
        pointCost = 0;
        return player != null
            && player.StatProgress.TryGetSkillUpgrade(
                skill,
                out nextLevel,
                out pointCost);
    }

    public bool IsCharacterSkillEquipped(
        string characterId,
        CombatSkill skill)
    {
        Player player = GetProgressCharacter(characterId);
        if (player == null || skill == null)
        {
            return false;
        }

        IReadOnlyList<CombatSkillState> states =
            player.Combat.EquippedSkillStates;
        for (int i = 0; i < states.Count; i++)
        {
            if (ReferenceEquals(states[i]?.Skill, skill))
            {
                return true;
            }
        }

        return false;
    }

    public bool CanEquipCharacterSkill(
        string characterId,
        CombatSkill skill)
    {
        Player player = GetTeamProgressCharacter(characterId);
        return player != null
            && GetCharacterSkillLevel(characterId, skill) > 0
            && player.Combat.CanEquipSkill(skill);
    }
}
