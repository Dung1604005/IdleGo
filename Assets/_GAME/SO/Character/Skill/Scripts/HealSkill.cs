using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealSkill", menuName = "IdleGo/Skills/Heal")]
public class HealSkill : CombatSkill
{
    [SerializeField] private List<int> healAmountByLevel ;

    public override float Range => float.PositiveInfinity;

    public override bool CanUse(CharacterCombat user, Character target)
    {
        return user != null && user.IsInitialized && user.Character != null && !user.Character.IsDead &&
            user.Character.CurrentHealth < user.Character.MaxHealth;
    }

    public override void Execute(CharacterCombat user, Character target, int level)
    {
        if (CanUse(user, target))
        {
            user.Character.Heal(Mathf.Max(1, healAmountByLevel[level]));
        }
    }
}
