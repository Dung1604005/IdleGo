using UnityEngine;

public class AutoCombat : MonoBehaviour
{
    [SerializeField] private Character character = null;
    [SerializeField] private PlayerManager playerManager = null;
    [SerializeField] private EnemyManager enemyManager = null;
    [SerializeField] private CombatSkill[] skills = new CombatSkill[0];

    private Character target;
    private double[] readyAt;
    private double nextActionAt;
    private bool isInitialized;

    public Character Target => target;

    public void OnInit()
    {
        isInitialized = false;

        if (character == null ||
            (character is Player && enemyManager == null) ||
            (character is Enemy && playerManager == null) ||
            (!(character is Player) && !(character is Enemy)))
        {
            Debug.LogError("AutoCombat needs a Player or Enemy and the opposing manager assigned in the Inspector.", this);
            enabled = false;
            return;
        }

        if (!character.IsInitialized)
        {
            Debug.LogError("Call Character.OnInit() before AutoCombat.OnInit().", this);
            return;
        }

        if (skills == null)
        {
            skills = new CombatSkill[0];
        }

        readyAt = new double[skills.Length];
        nextActionAt = Time.timeAsDouble;
        target = null;
        isInitialized = true;
        enabled = true;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        double now = Time.timeAsDouble;
        if (character.IsDead || now < nextActionAt)
        {
            return;
        }

        target = GetTargetFromManager();

        if (target == null)
        {
            nextActionAt = now + ActionInterval;
            return;
        }

        int skillIndex = SelectSkill(now);
        if (skillIndex >= 0)
        {
            CombatSkill skill = skills[skillIndex];
            skill.Execute(character, target);
            readyAt[skillIndex] = now + Mathf.Max(0.01f, skill.Cooldown * (1f - Mathf.Clamp01(character.Stats.CurrentCooldownReduction)));
        }
        else
        {
            character.Attack(target);
        }

        nextActionAt = now + ActionInterval;
    }

    private float ActionInterval => 1f / Mathf.Max(0.01f, character.Stats.CurrentAttackSpeed);

    private Character GetTargetFromManager()
    {
        if (character is Player)
        {
            return enemyManager.GetTarget();
        }

        return playerManager.GetTarget();
    }

    private int SelectSkill(double now)
    {
        int selected = -1;
        double newestReadyAt = double.NegativeInfinity;

        for (int i = 0; i < skills.Length; i++)
        {
            CombatSkill skill = skills[i];
            if (skill == null || readyAt[i] > now || !skill.CanUse(character, target))
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
