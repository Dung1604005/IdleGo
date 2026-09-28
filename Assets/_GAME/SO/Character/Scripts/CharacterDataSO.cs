using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "IdleGo/CharacterDataSO")]
public class CharacterDataSO : ScriptableObject
{
    [SerializeField] private string characterId;
    [SerializeField] private String characterName;
    [SerializeField] private CharacterType characterType;
    [SerializeField] private CharacterStatSO characterStatSO;

    [SerializeField] private CharacterCombatSO characterCombatSO;

    public string CharacterId => string.IsNullOrWhiteSpace(characterId)
        ? name
        : characterId.Trim();

    public String CharacterName => characterName;

    public CharacterType CharacterType => characterType;

    public CharacterStatSO StatSO => characterStatSO;

    public CharacterCombatSO CombatSO => characterCombatSO;
}
