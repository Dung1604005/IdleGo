using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "IdleGo/CharacterDataSO")]
public class CharacterDataSO : ScriptableObject
{
    [SerializeField] private String characterName;
    [SerializeField] private CharacterStatSO characterStatSO;

    [SerializeField] private CharacterCombatSO characterCombatSO;

    public String CharacterName => characterName;

    public CharacterStatSO StatSO => characterStatSO;

    public CharacterCombatSO CombatSO => characterCombatSO;
}
