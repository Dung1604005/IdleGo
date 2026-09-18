using System;
using NUnit.Framework.Constraints;
using UnityEngine;

public abstract class Character : GameUnit
{
    [SerializeField] protected CharacterStat stats = new CharacterStat();

    [SerializeField] protected CharacterCombat combat = new CharacterCombat();

    [SerializeField] protected CharacterMovement movement = new CharacterMovement();

    [SerializeField] protected CharacterDataSO characterDataSO;

    [SerializeField] protected AutoCombat autoCombat;

    [SerializeField] protected CharacterHealthBar healthBar;

    [SerializeField]protected Animator animator;

    protected String currentAnim;

    public CharacterStat Stats => stats;

    public CharacterMovement Movement => movement;

    public CharacterCombat Combat => combat;

    public int MaxHealth => stats != null ? stats.CurrentMaxHealth : 0;
    public int AttackDamage => stats != null ? Mathf.Max(0, stats.CurrentDamage) : 0;
    public int CurrentHealth => stats != null ? stats.CurrentHealth : 0;
    public bool IsDead => stats == null || stats.IsDead;
    public bool IsInitialized { get; private set; }

    public virtual void OnInit()
    {
        IsInitialized = true;
        stats.OnInit(characterDataSO.StatSO, this);
        combat.OnInit(characterDataSO.CombatSO);
        movement.OnInit(this);
        healthBar?.OnInit();
    }

    public override void OnSpawn()
    {
        // Object lấy lại từ pool phải reset máu, combat, movement và health bar.
        OnInit();
    }

    public override void OnDespawn()
    {
        combat.OnDespawn();
        movement.OnDespawn();
        healthBar?.OnDespawn();
        IsInitialized = false;
    }

    public virtual void Despawn()
    {
        
    }

    public virtual void TakeDamage(int damage)
    {
        if (!IsInitialized || IsDead || damage <= 0)
        {
            return;
        }

        if (stats.TakeDamage(damage))
        {
            Die();
        }
    }

    public virtual void Heal(int amount)
    {
        if (IsInitialized)
        {
            stats.Heal(amount);
        }
    }

    protected virtual void Die()
    {
        movement.Stop();
        ChangeAnim(GameConfig.ANIM_DIE);
        combat.SetTarget(null);
    }

    public virtual void ChangeAnim(String newAnim)
    {
        if (!String.IsNullOrEmpty(newAnim) && newAnim != currentAnim)
        {
            animator.ResetTrigger(currentAnim);
            currentAnim = newAnim;
            animator.SetTrigger(currentAnim);
        }
    }
    protected virtual void Awake()
    {
        OnInit();
    }

    protected virtual void Update()
    {
        if(IsDead)return;
        autoCombat.UpdateAutoCombat(combat);
    }
}
