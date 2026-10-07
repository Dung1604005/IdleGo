using UnityEngine;

public sealed class CharacterHealthController
{
    public bool TakeDamage(
        CharacterStat stats,
        Character character,
        ref int currentHealth,
        int incomingDamage)
    {
        if (stats == null
            || currentHealth <= 0
            || incomingDamage <= 0
            || CanDodge(stats))
        {
            return false;
        }

        character?.ChangeAnim(GameConfig.ANIM_HURT);
        // Armor doi thanh phan tram giam damage; don danh trung van gay toi thieu 1 damage.
        float reduction = CombatFormula.CalculateArmorDamageReduction(
            stats.GetCurrentStat(StatType.ARMOR));
        int damage = Mathf.Max(
            1,
            Mathf.RoundToInt(incomingDamage * (1f - reduction)));
        currentHealth = Mathf.Max(0, currentHealth - damage);
        return currentHealth <= 0;
    }

    public bool CanDodge(CharacterStat stats)
    {
        if (stats == null)
        {
            return false;
        }

        float dodgeChance = stats.GetCurrentStat(StatType.DODGE_CHANCE);
        return dodgeChance >= 1f || Random.value < dodgeChance;
    }

    public void Heal(
        CharacterStat stats,
        ref int currentHealth,
        int amount)
    {
        if (stats == null || currentHealth <= 0 || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(
            stats.CurrentMaxHealth,
            currentHealth + amount);
    }
}
