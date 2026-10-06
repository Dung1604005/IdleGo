using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "IdleGo/CharacterDataSO")]
public class CharacterDataSO : ScriptableObject
{
    [SerializeField] private string characterId;
    [SerializeField] private String characterName;

    [SerializeField] private Sprite portrait;
    [FormerlySerializedAs("characterType")]
    [SerializeField] private CharacaterClassType characaterClassType;
    [SerializeField] private CharacterStatSO characterStatSO;

    [SerializeField] private CharacterCombatSO characterCombatSO;

    public string CharacterId => string.IsNullOrWhiteSpace(characterId)
        ? name
        : characterId.Trim();

    public String CharacterName => characterName;

    public CharacaterClassType CharacaterClassType => characaterClassType;

    public CharacterStatSO StatSO => characterStatSO;

    public CharacterCombatSO CombatSO => characterCombatSO;

    public Sprite Portrait => portrait;
}
