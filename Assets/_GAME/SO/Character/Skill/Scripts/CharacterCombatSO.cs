using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DamageSkill", menuName = "IdleGo/CombatData")]
public class CharacterCombatSO : ScriptableObject
{
    [SerializeField] private float baseRangeAttack;
    [SerializeField] private AttackType basicAttackType;

    [SerializeField] private float delayAttack;
    [SerializeField] private List<CombatSkill> listCombatSkill = new List<CombatSkill>();

    public float BaseRangeAttack => baseRangeAttack;

    public float DelayAttack => delayAttack;
    public AttackType BasicAttackType => basicAttackType;

    public List<CombatSkill> GetCombatSkills()
    {
        return listCombatSkill;
    }


} 
