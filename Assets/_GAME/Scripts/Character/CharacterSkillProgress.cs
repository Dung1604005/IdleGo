using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CharacterSkillProgress
{
    [SerializeField] private List<CharacterSkillLevel> skillLevels =
        new List<CharacterSkillLevel>();

    [NonSerialized] private CharacterStatProgressSO progressData;

    public void OnInit(CharacterStatProgressSO data)
    {
        progressData = data;
        skillLevels ??= new List<CharacterSkillLevel>();
        RemoveUnavailableSkills();
        AddMissingSkills();
    }

    public int GetLevel(CombatSkill skill)
    {
        CharacterSkillLevel state = GetState(skill);
        return state != null ? state.Level : 0;
    }

    public bool TryGetUpgrade(
        CombatSkill skill,
        Player owner,
        out int nextLevel,
        out int statPointCost)
    {
        nextLevel = GetLevel(skill) + 1;
        statPointCost = 0;
        if (progressData == null
            || !progressData.ContainsSkill(skill)
            || !skill.MeetsCharacterRequirements(owner)
            || nextLevel > skill.MaxLevel)
        {
            return false;
        }

        statPointCost = skill.GetStatPointCost(nextLevel);
        return statPointCost > 0;
    }

    public bool SetLevel(CombatSkill skill, int level)
    {
        CharacterSkillLevel state = GetState(skill);
        if (state == null || skill == null)
        {
            return false;
        }

        int normalizedLevel = Mathf.Clamp(level, 0, skill.MaxLevel);
        if (state.Level == normalizedLevel)
        {
            return false;
        }

        state.SetLevel(normalizedLevel);
        return true;
    }

    public void CopyTo(List<CharacterSkillProgressSaveData> output)
    {
        output.Clear();
        for (int i = 0; i < skillLevels.Count; i++)
        {
            CharacterSkillLevel state = skillLevels[i];
            if (state != null && !string.IsNullOrWhiteSpace(state.SkillId))
            {
                output.Add(new CharacterSkillProgressSaveData
                {
                    skillId = state.SkillId,
                    level = state.Level
                });
            }
        }
    }

    public void Restore(IReadOnlyList<CharacterSkillProgressSaveData> savedSkills)
    {
        for (int i = 0; i < skillLevels.Count; i++)
        {
            skillLevels[i]?.SetLevel(0);
        }

        for (int i = 0; savedSkills != null && i < savedSkills.Count; i++)
        {
            CharacterSkillProgressSaveData saved = savedSkills[i];
            CombatSkill skill = GetSkill(saved?.skillId);
            if (skill != null)
            {
                SetLevel(skill, saved.level);
            }
        }
    }

    private CharacterSkillLevel GetState(CombatSkill skill)
    {
        string skillId = skill != null ? skill.SkillId : string.Empty;
        for (int i = 0; i < skillLevels.Count; i++)
        {
            if (skillLevels[i]?.SkillId == skillId)
            {
                return skillLevels[i];
            }
        }
        return null;
    }

    private CombatSkill GetSkill(string skillId)
    {
        IReadOnlyList<CombatSkill> skills = progressData?.CombatSkills;
        for (int i = 0; skills != null && i < skills.Count; i++)
        {
            if (skills[i] != null && skills[i].SkillId == skillId)
            {
                return skills[i];
            }
        }
        return null;
    }

    private void RemoveUnavailableSkills()
    {
        for (int i = skillLevels.Count - 1; i >= 0; i--)
        {
            if (skillLevels[i] == null || GetSkill(skillLevels[i].SkillId) == null)
            {
                skillLevels.RemoveAt(i);
            }
        }
    }

    private void AddMissingSkills()
    {
        IReadOnlyList<CombatSkill> skills = progressData?.CombatSkills;
        for (int i = 0; skills != null && i < skills.Count; i++)
        {
            CombatSkill skill = skills[i];
            if (skill != null && GetState(skill) == null)
            {
                skillLevels.Add(new CharacterSkillLevel(skill.SkillId));
            }
        }
    }
}

[Serializable]
public sealed class CharacterSkillLevel
{
    [SerializeField] private string skillId;
    [SerializeField, Min(0)] private int level;

    public CharacterSkillLevel()
    {
    }

    public CharacterSkillLevel(string id)
    {
        skillId = id;
    }

    public string SkillId => skillId;
    public int Level => Mathf.Max(0, level);

    public void SetLevel(int value)
    {
        level = Mathf.Max(0, value);
    }
}
