using UnityEngine;
using System;
[Serializable]
public class CharacterCombat 
{
    [SerializeField] private Character character ;
    [SerializeField, Min(0f)] private float basicAttackRange = 1.5f;

    public Character Character => character;
    public float BasicAttackRange => Mathf.Max(0f, basicAttackRange);
    public bool IsInitialized { get; private set; }

    public void OnInit()
    {
        IsInitialized = true;
    }

    public void OnDespawn()
    {
        IsInitialized = false;
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
