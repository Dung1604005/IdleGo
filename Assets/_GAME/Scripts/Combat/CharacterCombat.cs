using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable]
public class CharacterCombat 
{
    public const int MaxEquippedSkillCount = 2;

    [SerializeField] private Character character ;
    [SerializeField, Min(0f)] private float basicAttackRange;

    [SerializeField] private List<CombatSkillState> combatSkillStates = new List<CombatSkillState>();

    [SerializeField] private float delayAttack;

    private AttackType basicAttackType;
    [NonSerialized] private CharacterCombatSO combatData;
    private Character target;
    private Character activeAttackTarget;
    private CombatSkillState activeSkillState;

    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool hasExecutedAttack;
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
        isAttacking = false;
        basicAttackRange = combatSO != null ? combatSO.BaseRangeAttack : 0f;
        basicAttackType = combatSO != null ? combatSO.BasicAttackType : null;
        if (combatSkillStates == null)
        {
            combatSkillStates = new List<CombatSkillState>();
        }
        delayAttack = combatSO != null ? combatSO.DelayAttack : 0f;
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
        nextActionAt = 0d;
        target = null;
        activeAttackTarget = null;
        activeSkillState = null;
        hasExecutedAttack = false;
        IsInitialized = true;
    }

    public bool CanEquipSkill(CombatSkill skill)
    {
        return IsInitialized
            && combatSkillStates.Count < MaxEquippedSkillCount
            && IsSkillAvailable(skill)
            && skill.CanEquip(character)
            && GetSkillState(skill) == null;
    }

    public bool EquipSkill(CombatSkill skill)
    {
        return CanEquipSkill(skill) && TryEquipSkillInternal(skill);
    }

    public bool UnequipSkill(CombatSkill skill)
    {
        CombatSkillState state = GetSkillState(skill);
        if (!IsInitialized || state == null || ReferenceEquals(state, activeSkillState))
        {
            return false;
        }

        state.OnDespawn();
        combatSkillStates.Remove(state);
        return true;
    }

    public void ClearEquippedSkills()
    {
        if (combatSkillStates == null || isAttacking)
        {
            return;
        }

        for (int i = 0; i < combatSkillStates.Count; i++)
        {
            combatSkillStates[i]?.OnDespawn();
        }

        combatSkillStates.Clear();
    }

    public CombatSkill GetAvailableSkill(string skillId)
    {
        if (combatData == null || string.IsNullOrWhiteSpace(skillId))
        {
            return null;
        }

        IReadOnlyList<CombatSkill> availableSkills = combatData.GetCombatSkills();
        for (int i = 0; i < availableSkills.Count; i++)
        {
            CombatSkill skill = availableSkills[i];
            if (skill != null && skill.SkillId == skillId)
            {
                return skill;
            }
        }

        return null;
    }

    public bool SetEquippedSkillLevel(CombatSkill skill, int level)
    {
        CombatSkillState state = GetSkillState(skill);
        if (state == null)
        {
            return false;
        }

        state.SetLevel(level);
        return true;
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

    private bool TryEquipSkillInternal(CombatSkill skill)
    {
        if (skill == null
            || combatSkillStates.Count >= MaxEquippedSkillCount
            || !skill.CanEquip(character)
            || GetSkillState(skill) != null)
        {
            return false;
        }

        // Chi skill da dat level va CharacterType requirement moi duoc tao state chien dau.
        CombatSkillState state = new CombatSkillState();
        state.OnInit(skill, character.Stats.CurrentLevel);
        combatSkillStates.Add(state);
        return true;
    }

    private bool IsSkillAvailable(CombatSkill skill)
    {
        if (skill == null || character == null || combatData == null)
        {
            return false;
        }

        IReadOnlyList<CombatSkill> availableSkills = combatData.GetCombatSkills();
        for (int i = 0; i < availableSkills.Count; i++)
        {
            if (ReferenceEquals(availableSkills[i], skill))
            {
                return true;
            }
        }

        return false;
    }

    private CombatSkillState GetSkillState(CombatSkill skill)
    {
        for (int i = 0; i < combatSkillStates.Count; i++)
        {
            CombatSkillState state = combatSkillStates[i];
            if (state != null && ReferenceEquals(state.Skill, skill))
            {
                return state;
            }
        }

        return null;
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
        nextActionAt = now + delayAttack / Mathf.Max(0.01f, character.Stats.GetCurrentStat(StatType.ATTACK_SPEED));
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
        character.ChangeAnim(GameConfig.ANIM_IDLE);
        if (!isAttacking)
        {
            return;
        }

        // Animation Event là thời điểm xác nhận đòn đánh đã hoàn tất và bắt đầu hồi skill.
        if (activeSkillState != null)
        {
            activeSkillState.StartCooldown(character.Stats.GetCurrentStat(StatType.COOLDOWN_REDUCTION));
        }

        activeAttackTarget = null;
        activeSkillState = null;
        hasExecutedAttack = false;
        SetIsAttacking(false);
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
        float criticalChance = stats.GetCurrentStat(StatType.CRITICAL_CHANCE);
        bool isCritical = criticalChance >= 1f || UnityEngine.Random.value < criticalChance;
        float criticalMultiplier = isCritical ? stats.GetCurrentStat(StatType.CRITICAL_DAMAGE) : 1f;
        float amplification = 1f + stats.GetCurrentStat(StatType.DAMAGE_AMPLIFICATION);
        int damage = Mathf.Max(0, Mathf.RoundToInt(character.AttackDamage * Mathf.Max(0f, multiplier) * amplification * criticalMultiplier));
        int healthBeforeHit = target.CurrentHealth;
        target.TakeDamage(damage);

        int healthLost = Mathf.Max(0, healthBeforeHit - target.CurrentHealth);
        if (healthLost > 0 && target is Enemy)
        {
            UIManager.Ins.GetUI<CanvasCombat>().ShowDamage(damage, isCritical, target.transform);
        }

        if (healthLost > 0 && stats.GetCurrentStat(StatType.LIFE_STEAL) > 0f)
        {
            character.Heal(Mathf.RoundToInt(healthLost * stats.GetCurrentStat(StatType.LIFE_STEAL)));
        }
    }
}
