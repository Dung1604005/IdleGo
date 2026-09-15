using System;
using UnityEngine;

public class AutoCombat : MonoBehaviour
{
    public void UpdateAutoCombat(CharacterCombat combat)
    {
        Execute(combat, Time.timeAsDouble, Time.deltaTime);
    }

    public static void Execute(CharacterCombat combat, double now, float deltaTime)
    {
        if (!TryGetInitializedCharacter(combat, out Character character))
        {
            return;
        }

        // Hồi chiêu vẫn tiếp tục chạy khi nhân vật đang chết.
        combat.TickSkillStates(deltaTime, now);
        if (combat.IsAttacking)
        {
            return;
        }

        if (character.IsDead)
        {
            character.Movement.Stop();
            combat.SetTarget(null);
            return;
        }

        if (!TryAcquireTarget(combat, character, out Character target))
        {
            return;
        }

        // CharacterCombat ưu tiên skill vừa hồi xong gần nhất.
        CombatSkillState skillState = combat.SelectReadySkill(target);
        if (!TryReachAttackRange(combat, target, skillState, now))
        {
            return;
        }

        PerformAction(combat, character, target, skillState, now);
    }

    private static bool TryGetInitializedCharacter(CharacterCombat combat, out Character character)
    {
        character = null;
        if (combat == null || !combat.IsInitialized)
        {
            return false;
        }

        character = combat.Character;
        if (character == null || !character.IsInitialized)
        {
            combat.OnDespawn();
            return false;
        }

        return true;
    }

    private static bool TryAcquireTarget(CharacterCombat combat, Character character, out Character target)
    {
        target = GetNearestOpponent(character);
        combat.SetTarget(target);
        if (target == null)
        {
            character.Movement.Stop();
            return false;
        }

        return true;
    }

    private static bool TryReachAttackRange(CharacterCombat combat, Character target, CombatSkillState skillState, double now)
    {
        float range = skillState != null ? skillState.Skill.Range : combat.BasicAttackRange;

        // Vẫn tiến về mục tiêu trong lúc chờ nhịp tấn công chung.
        return combat.Character.Movement.MoveToward(target, range) && now >= combat.NextActionAt;
    }

    private static void PerformAction(CharacterCombat combat, Character character, Character target, CombatSkillState skillState, double now)
    {
        // Dừng di chuyển trước khi dùng skill hoặc đánh thường.
        character.Movement.Stop();
        String animAttack = GameConfig.ANIM_BASIC_ATTACK;
        if (skillState != null)
        {
            skillState.Skill.Execute(combat, target, skillState.Level);
            animAttack = skillState.Skill.NameAnim;
        }
        else
        {
            combat.Attack(target);
        }
        // CharacterCombat giữ skill này cho đến khi Animation Event gọi EndAttack.
        combat.StartAttack(animAttack, skillState);

        // Nhịp tấn công chung tách biệt với thời gian hồi của từng skill.
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
