using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public partial class CharacterCombat
{
    public const int MaxEquippedSkillCount = 2;

    [SerializeField] private Character character;
    [SerializeField, Min(0f)] private float basicAttackRange;
    [SerializeField] private List<CombatSkillState> combatSkillStates = new List<CombatSkillState>();
    [SerializeField] private float delayAttack;
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool hasExecutedAttack;

    private AttackType basicAttackType;
    [NonSerialized] private CharacterCombatSO combatData;
    private Character target;
    private Character activeAttackTarget;
    private CombatSkillState activeSkillState;
    private double nextActionAt;

    public Character Character => character;
    public bool IsAttacking => isAttacking;
    public float BasicAttackRange => Mathf.Max(0f, basicAttackRange);
    public bool IsInitialized { get; private set; }
    public double NextActionAt => nextActionAt;
    public IReadOnlyList<CombatSkillState> EquippedSkillStates => combatSkillStates;

    public void OnInit(CharacterCombatSO combatSO)
    {
        OnDespawn();
        combatData = combatSO;
        basicAttackRange = combatSO != null ? combatSO.BaseRangeAttack : 0f;
        basicAttackType = combatSO != null ? combatSO.BasicAttackType : null;
        delayAttack = combatSO != null ? combatSO.DelayAttack : 0f;
        combatSkillStates ??= new List<CombatSkillState>();
        combatSkillStates.Clear();

        IReadOnlyList<CombatSkill> skills = combatSO != null
            ? combatSO.GetCombatSkills()
            : null;
        if (skills != null)
        {
            for (int i = 0; i < skills.Count && combatSkillStates.Count < MaxEquippedSkillCount; i++)
            {
                TryEquipSkillInternal(skills[i]);
            }
        }

        ResetRuntimeState();
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
        combatData = null;
        ResetRuntimeState();
        IsInitialized = false;
    }

    public void SetTarget(Character opponent)
    {
        if (target == opponent)
        {
            return;
        }

        // Chi target cua Player moi thay doi sorting cua Enemy.
        SetPlayerTargetSorting(target, false);
        target = opponent;
        SetPlayerTargetSorting(target, true);
    }

    private void ResetRuntimeState()
    {
        target = null;
        activeAttackTarget = null;
        activeSkillState = null;
        isAttacking = false;
        hasExecutedAttack = false;
        nextActionAt = 0d;
    }

    private void SetPlayerTargetSorting(Character opponent, bool isTarget)
    {
        if (character is Player && opponent is Enemy enemy)
        {
            enemy.SetAsPlayerTarget(isTarget);
        }
    }
}
