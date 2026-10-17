using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp",
    sourceClassName: "SingleTargetAttack")]
[CreateAssetMenu(fileName = "SingleAttackMelee", menuName = "IdleGo/Attacks/Single Attack Melee")]
public sealed class SingleAttackMelee : AttackType
{
    public override void Execute(CharacterCombat user, Character primaryTarget, float damageMultiplier)
    {
        if (!AttackTargetUtility.CanAttack(user, primaryTarget))
        {
            return;
        }

        // AutoCombat đã chọn đối thủ gần nhất; đòn đơn chỉ gây sát thương lên mục tiêu đó.
        user.DealDamage(primaryTarget, damageMultiplier);
    }
}
