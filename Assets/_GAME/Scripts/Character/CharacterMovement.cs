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

    public bool MoveToward(Character target, float stoppingDistance)
    {
        if (!IsInitialized || character == null || character.IsDead || target == null || target.IsDead)
        {
            Stop();
            return false;
        }

        Vector3 currentPosition = character.transform.position;
        Vector3 targetPosition = target.transform.position;
        float distance = Vector3.Distance(currentPosition, targetPosition);
        float range = Mathf.Max(0f, stoppingDistance);

        if (distance <= range)
        {
            Stop();
            return true;
        }

        float step = character.Stats.CurrentRunSpeed * Time.deltaTime;
        if (step <= 0f)
        {
            Stop();
            return false;
        }

        character.ChangeAnim(GameConfig.ANIM_RUN);
        float movement = Mathf.Min(step, distance - range);
        character.transform.position = Vector3.MoveTowards(currentPosition, targetPosition, movement);
        IsMoving = Vector3.Distance(character.transform.position, targetPosition) > range + 0.0001f;
        return !IsMoving;
    }

    public void Stop()
    {
        character.ChangeAnim(GameConfig.ANIM_IDLE);
        IsMoving = false;
    }
}
