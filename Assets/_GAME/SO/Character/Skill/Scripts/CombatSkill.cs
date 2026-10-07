using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatSkill : ScriptableObject
{
    [SerializeField] private string skillId;
    [SerializeField, Min(0.01f)] private float cooldown = 3f;

    [SerializeField, Min(1)] protected int maxLevel = 1;
    [SerializeField] private List<int> statPointCostByLevel = new List<int>();

    [SerializeField] private Sprite iconSkill;

    [SerializeField] private String nameAnim;

    [SerializeField] private int levelRequire;

    [SerializeField] private CharacterRequirementType characterRequirement =
        CharacterRequirementType.ALL;

    public string SkillId => string.IsNullOrWhiteSpace(skillId) ? name : skillId.Trim();

    public float Cooldown => Mathf.Max(0.01f, cooldown);
    public abstract float Range { get; }

    public Sprite IconSkill => iconSkill;

    public String NameAnim => nameAnim;

    public int MaxLevel => maxLevel;

    public int LevelRequire => Mathf.Max(0, levelRequire);

    public CharacterRequirementType CharacterRequirement => characterRequirement;

    public abstract bool CanUse(CharacterCombat user, Character target);
    public abstract void Execute(CharacterCombat user, Character target, int level);

    public bool IsUnlocked(int level)
    {
        return level >= LevelRequire;
    }

    public bool CanEquip(Character character)
    {
        return MeetsCharacterRequirements(character)
            && IsUnlocked(character.Stats.CurrentLevel);
    }

    public bool MeetsCharacterRequirements(Character character)
    {
        return character != null
            && CharacaterClassTypeUtility.Matches(
                characterRequirement,
                character.CharacaterClassType);
    }

    public int GetStatPointCost(int targetLevel)
    {
        int index = targetLevel - 1;
        if (index < 0 || index >= MaxLevel)
        {
            return 0;
        }

        return index < statPointCostByLevel.Count
            ? Mathf.Max(1, statPointCostByLevel[index])
            : Mathf.Max(1, targetLevel);
    }

    protected virtual void OnValidate()
    {
        maxLevel = Mathf.Max(1, maxLevel);
        statPointCostByLevel ??= new List<int>();
        while (statPointCostByLevel.Count < maxLevel)
        {
            statPointCostByLevel.Add(statPointCostByLevel.Count + 1);
        }
        if (statPointCostByLevel.Count > maxLevel)
        {
            statPointCostByLevel.RemoveRange(
                maxLevel,
                statPointCostByLevel.Count - maxLevel);
        }
        for (int i = 0; i < statPointCostByLevel.Count; i++)
        {
            statPointCostByLevel[i] = Mathf.Max(1, statPointCostByLevel[i]);
        }
    }
}
