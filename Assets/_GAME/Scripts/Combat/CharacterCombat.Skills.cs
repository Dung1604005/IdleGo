using System;
using System.Collections.Generic;

public partial class CharacterCombat
{
    public CombatSkill GetEquippedSkill(int skillIndex)
    {
        CombatSkillState state = GetEquippedSkillState(skillIndex);
        return state != null ? state.Skill : null;
    }

    public float GetSkillCooldownProgress(int skillIndex)
    {
        CombatSkillState state = GetEquippedSkillState(skillIndex);
        return state != null ? state.CooldownProgress : 0f;
    }

    public bool CanEquipSkill(CombatSkill skill)
    {
        return IsInitialized
            && combatSkillStates.Count < MaxEquippedSkillCount
            && IsSkillAvailable(skill)
            && skill.CanEquip(character)
            && GetSkillState(skill) == null;
    }

    public bool EquipSkill(CombatSkill skill)
    {
        return CanEquipSkill(skill) && TryEquipSkillInternal(skill);
    }

    public bool UnequipSkill(CombatSkill skill)
    {
        CombatSkillState state = GetSkillState(skill);
        if (!IsInitialized || state == null || ReferenceEquals(state, activeSkillState))
        {
            return false;
        }

        state.OnDespawn();
        combatSkillStates.Remove(state);
        return true;
    }

    public void ClearEquippedSkills()
    {
        if (combatSkillStates == null || isAttacking)
        {
            return;
        }

        for (int i = 0; i < combatSkillStates.Count; i++)
        {
            combatSkillStates[i]?.OnDespawn();
        }

        combatSkillStates.Clear();
    }

    public CombatSkill GetAvailableSkill(string skillId)
    {
        if (combatData == null || string.IsNullOrWhiteSpace(skillId))
        {
            return null;
        }

        IReadOnlyList<CombatSkill> availableSkills = combatData.GetCombatSkills();
        for (int i = 0; i < availableSkills.Count; i++)
        {
            CombatSkill skill = availableSkills[i];
            if (skill != null && skill.SkillId == skillId)
            {
                return skill;
            }
        }

        return null;
    }

    public bool SetEquippedSkillLevel(CombatSkill skill, int level)
    {
        CombatSkillState state = GetSkillState(skill);
        if (state == null)
        {
            return false;
        }

        state.SetLevel(level);
        return true;
    }

    public void TickSkillStates(float deltaTime, double now)
    {
        for (int i = 0; i < combatSkillStates.Count; i++)
        {
            CombatSkillState state = combatSkillStates[i];
            state.RefreshUnlock(character.Stats.CurrentLevel, now);
            state.TickCooldown(deltaTime, now);
        }
    }

    public CombatSkillState SelectReadySkill(Character opponent)
    {
        CombatSkillState selected = null;
        for (int i = 0; i < combatSkillStates.Count; i++)
        {
            CombatSkillState state = combatSkillStates[i];
            if (!state.IsReady || !state.Skill.CanUse(this, opponent))
            {
                continue;
            }

            if (selected == null || state.LastReadyAt > selected.LastReadyAt)
            {
                selected = state;
            }
        }

        return selected;
    }

    private bool TryEquipSkillInternal(CombatSkill skill)
    {
        if (skill == null
            || combatSkillStates.Count >= MaxEquippedSkillCount
            || !skill.CanEquip(character)
            || GetSkillState(skill) != null)
        {
            return false;
        }

        // Chi skill dat dieu kien moi duoc tao state chien dau.
        CombatSkillState state = new CombatSkillState();
        state.OnInit(skill, character.Stats.CurrentLevel);
        combatSkillStates.Add(state);
        return true;
    }

    private bool IsSkillAvailable(CombatSkill skill)
    {
        if (skill == null || character == null || combatData == null)
        {
            return false;
        }

        IReadOnlyList<CombatSkill> availableSkills = combatData.GetCombatSkills();
        for (int i = 0; i < availableSkills.Count; i++)
        {
            if (ReferenceEquals(availableSkills[i], skill))
            {
                return true;
            }
        }

        return false;
    }

    private CombatSkillState GetSkillState(CombatSkill skill)
    {
        for (int i = 0; i < combatSkillStates.Count; i++)
        {
            CombatSkillState state = combatSkillStates[i];
            if (state != null && ReferenceEquals(state.Skill, skill))
            {
                return state;
            }
        }

        return null;
    }

    private CombatSkillState GetEquippedSkillState(int skillIndex)
    {
        if (combatSkillStates == null
            || skillIndex < 0
            || skillIndex >= combatSkillStates.Count)
        {
            return null;
        }

        return combatSkillStates[skillIndex];
    }
}
