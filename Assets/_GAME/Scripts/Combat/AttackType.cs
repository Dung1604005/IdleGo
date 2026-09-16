using UnityEngine;

public abstract class AttackType : ScriptableObject
{
    public abstract void Execute(CharacterCombat user, Character primaryTarget, float damageMultiplier);
}
