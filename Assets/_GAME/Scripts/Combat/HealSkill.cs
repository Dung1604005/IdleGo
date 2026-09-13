using UnityEngine;

[CreateAssetMenu(fileName = "HealSkill", menuName = "IdleGo/Skills/Heal")]
public class HealSkill : CombatSkill
{
    [SerializeField, Min(1)] private int healAmount = 20;

    public override float Range => float.PositiveInfinity;

    public override bool CanUse(CharacterCombat user, Character target)
    {
        return user != null && user.IsInitialized && user.Character != null && !user.Character.IsDead &&
            user.Character.CurrentHealth < user.Character.MaxHealth;
    }

    public override void Execute(CharacterCombat user, Character target)
    {
        if (CanUse(user, target))
        {
            user.Character.Heal(Mathf.Max(1, healAmount));
        }
    }
}
