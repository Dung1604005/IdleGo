using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AoeAttack", menuName = "IdleGo/Attacks/AOE")]
public class AoeAttack : AttackType
{
    [SerializeField, Min(0f)] private float radius = 2f;
    [SerializeField] private LayerMask targetLayers = ~0;

    public float Radius => Mathf.Max(0f, radius);

    public override void Execute(CharacterCombat user, Character primaryTarget, float damageMultiplier)
    {
        if (user == null || user.Character == null || primaryTarget == null)
        {
            return;
        }
        // Tâm AOE nằm tại mục tiêu gần nhất để đòn đánh có thể trúng các đối thủ đứng quanh nó.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            primaryTarget.transform.position,
            Radius,
            targetLayers
        );

        if (user.Character is Player)
        {
            DamageTargets(user, EnemyManager.Ins.Enemies, colliders, damageMultiplier);
        }
        else if (user.Character is Enemy)
        {
            DamageTargets(user, PlayerManager.Ins.Players, colliders, damageMultiplier);
        }
    }

    private static void DamageTargets<T>(
        CharacterCombat user,
        IReadOnlyList<T> targets,
        Collider2D[] colliders,
        float damageMultiplier
    ) where T : Character
    {
        if (targets == null || colliders == null)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            T target = targets[i];
            if (target == null || !target.isActiveAndEnabled || target.IsDead)
            {
                continue;
            }
            if (ContainsCharacterCollider(target, colliders))
            {
                // Mỗi nhân vật chỉ nhận một lần damage dù có nhiều collider trong vùng AOE.
                user.DealDamage(target, damageMultiplier);
            }
        }
    }

    private static bool ContainsCharacterCollider(Character target, Collider2D[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D hit = colliders[i];
            if (hit == null)
            {
                continue;
            }

            Transform hitTransform = hit.transform;
            if (hitTransform == target.transform || hitTransform.IsChildOf(target.transform))
            {
                return true;
            }
        }

        return false;
    }
}
