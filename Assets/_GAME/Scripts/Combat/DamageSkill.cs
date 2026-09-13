using UnityEngine;

[CreateAssetMenu(fileName = "DamageSkill", menuName = "IdleGo/Skills/Damage")]
public class DamageSkill : CombatSkill
{
    [SerializeField, Min(0f)] private float damageMultiplier = 2f;

    public override bool CanUse(Character user, Character target)
    {
        return user != null && !user.IsDead && target != null && !target.IsDead;
    }

    public override void Execute(Character user, Character target)
    {
        if (!CanUse(user, target))
        {
            return;
        }

        user.DealDamage(target, damageMultiplier);
    }
}
