using UnityEngine;

public sealed class CharacterLevelProgression
{
    private const int MaxLevelUpsPerCheck = 100000;
    private readonly CharacterStat stats;

    public CharacterLevelProgression(CharacterStat characterStats)
    {
        stats = characterStats;
    }

    public int GetExpToNextLevel(int level)
    {
        return Mathf.RoundToInt(
            GameConfig.BASE_EXP * Mathf.Pow(level, GameConfig.POWER_EXP));
    }

    public void AddExperience(int amount)
    {
        if (amount > 0)
        {
            stats.AddCurrentStat(StatType.EXPERIENCE, amount);
        }
    }

    public void CheckLevelUp()
    {
        // Gioi han vong lap de save hong khong the khoa game vo han.
        for (int i = 0; i < MaxLevelUpsPerCheck; i++)
        {
            int requiredExperience = GetExpToNextLevel(stats.CurrentLevel);
            if (stats.CurrentExperience < requiredExperience)
            {
                return;
            }

            stats.SetCurrentStat(
                StatType.EXPERIENCE,
                stats.CurrentExperience - requiredExperience);
            stats.SetCurrentStat(StatType.LEVEL, stats.CurrentLevel + 1);
        }
    }
}
