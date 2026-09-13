using NUnit.Framework.Constraints;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private CharacterStat stats = new CharacterStat();

    [SerializeField] private CharacterCombat combat = new CharacterCombat();

    [SerializeField] private CharacterMovement movement = new CharacterMovement();

    public CharacterStat Stats => stats;
    public int MaxHealth => stats != null ? stats.CurrentMaxHealth : 0;
    public int AttackDamage => stats != null ? Mathf.Max(0, stats.CurrentDamage) : 0;
    public int CurrentHealth => stats != null ? stats.CurrentHealth : 0;
    public bool IsDead => stats == null || stats.IsDead;
    public bool IsInitialized { get; private set; }

    public virtual void OnInit()
    {
    
        stats.OnInit();
        combat.OnInit();
        movement.OnInit(this);

        IsInitialized = true;
    }

    public virtual void OnDespawn()
    {
        IsInitialized = false;
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
    }
}
