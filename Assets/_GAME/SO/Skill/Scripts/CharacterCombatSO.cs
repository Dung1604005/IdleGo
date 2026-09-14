using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DamageSkill", menuName = "IdleGo/CombatData")]
public class CharacterCombatSO : ScriptableObject
{
    [SerializeField] private float baseRangeAttack;


    [SerializeField] private List<CombatSkill> listCombatSkill = new List<CombatSkill>();

    public float BaseRangeAttack => baseRangeAttack;

    public List<CombatSkill> GetCombatSkills()
    {
        return listCombatSkill;
    }


} 
