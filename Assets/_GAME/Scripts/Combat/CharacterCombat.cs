using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable]
public class CharacterCombat 
{
    [SerializeField] private Character character ;
    [SerializeField, Min(0f)] private float basicAttackRange;

    [SerializeField] private List<CombatSkillState> combatSkillStates = new List<CombatSkillState>();

    private AttackType basicAttackType;
    private Character target;
    private CombatSkillState activeSkillState;

    private bool isAttacking = false;
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
        activeSkillState = null;
        IsInitialized = true;
    }

    public void OnDespawn()
    {
        if (combatSkillStates != null)
        {
            for (int i = 0; i < combatSkillStates.Count; i++)
            {
                combatSkillStates[i]?.OnDespawn();
            }
        }
        target = null;
        basicAttackType = null;
        activeSkillState = null;
        isAttacking = false;
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
        target = opponent;
    }

    public void SetIsAttacking(bool val)
    {
        isAttacking = val;
    }

    public void MarkAction(double now)
    {
        nextActionAt = now + 1f / Mathf.Max(0.01f, character.Stats.CurrentAttackSpeed);
    }

    public void StartAttack(String animAttack, CombatSkillState skillState)
    {
        // Giữ lại skill đang thi triển để cooldown chỉ bắt đầu khi animation kết thúc.
        activeSkillState = skillState;
        SetIsAttacking(true);
        character.ChangeAnim(animAttack);
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
            activeSkillState = null;
        }

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
        if (healthLost > 0 && stats.CurrentLifeSteal > 0f)
        {
            character.Heal(Mathf.RoundToInt(healthLost * stats.CurrentLifeSteal));
        }
    }
}
