using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterStatProgress",
    menuName = "IdleGo/Character/Stat Progress")]
public sealed class CharacterStatProgressSO : ScriptableObject
{
    [SerializeField] private List<CharacterStatUpgradeRule> statUpgradeRules =
        new List<CharacterStatUpgradeRule>();
    [SerializeField] private List<CombatSkill> combatSkills =
        new List<CombatSkill>();

    public IReadOnlyList<CharacterStatUpgradeRule> StatUpgradeRules =>
        statUpgradeRules;
    public IReadOnlyList<CombatSkill> CombatSkills => combatSkills;

    public bool TryGetRule(
        StatType statType,
        out CharacterStatUpgradeRule rule)
    {
        for (int i = 0; i < statUpgradeRules.Count; i++)
        {
            if (statUpgradeRules[i].StatType == statType
                && StatTypeUtility.CanHaveModifiers(statType))
            {
                rule = statUpgradeRules[i];
                return true;
            }
        }

        rule = default;
        return false;
    }

    public bool ContainsSkill(CombatSkill skill)
    {
        return skill != null && combatSkills.Contains(skill);
    }

    private void OnValidate()
    {
        statUpgradeRules ??= new List<CharacterStatUpgradeRule>();
        combatSkills ??= new List<CombatSkill>();
    }
}

[Serializable]
public struct CharacterStatUpgradeRule
{
    [SerializeField] private StatType statType;
    [SerializeField] private float valuePerUpgrade;
    [SerializeField, Min(1)] private int statPointCost;

    public StatType StatType => statType;
    public float ValuePerUpgrade => valuePerUpgrade;
    public int StatPointCost => Mathf.Max(1, statPointCost);
}
