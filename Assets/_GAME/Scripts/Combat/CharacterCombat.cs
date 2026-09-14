using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable]
public class CharacterCombat 
{
    [SerializeField] private Character character ;
    [SerializeField, Min(0f)] private float basicAttackRange;

    [SerializeField] private List<CombatSkillState> combatSkillStates = new List<CombatSkillState>();

    private static readonly List<CharacterCombat> activeCombats = new List<CharacterCombat>();
    private static int lastProcessedFrame = -1;

    private CharacterMovement movement = new CharacterMovement();
    private Character target;
    private double nextActionAt;

    public Character Character => character;
    public float BasicAttackRange => Mathf.Max(0f, basicAttackRange);
    public bool IsInitialized { get; private set; }
    public IReadOnlyList<CombatSkillState> CombatSkillStates => combatSkillStates;
    public CharacterMovement Movement => movement;
    public Character Target => target;
    public double NextActionAt => nextActionAt;
    public static IReadOnlyList<CharacterCombat> ActiveCombats => activeCombats;

    public void OnInit(CharacterCombatSO combatSO)
    {
        OnDespawn();

        if (character == null || combatSO == null)
        {
            Debug.LogError("CharacterCombat needs a Character and CharacterCombatSO before OnInit().");
            return;
        }

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

        movement = new CharacterMovement();
        nextActionAt = 0d;
        target = null;
        IsInitialized = true;
        activeCombats.Add(this);
    }

    public void OnDespawn()
    {
        activeCombats.Remove(this);
        if (combatSkillStates != null)
        {
            for (int i = 0; i < combatSkillStates.Count; i++)
            {
                combatSkillStates[i]?.OnDespawn();
            }
        }

        movement?.OnDespawn();
        target = null;
        nextActionAt = 0d;
        IsInitialized = false;
    }

    public static bool TryClaimFrame(int frame)
    {
        if (lastProcessedFrame == frame)
        {
            return false;
        }

        lastProcessedFrame = frame;
        return true;
    }

    public void EnsureMovementInitialized()
    {
        if (!movement.IsInitialized && character != null && character.IsInitialized)
        {
            movement.OnInit(character);
        }
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
