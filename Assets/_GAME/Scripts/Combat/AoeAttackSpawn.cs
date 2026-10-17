using UnityEngine;

[CreateAssetMenu(fileName = "AoeAttackSpawn", menuName = "IdleGo/Attacks/AOE Attack Spawn")]
public sealed class AoeAttackSpawn : SpawnAttackType
{
    [SerializeField, Min(0f)] private float radius = 2f;

    public float Radius => Mathf.Max(0f, radius);

    public override void Execute(CharacterCombat user, Character primaryTarget, float damageMultiplier)
    {
        SpawnProjectile(user, primaryTarget, damageMultiplier, true, Radius);
    }
}
