using System;
using UnityEngine;

[Serializable]
public class CombatSkillState
{
    [SerializeField] private CombatSkill skill;
    [SerializeField] private float currentCooldown;
    [SerializeField] private int level;
    [SerializeField] private bool isUnlocked;

    private double lastReadyAt;

    public CombatSkill Skill => skill;
    public float CurrentCooldown => currentCooldown;
    public int Level => level;
    public bool IsUnlocked => isUnlocked;
    public bool IsReady => skill != null && isUnlocked && currentCooldown <= 0f;
    public double LastReadyAt => lastReadyAt;

    public void OnInit(CombatSkill combatSkill, int characterLevel)
    {
        skill = combatSkill;
        currentCooldown = 0f;
        level = 0;
        isUnlocked = skill != null && skill.IsUnlocked(characterLevel);
        lastReadyAt = double.NegativeInfinity;
    }

    public void OnDespawn()
    {
        currentCooldown = 0f;
        isUnlocked = false;
    }

    public void SetLevel(int value)
    {
        int maximumIndex = skill != null ? Mathf.Max(0, skill.MaxLevel - 1) : 0;
        level = Mathf.Min(Mathf.Max(0, value), maximumIndex);
    }

    public void RefreshUnlock(int characterLevel, double now)
    {
        bool unlocked = skill != null && skill.IsUnlocked(characterLevel);
        if (unlocked && !isUnlocked && currentCooldown <= 0f)
        {
            lastReadyAt = now;
        }

        isUnlocked = unlocked;
    }

    public void TickCooldown(float deltaTime, double now)
    {
        if (currentCooldown <= 0f)
        {
            return;
        }

        float elapsed = Mathf.Max(0f, deltaTime);
        float remaining = currentCooldown;
        currentCooldown = Mathf.Max(0f, remaining - elapsed);

        if (currentCooldown <= 0f)
        {
            lastReadyAt = now - Mathf.Max(0f, elapsed - remaining);
        }
    }

    public void StartCooldown(float cooldownReduction)
    {
        if (skill == null)
        {
            return;
        }

        currentCooldown = Mathf.Max(0.01f, skill.Cooldown * (1f - Mathf.Clamp01(cooldownReduction)));
    }
}
