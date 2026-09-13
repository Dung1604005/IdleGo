using UnityEngine;

public abstract class CombatSkill : ScriptableObject
{
    [SerializeField, Min(0.01f)] private float cooldown = 3f;

    public float Cooldown => Mathf.Max(0.01f, cooldown);

    public abstract bool CanUse(Character user, Character target);
    public abstract void Execute(Character user, Character target);
}
