using System.Collections.Generic;

public static class PlayerSkillSaveMapper
{
    public static void AddLoadout(Player player, List<EquippedSkillSaveData> output)
    {
        IReadOnlyList<CombatSkillState> states = player.Combat.EquippedSkillStates;
        for (int i = 0; i < states.Count; i++)
        {
            CombatSkillState state = states[i];
            if (state?.Skill == null)
            {
                continue;
            }

            output.Add(new EquippedSkillSaveData
            {
                skillId = state.Skill.SkillId,
                level = state.Level
            });
        }
    }

    public static void RestoreLoadout(
        Player player,
        IReadOnlyList<EquippedSkillSaveData> savedSkills)
    {
        player.Combat.ClearEquippedSkills();
        if (savedSkills == null)
        {
            return;
        }

        for (int i = 0; i < savedSkills.Count; i++)
        {
            RestoreSkill(player, savedSkills[i]);
        }
    }

    private static void RestoreSkill(Player player, EquippedSkillSaveData savedSkill)
    {
        CombatSkill skill = savedSkill != null
            ? player.Combat.GetAvailableSkill(savedSkill.skillId)
            : null;
        if (skill == null || !player.Combat.EquipSkill(skill))
        {
            return;
        }

        player.Combat.SetEquippedSkillLevel(skill, savedSkill.level);
    }
}
