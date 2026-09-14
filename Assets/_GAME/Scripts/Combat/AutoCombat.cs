using System.Collections.Generic;
using UnityEngine;

public class AutoCombat : MonoBehaviour
{
    private void Update()
    {
        if (!CharacterCombat.TryClaimFrame(Time.frameCount))
        {
            return;
        }

        IReadOnlyList<CharacterCombat> combats = CharacterCombat.ActiveCombats;
        for (int i = combats.Count - 1; i >= 0; i--)
        {
            Execute(combats[i], Time.timeAsDouble, Time.deltaTime);
        }
    }

    public static void Execute(CharacterCombat combat, double now, float deltaTime)
    {
        if (combat == null || !combat.IsInitialized)
        {
            return;
        }

        Character character = combat.Character;
        if (character == null || !character.IsInitialized)
        {
            combat.OnDespawn();
            return;
        }

        combat.EnsureMovementInitialized();
        combat.TickSkillStates(deltaTime, now);

        if (character.IsDead)
        {
            combat.Movement.Stop();
            combat.SetTarget(null);
            return;
        }

        Character target = GetNearestOpponent(character);
        combat.SetTarget(target);
        if (target == null)
        {
            combat.Movement.Stop();
            return;
        }

        CombatSkillState skillState = combat.SelectReadySkill(target);
        float range = skillState != null ? skillState.Skill.Range : combat.BasicAttackRange;
        if (!combat.Movement.MoveToward(target, range) || now < combat.NextActionAt)
        {
            return;
        }

        combat.Movement.Stop();
        if (skillState != null)
        {
            skillState.Skill.Execute(combat, target, skillState.Level);
            skillState.StartCooldown(character.Stats.CurrentCooldownReduction);
        }
        else
        {
            combat.Attack(target);
        }

        combat.MarkAction(now);
    }

    private static Character GetNearestOpponent(Character character)
    {
        if (character is Player)
        {
            return EnemyManager.Ins.GetNearestTarget(character.transform.position);
        }

        if (character is Enemy)
        {
            return PlayerManager.Ins.GetNearestTarget(character.transform.position);
        }

        return null;
    }
}
