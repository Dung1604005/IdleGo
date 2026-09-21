using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable]
public class CharacterCombat 
{
    [SerializeField] private Character character ;
    [SerializeField, Min(0f)] private float basicAttackRange;

    [SerializeField] private List<CombatSkillState> combatSkillStates = new List<CombatSkillState>();

    [SerializeField] private float delayAttack;

    private AttackType basicAttackType;
    private Character target;
    private Character activeAttackTarget;
    private CombatSkillState activeSkillState;

    private bool isAttacking = false;
    private bool hasExecutedAttack;
    private double nextActionAt;

    public Character Character => character;

    public bool IsAttacking => isAttacking;
    public float BasicAttackRange => Mathf.Max(0f, basicAttackRange);
    public bool IsInitialized { get; private set; }
    public double NextActionAt => nextActionAt;

    public void OnInit(CharacterCombatSO combatSO)
    {
        OnDespawn();
        isAttacking = false;
        basicAttackRange = combatSO.BaseRangeAttack;
        basicAttackType = combatSO.BasicAttackType;
        if (combatSkillStates == null)
        {
            combatSkillStates = new List<CombatSkillState>();
        }
        delayAttack = combatSO.DelayAttack;
        combatSkillStates.Clear();
        List<CombatSkill> skills = combatSO.GetCombatSkills();
        if (skills != null)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                CombatSkillState state = new CombatSkillState();
                state.OnInit(skills[i], character.Stats.CurrentLevel);
                combatSkillStates.Add(state);
            }
        }
        nextActionAt = 0d;
        target = null;
        activeAttackTarget = null;
        activeSkillState = null;
        hasExecutedAttack = false;
        IsInitialized = true;
    }

    public void OnDespawn()
    {
        SetTarget(null);

        if (combatSkillStates != null)
        {
            for (int i = 0; i < combatSkillStates.Count; i++)
            {
                combatSkillStates[i]?.OnDespawn();
            }
        }
        basicAttackType = null;
        activeAttackTarget = null;
        activeSkillState = null;
        isAttacking = false;
        hasExecutedAttack = false;
        nextActionAt = 0d;
        IsInitialized = false;
    }

    public void TickSkillStates(float deltaTime, double now)
    {
        for (int i = 0; i < combatSkillStates.Count; i++)
        {
            CombatSkillState state = combatSkillStates[i];
            state.RefreshUnlock(character.Stats.CurrentLevel, now);
            state.TickCooldown(deltaTime, now);
        }
    }

    public CombatSkillState SelectReadySkill(Character opponent)
    {
        CombatSkillState selected = null;

        for (int i = 0; i < combatSkillStates.Count; i++)
        {
            CombatSkillState state = combatSkillStates[i];
            if (!state.IsReady || !state.Skill.CanUse(this, opponent))
            {
                continue;
            }

            if (selected == null || state.LastReadyAt > selected.LastReadyAt)
            {
                selected = state;
            }
        }

        return selected;
    }

    public void SetTarget(Character opponent)
    {
        if (target == opponent)
        {
            return;
        }

        // Chỉ target do Player chọn mới được đưa lên trên; target của Enemy không đổi sorting của Player.
        SetPlayerTargetSorting(target, false);
        target = opponent;
        SetPlayerTargetSorting(target, true);
    }

    private void SetPlayerTargetSorting(Character opponent, bool isTarget)
    {
        if (character is Player && opponent is Enemy enemy)
        {
            enemy.SetAsPlayerTarget(isTarget);
        }
    }

    public void SetIsAttacking(bool val)
    {
        isAttacking = val;
    }

    public void MarkAction(double now)
    {
        nextActionAt = now + delayAttack / Mathf.Max(0.01f, character.Stats.CurrentAttackSpeed);
    }

    public void StartAttack(String animAttack, Character attackTarget, CombatSkillState skillState)
    {
        // Chỉ lưu dữ liệu đòn đánh tại đây; damage sẽ được thực thi bởi Animation Event.
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

        // Đánh dấu trước khi thực thi để một animation không thể gây damage hai lần do event bị gắn trùng.
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
        if (!isAttacking)
        {
            return;
        }

        // Animation Event là thời điểm xác nhận đòn đánh đã hoàn tất và bắt đầu hồi skill.
        if (activeSkillState != null)
        {
            activeSkillState.StartCooldown(character.Stats.CurrentCooldownReduction);
        }

        activeAttackTarget = null;
        activeSkillState = null;
        hasExecutedAttack = false;
        SetIsAttacking(false);
        character.ChangeAnim(GameConfig.ANIM_IDLE);
    }

    public virtual void Attack(Character target)
    {
        if (basicAttackType != null)
        {
            basicAttackType.Execute(this, target, 1f);
            return;
        }

        // Giữ hành vi đánh đơn cũ khi CombatData chưa được gán AttackType.
        DealDamage(target, 1f);
    }

    public virtual void DealDamage(Character target, float multiplier)
    {
        if (!IsInitialized || character == null || character.IsDead || target == null || target.IsDead)
        {
            return;
        }

        CharacterStat stats = character.Stats;
        float criticalChance = stats.CurrentCriticalChance;
        bool isCritical = criticalChance >= 1f || UnityEngine.Random.value < criticalChance;
        float criticalMultiplier = isCritical ? stats.CurrentCriticalDamage : 1f;
        float amplification = 1f + stats.CurrentDamageAmplification;
        int damage = Mathf.Max(0, Mathf.RoundToInt(character.AttackDamage * Mathf.Max(0f, multiplier) * amplification * criticalMultiplier));
        int healthBeforeHit = target.CurrentHealth;
        target.TakeDamage(damage);

        int healthLost = Mathf.Max(0, healthBeforeHit - target.CurrentHealth);
        if (healthLost > 0 && target is Enemy)
        {
            UIManager.Ins.GetUI<CanvasCombat>().ShowDamage(damage, isCritical, target.transform);
        }

        if (healthLost > 0 && stats.CurrentLifeSteal > 0f)
        {
            character.Heal(Mathf.RoundToInt(healthLost * stats.CurrentLifeSteal));
        }
    }
}
