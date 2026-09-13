using UnityEngine;

public class AutoCombat : MonoBehaviour
{
    [SerializeField] private Character character ;
    [SerializeField] private CharacterCombat characterCombat ;
    [SerializeField] private CharacterMovement movement = new CharacterMovement();
    [SerializeField] private CombatSkill[] skills = new CombatSkill[0];

    private Character target;
    private double[] readyAt;
    private double nextActionAt;
    private bool isInitialized;

    public Character Target => target;
    public bool IsMoving => movement != null && movement.IsMoving;

    public void OnInit()
    {
        isInitialized = false;

        if (character == null || characterCombat == null || characterCombat.Character != character ||
            !character.IsInitialized || !characterCombat.IsInitialized)
        {
            Debug.LogError("Assign matching Character and CharacterCombat, then call their OnInit() methods before AutoCombat.OnInit().", this);
            return;
        }

        if (skills == null)
        {
            skills = new CombatSkill[0];
        }

        if (movement == null)
        {
            movement = new CharacterMovement();
        }

        movement.OnInit(character);

        readyAt = new double[skills.Length];
        nextActionAt = Time.timeAsDouble;
        target = null;
        isInitialized = true;
        enabled = true;
    }

    public void OnDespawn()
    {
        movement?.OnDespawn();
        target = null;
        readyAt = null;
        isInitialized = false;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        double now = Time.timeAsDouble;
        if (character.IsDead)
        {
            movement.Stop();
            target = null;
            return;
        }

        target = GetTargetFromManager();

        if (target == null)
        {
            movement.Stop();
            return;
        }

        int skillIndex = SelectSkill(now);
        float range = skillIndex >= 0 ? skills[skillIndex].Range : characterCombat.BasicAttackRange;
        if (!movement.MoveToward(target, range) || now < nextActionAt)
        {
            return;
        }

        movement.Stop();
        if (skillIndex >= 0)
        {
            CombatSkill skill = skills[skillIndex];
            skill.Execute(characterCombat, target);
            readyAt[skillIndex] = now + Mathf.Max(0.01f, skill.Cooldown * (1f - Mathf.Clamp01(character.Stats.CurrentCooldownReduction)));
        }
        else
        {
            characterCombat.Attack(target);
        }

        nextActionAt = now + ActionInterval;
    }

    private float ActionInterval => 1f / Mathf.Max(0.01f, character.Stats.CurrentAttackSpeed);

    private Character GetTargetFromManager()
    {
        if (character is Player)
        {
            return EnemyManager.Ins.GetNearestTarget(character.transform.position);
        }

        return PlayerManager.Ins.GetNearestTarget(character.transform.position);
    }

    private int SelectSkill(double now)
    {
        int selected = -1;
        double newestReadyAt = double.NegativeInfinity;

        for (int i = 0; i < skills.Length; i++)
        {
            CombatSkill skill = skills[i];
            if (skill == null || readyAt[i] > now || !skill.CanUse(characterCombat, target))
            {
                continue;
            }

            if (readyAt[i] > newestReadyAt)
            {
                newestReadyAt = readyAt[i];
                selected = i;
            }
        }

        return selected;
    }
}
