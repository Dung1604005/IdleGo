using UnityEngine;

[CreateAssetMenu(fileName = "SingleTargetAttack", menuName = "IdleGo/Attacks/Single Target")]
public class SingleTargetAttack : AttackType
{
    public override void Execute(CharacterCombat user, Character primaryTarget, float damageMultiplier)
    {
        if (user == null || primaryTarget == null)
        {
            return;
        }

        // AutoCombat đã chọn đối thủ gần nhất; đòn đơn chỉ gây sát thương lên mục tiêu đó.
        user.DealDamage(primaryTarget, damageMultiplier);
    }
}
