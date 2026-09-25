using UnityEngine;


public class Player : Character
{
    [SerializeField] protected CharacterEquipment equipment = new CharacterEquipment();

    public CharacterEquipment Equipment => equipment;
    protected override void OnStatsInitialized()
    {
        if (equipment == null)
        {
            equipment = new CharacterEquipment();
        }

        equipment.OnInit(this);
        Stats.RestoreHealthToMax();
    }

    public override void OnDespawn()
    {
        equipment?.OnDespawn();
        base.OnDespawn();
    }
}
