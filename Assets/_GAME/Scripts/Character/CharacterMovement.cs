using System;
using UnityEngine;

[Serializable]
public class CharacterMovement
{
    private Character character;

    public bool IsInitialized { get; private set; }
    public bool IsMoving { get; private set; }

    public void OnInit(Character owner)
    {
        character = owner;
        IsInitialized = owner != null && owner.IsInitialized;
        IsMoving = false;
    }

    public void OnDespawn()
    {
        Stop();
        IsInitialized = false;
        character = null;
    }

    public bool CanMove(Character target)
    {
        return !(!IsInitialized || character == null || character.IsDead || target == null || target.IsDead|| character.Combat.IsAttacking);
    }
    public bool MoveToward(Character target, float stoppingDistance)
    {
        if (!CanMove(target))
        {
            Stop();
            return false;
        }

        Vector3 currentPosition = character.TF.position;
        Vector3 targetPosition = target.TF.position;
        float distance = Vector3.Distance(currentPosition, targetPosition);
        float range = Mathf.Max(0f, stoppingDistance);

        

        if (distance <= range + 0.01f)
        {
            Stop();
            return true;
        }

        float step = character.Stats.GetCurrentStat(StatType.RUN_SPEED) * Time.deltaTime;
        if (step <= 0.01f)
        {
            Stop();
            return false;
        }

        character.ChangeAnim(GameConfig.ANIM_RUN);
        float movement = Mathf.Min(step, distance - range);
        character.transform.position = Vector3.MoveTowards(currentPosition, targetPosition, movement);
        IsMoving = Vector3.Distance(character.TF.position, targetPosition) > range + 0.0001f;
        return !IsMoving;
    }

    public void Stop()
    {
        //character.ChangeAnim(GameConfig.ANIM_IDLE);
        IsMoving = false;
    }
}
