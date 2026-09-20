using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStat", menuName = "IdleGo/CharacterStatSO")]
public class CharacterStatSO : ScriptableObject
{
    [SerializeField, Min(1)] private int baseMaxHealth = 100;
    [SerializeField, Min(0f)] private float baseRunSpeed = 5f;
    [SerializeField, Min(0)] private int baseDamage = 10;
    [SerializeField, Min(1)] private int baseLevel = 1;
    [SerializeField, Min(0)] private int baseExperience = 0;
    [SerializeField, Min(0.01f)] private float baseAttackSpeed = 1f;
    [SerializeField, Range(0f, 1f)] private float baseCriticalChance = 0.05f;
    [SerializeField, Min(1f)] private float baseCriticalDamage = 1.5f;
    [SerializeField, Range(0f, 1f)] private float baseCooldownReduction = 0f;
    [SerializeField, Min(0)] private int baseArmor = 0;
    [SerializeField, Range(0f, 1f)] private float baseLifeSteal = 0f;
    [SerializeField, Range(0f, 1f)] private float baseDodgeChance = 0f;
    [SerializeField, Min(0f)] private float baseDamageAmplification = 0f;

    public int BaseMaxHealth => Mathf.Max(1, baseMaxHealth);
    public float BaseRunSpeed => Mathf.Max(0f, baseRunSpeed);
    public int BaseDamage => Mathf.Max(0, baseDamage);
    public int BaseLevel => Mathf.Max(1, baseLevel);
    public int BaseExperience => Mathf.Max(0, baseExperience);
    public float BaseAttackSpeed => Mathf.Max(0.01f, baseAttackSpeed);
    public float BaseCriticalChance => Mathf.Clamp01(baseCriticalChance);
    public float BaseCriticalDamage => Mathf.Max(1f, baseCriticalDamage);
    public float BaseCooldownReduction => Mathf.Clamp01(baseCooldownReduction);
    public int BaseArmor => Mathf.Max(0, baseArmor);
    public float BaseLifeSteal => Mathf.Clamp01(baseLifeSteal);
    public float BaseDodgeChance => Mathf.Clamp01(baseDodgeChance);
    public float BaseDamageAmplification => Mathf.Max(0f, baseDamageAmplification);
}
