using System.Collections.Generic;
using UnityEngine;

public static class AttackTargetUtility
{
    public static bool IsAlive(Character character)
    {
        return character != null && character.IsInitialized
            && character.isActiveAndEnabled && !character.IsDead;
    }

    public static bool CanAttack(CharacterCombat combat, Character target)
    {
        if (combat == null || !combat.IsInitialized
            || !IsAlive(combat.Character) || !IsAlive(target)) return false;
        Character owner = combat.Character;
        // Match factions explicitly: never damage the owner or another ally.
        return owner != target
            && ((owner is Player && target is Enemy)
                || (owner is Enemy && target is Player));
    }

    public static bool ContainsCollider(Character character, Collider2D collider)
    {
        if (character == null || collider == null) return false;
        Transform hit = collider.transform;
        return hit == character.TF || hit.IsChildOf(character.TF);
    }

    public static void DamageInCircle(CharacterCombat combat, Vector3 center,
        float radius, LayerMask layers, float multiplier)
    {
        if (combat == null || !IsAlive(combat.Character)) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, layers);
        if (combat.Character is Player)
        {
            DamageTargets(combat, DataManager.Ins.GetEnemies(), hits, multiplier);
        }
        else if (combat.Character is Enemy)
        {
            DamageTargets(combat, DataManager.Ins.GetTeamPlayers(), hits, multiplier);
        }
    }

    private static void DamageTargets<T>(CharacterCombat combat,
        IReadOnlyList<T> targets, Collider2D[] hits, float multiplier) where T : Character
    {
        if (targets == null) return;
        for (int i = 0; i < targets.Count; i++)
        {
            T target = targets[i];
            if (!CanAttack(combat, target)) continue;
            for (int j = 0; j < hits.Length; j++)
            {
                if (!ContainsCollider(target, hits[j])) continue;
                // Iterate characters, not hits: multiple colliders still count as one hit.
                combat.DealDamage(target, multiplier);
                break;
            }
        }
    }
}
