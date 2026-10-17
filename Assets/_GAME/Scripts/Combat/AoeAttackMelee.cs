using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp",
    sourceClassName: "AoeAttack")]
[CreateAssetMenu(fileName = "AoeAttackMelee", menuName = "IdleGo/Attacks/AOE Attack Melee")]
public sealed class AoeAttackMelee : AttackType
{
    [SerializeField, Min(0f)] private float radius = 2f;
    [SerializeField] private LayerMask targetLayers = ~0;

    public float Radius => Mathf.Max(0f, radius);

    public override void Execute(CharacterCombat user, Character primaryTarget, float damageMultiplier)
    {
        if (!AttackTargetUtility.CanAttack(user, primaryTarget)) return;
        // Preserve the melee AOE center around the nearest selected opponent.
        AttackTargetUtility.DamageInCircle(user, primaryTarget.TF.position,
            Radius, targetLayers, damageMultiplier);
    }
}
