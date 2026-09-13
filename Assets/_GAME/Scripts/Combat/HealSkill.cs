using UnityEngine;

[CreateAssetMenu(fileName = "HealSkill", menuName = "IdleGo/Skills/Heal")]
public class HealSkill : CombatSkill
{
    [SerializeField, Min(1)] private int healAmount = 20;

    public override bool CanUse(Character user, Character target)
    {
        return user != null && !user.IsDead && user.CurrentHealth < user.MaxHealth;
    }

    public override void Execute(Character user, Character target)
    {
        if (CanUse(user, target))
        {
            user.Heal(Mathf.Max(1, healAmount));
        }
    }
}
