using UnityEngine;

public partial class CharacterCombat
{
    public void SetIsAttacking(bool value)
    {
        isAttacking = value;
    }

    public void MarkAction(double now)
    {
        float attackSpeed = character.Stats.GetCurrentStat(StatType.ATTACK_SPEED);
        nextActionAt = now + delayAttack / Mathf.Max(0.01f, attackSpeed);
    }

    public void StartAttack(
        string animAttack,
        Character attackTarget,
        CombatSkillState skillState)
    {
        // Damage chi duoc thuc thi khi Animation Event goi ExecuteAttack.
        activeAttackTarget = attackTarget;
        activeSkillState = skillState;
        hasExecutedAttack = false;
        SetIsAttacking(true);
        character.ChangeAnim(animAttack);
    }

    public void ExecuteAttack()
    {
        if (!isAttacking || hasExecutedAttack)
        {
            return;
        }

        // Danh dau truoc de mot animation khong the gay damage hai lan.
        hasExecutedAttack = true;
        if (activeSkillState != null)
        {
            activeSkillState.Skill.Execute(this, activeAttackTarget, activeSkillState.Level);
            return;
        }

        Attack(activeAttackTarget);
    }

    public void EndAttack()
    {
        character.ChangeAnim(GameConfig.ANIM_IDLE);
        if (!isAttacking)
        {
            return;
        }

        // Cooldown bat dau khi Animation Event xac nhan don danh da ket thuc.
        if (activeSkillState != null)
        {
            float cooldownReduction = character.Stats.GetCurrentStat(StatType.COOLDOWN_REDUCTION);
            activeSkillState.StartCooldown(cooldownReduction);
        }

        activeAttackTarget = null;
        activeSkillState = null;
        hasExecutedAttack = false;
        SetIsAttacking(false);
    }

    public virtual void Attack(Character attackTarget)
    {
        if (basicAttackType != null)
        {
            basicAttackType.Execute(this, attackTarget, 1f);
            return;
        }

        // Giu don danh don mac dinh neu CombatData chua co AttackType.
        DealDamage(attackTarget, 1f);
    }

    public virtual void DealDamage(Character attackTarget, float multiplier)
    {
        if (!CanDealDamage(attackTarget))
        {
            return;
        }

        CharacterStat stats = character.Stats;
        float criticalChance = stats.GetCurrentStat(StatType.CRITICAL_CHANCE);
        bool isCritical = criticalChance >= 1f || Random.value < criticalChance;
        float criticalMultiplier = isCritical
            ? stats.GetCurrentStat(StatType.CRITICAL_DAMAGE)
            : 1f;
        float amplification = 1f + stats.GetCurrentStat(StatType.DAMAGE_AMPLIFICATION);
        int damage = CalculateDamage(multiplier, criticalMultiplier, amplification);
        int healthBeforeHit = attackTarget.CurrentHealth;
        attackTarget.TakeDamage(damage);
        ApplyDamageResult(attackTarget, healthBeforeHit, damage, isCritical, stats);
    }

    private bool CanDealDamage(Character attackTarget)
    {
        return IsInitialized
            && character != null
            && !character.IsDead
            && attackTarget != null
            && !attackTarget.IsDead;
    }

    private int CalculateDamage(
        float multiplier,
        float criticalMultiplier,
        float amplification)
    {
        float damage = character.AttackDamage
            * Mathf.Max(0f, multiplier)
            * amplification
            * criticalMultiplier;
        return Mathf.Max(0, Mathf.RoundToInt(damage));
    }

    private void ApplyDamageResult(
        Character attackTarget,
        int healthBeforeHit,
        int damage,
        bool isCritical,
        CharacterStat stats)
    {
        int healthLost = Mathf.Max(0, healthBeforeHit - attackTarget.CurrentHealth);
        if (healthLost > 0 && attackTarget is Enemy)
        {
            UIManager.Ins.GetUI<CanvasCombat>()
                .ShowDamage(damage, isCritical, attackTarget.transform);
        }

        float lifeSteal = stats.GetCurrentStat(StatType.LIFE_STEAL);
        if (healthLost > 0 && lifeSteal > 0f)
        {
            character.Heal(Mathf.RoundToInt(healthLost * lifeSteal));
        }
    }
}
