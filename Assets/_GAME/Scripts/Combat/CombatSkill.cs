using System;
using UnityEngine;

public abstract class CombatSkill : ScriptableObject
{
    [SerializeField, Min(0.01f)] private float cooldown = 3f;

    [SerializeField] private Sprite iconSkill;

    [SerializeField] private String nameAnim;

    public float Cooldown => Mathf.Max(0.01f, cooldown);
    public abstract float Range { get; }

    public Sprite IconSkill => iconSkill;

    public String NameAnim => nameAnim;

    public abstract bool CanUse(CharacterCombat user, Character target);
    public abstract void Execute(CharacterCombat user, Character target);
}
