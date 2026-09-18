using UnityEngine;

public class Enemy : Character
{

    public override void Despawn()
    {
        base.Despawn();
        EnemyManager.Ins.DespawnEnemy(this);
    }


}
