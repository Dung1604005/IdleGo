using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable]
public class CharacterCombat 
{
    [SerializeField] private Character character ;
    [SerializeField, Min(0f)] private float basicAttackRange;

    [SerializeField] private List<CombatSkillState> combatSkillStates = new List<CombatSkillState>();

    private Character target;
    private double nextActionAt;

    public Character Character => character;
    public float BasicAttackRange => Mathf.Max(0f, basicAttackRange);
    public bool IsInitialized { get; private set; }
    public double NextActionAt => nextActionAt;

    public void OnInit(CharacterCombatSO combatSO)
    {
        OnDespawn();
        basicAttackRange = combatSO.BaseRangeAttack;
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

    public void MarkAction(double now)
    {
        nextActionAt = now + 1f / Mathf.Max(0.01f, character.Stats.CurrentAttackSpeed);
    }

    public virtual void Attack(Character target)
    {
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
