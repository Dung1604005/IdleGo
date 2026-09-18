using UnityEngine;

public class Enemy : Character
{

    protected override void Die()
    {
        base.Die();
        SimplePool.Despawn(this);
    }

}
