using UnityEngine;

[CreateAssetMenu(fileName = "DamageSkill", menuName = "IdleGo/Skills/Damage")]
public class DamageSkill : CombatSkill
{
    [SerializeField, Min(0f)] private float damageMultiplier = 2f;
    [SerializeField, Min(0f)] private float range = 1.5f;

    public override float Range => Mathf.Max(0f, range);

    public override bool CanUse(CharacterCombat user, Character target)
    {
        return user != null && user.IsInitialized && user.Character != null &&
            !user.Character.IsDead && target != null && !target.IsDead;
    }

    public override void Execute(CharacterCombat user, Character target)
    {
        if (!CanUse(user, target))
        {
            return;
        }

        user.DealDamage(target, damageMultiplier);
    }
}
