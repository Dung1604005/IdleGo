using System;
using UnityEngine;

public abstract class CombatSkill : ScriptableObject
{
    [SerializeField] private string skillId;
    [SerializeField, Min(0.01f)] private float cooldown = 3f;

    [SerializeField] protected int maxLevel;

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
        return character != null
            && IsUnlocked(character.Stats.CurrentLevel)
            && CharacaterClassTypeUtility.Matches(
                characterRequirement,
                character.CharacaterClassType);
    }
}
