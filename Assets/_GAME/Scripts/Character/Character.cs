using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private CharacterStat stats = new CharacterStat();

    public CharacterStat Stats => stats;
    public int MaxHealth => stats != null ? stats.CurrentMaxHealth : 0;
    public int AttackDamage => stats != null ? Mathf.Max(0, stats.CurrentDamage) : 0;
    public int CurrentHealth => stats != null ? stats.CurrentHealth : 0;
    public bool IsDead => stats == null || stats.IsDead;
    public bool IsInitialized { get; private set; }

    public virtual void OnInit()
    {
        if (stats == null)
        {
            stats = new CharacterStat();
        }

        stats.OnInit();
        IsInitialized = true;
    }

    public virtual void Attack(Character target)
    {
        DealDamage(target, 1f);
    }

    public virtual void DealDamage(Character target, float multiplier)
    {
        if (!IsInitialized || IsDead || target == null || target.IsDead)
        {
            return;
        }

        float criticalChance = Mathf.Clamp01(stats.CurrentCriticalChance);
        bool isCritical = criticalChance >= 1f || Random.value < criticalChance;
        float criticalMultiplier = isCritical ? Mathf.Max(1f, stats.CurrentCriticalDamage) : 1f;
        int damage = Mathf.Max(0, Mathf.RoundToInt(AttackDamage * Mathf.Max(0f, multiplier) * criticalMultiplier));
        target.TakeDamage(damage);
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
