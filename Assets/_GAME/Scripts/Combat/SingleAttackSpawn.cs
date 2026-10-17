using UnityEngine;

[CreateAssetMenu(fileName = "SingleAttackSpawn", menuName = "IdleGo/Attacks/Single Attack Spawn")]
public sealed class SingleAttackSpawn : SpawnAttackType
{
    public override void Execute(CharacterCombat user, Character primaryTarget, float damageMultiplier)
    {
        SpawnProjectile(user, primaryTarget, damageMultiplier, false, 0f);
    }
}
