using System;
using System.Collections.Generic;
using UnityEngine;

public partial class DataManager
{
    public int GetGlobalLevel()
    {
        return Mathf.RoundToInt(GetGlobalStat(GlobalStatType.LEVEL));
    }

    public float GetGlobalStat(GlobalStatType statType)
    {
        return playerData?.Progression.GlobalStats.GetStat(statType) ?? 0f;
    }

    public float GetGlobalStatMultiplier(GlobalStatType buffType)
    {
        return playerData?.Progression.GlobalStats.GetMultiplier(buffType) ?? 1f;
    }

    public int CalculateGoldReward(int baseGold)
    {
        return playerData?.Progression.GlobalStats.ApplyGoldBuff(baseGold)
            ?? UnityEngine.Mathf.Max(0, baseGold);
    }

    public IReadOnlyList<Player> GetTeamPlayers()
    {
        return playerData?.Players ?? Array.Empty<Player>();
    }

    public IReadOnlyList<Enemy> GetEnemies()
    {
        return EnemyManager.Ins != null
            ? EnemyManager.Ins.Enemies
            : Array.Empty<Enemy>();
    }

    public Player GetNearestTeamPlayer(Vector3 position)
    {
        return playerData?.GetNearestTarget(position);
    }

    public Enemy GetNearestEnemy(Vector3 position)
    {
        return EnemyManager.Ins != null
            ? EnemyManager.Ins.GetNearestTarget(position)
            : null;
    }

    public bool SetGlobalStat(GlobalStatType statType, float value)
    {
        GlobalStat globalStats = playerData?.Progression.GlobalStats;
        if (globalStats == null || statType == GlobalStatType.EXPERIENCE)
        {
            return false;
        }

        float previousValue = globalStats.GetStat(statType);
        globalStats.SetStat(statType, value);
        return CompleteGlobalProgressChange(
            !Mathf.Approximately(previousValue, globalStats.GetStat(statType)));
    }

    public bool AddGlobalStat(GlobalStatType statType, float amount)
    {
        GlobalStat globalStats = playerData?.Progression.GlobalStats;
        if (globalStats == null
            || !GlobalStatTypeUtility.IsValid(statType)
            || Mathf.Approximately(amount, 0f))
        {
            return false;
        }

        float previousValue = globalStats.GetStat(statType);
        int previousLevel = globalStats.CurrentLevel;
        if (statType == GlobalStatType.EXPERIENCE)
        {
            int experience = Mathf.RoundToInt(amount);
            if (experience <= 0)
            {
                return false;
            }
            globalStats.AddExperience(experience);
        }
        else
        {
            globalStats.AddStat(statType, amount);
        }

        bool changed = previousLevel != globalStats.CurrentLevel
            || !Mathf.Approximately(
                previousValue,
                globalStats.GetStat(statType));
        return CompleteGlobalProgressChange(changed);
    }

    public CharacterStatProgressSO GetCharacterStatProgressData(string characterId)
    {
        return GetProgressCharacter(characterId)?.StatProgress.ProgressData;
    }

    public int GetCharacterAvailableStatPoints(string characterId)
    {
        return GetProgressCharacter(characterId)?.StatProgress.AvailableStatPoints ?? 0;
    }

    public int GetCharacterStatUpgradeCount(
        string characterId,
        StatType statType)
    {
        return GetProgressCharacter(characterId)?.StatProgress
            .GetUpgradeCount(statType) ?? 0;
    }

    public int GetCharacterSkillLevel(
        string characterId,
        CombatSkill skill)
    {
        return GetProgressCharacter(characterId)?.StatProgress
            .GetSkillLevel(skill) ?? 0;
    }

    public bool UpgradeCharacterStat(string characterId, StatType statType)
    {
        Player player = GetTeamProgressCharacter(characterId);
        return CompleteCharacterProgressChange(
            player,
            player != null && player.StatProgress.TryUpgradeStat(statType));
    }

    public bool DowngradeCharacterStat(string characterId, StatType statType)
    {
        Player player = GetTeamProgressCharacter(characterId);
        return CompleteCharacterProgressChange(
            player,
            player != null && player.StatProgress.TryDowngradeStat(statType));
    }

    public bool ResetCharacterStatPoints(string characterId)
    {
        Player player = GetTeamProgressCharacter(characterId);
        return CompleteCharacterProgressChange(
            player,
            player != null && player.StatProgress.ResetStatPoints());
    }

    public bool UpgradeCharacterSkill(string characterId, CombatSkill skill)
    {
        Player player = GetTeamProgressCharacter(characterId);
        return CompleteCharacterProgressChange(
            player,
            player != null && player.StatProgress.TryUpgradeSkill(skill));
    }

    public bool EquipCharacterSkill(string characterId, CombatSkill skill)
    {
        Player player = GetTeamProgressCharacter(characterId);
        return player != null
            && player.StatProgress.GetSkillLevel(skill) > 0
            && playerData.EquipSkill(player, skill);
    }

    public bool UnequipCharacterSkill(string characterId, CombatSkill skill)
    {
        Player player = GetTeamProgressCharacter(characterId);
        return player != null && playerData.UnequipSkill(player, skill);
    }

    public bool RegisterEnemyKill(Player killer, Enemy defeatedEnemy)
    {
        if (killer == null
            || defeatedEnemy == null
            || playerData == null
            || !playerData.ContainsPlayer(killer))
        {
            return false;
        }

        EnemyDataSO enemyData = EnemyManager.Ins.GetEnemyData(defeatedEnemy);
        if (enemyData == null || enemyData.Experience <= 0)
        {
            return false;
        }

        int awardedExperience = playerData.Progression.AddEnemyExperience(
            enemyData.Experience);
        int previousLevel = killer.Stats.CurrentLevel;
        killer.Stats.AddExperience(awardedExperience);
        killer.Stats.CheckLevelUp();
        int gainedLevels = killer.Stats.CurrentLevel - previousLevel;
        killer.StatProgress.AddLevelStatPoints(gainedLevels);
        playerData.CompleteProgressChange();
        return true;
    }

    private Player GetProgressCharacter(string characterId)
    {
        return playerData?.GetCharacter(characterId);
    }

    private Player GetTeamProgressCharacter(string characterId)
    {
        Player player = GetProgressCharacter(characterId);
        return playerData != null && playerData.ContainsPlayer(player)
            ? player
            : null;
    }

    private bool CompleteCharacterProgressChange(
        Player player,
        bool changed)
    {
        if (player == null || !changed || playerData == null)
        {
            return false;
        }

        playerData.CompleteProgressChange();
        return true;
    }

    private bool CompleteGlobalProgressChange(bool changed)
    {
        if (!changed || playerData == null)
        {
            return false;
        }

        playerData.CompleteProgressChange();
        return true;
    }
}
