using UnityEngine;


public class Player : Character
{
    [SerializeField] protected CharacterEquipment equipment = new CharacterEquipment();
    [SerializeField] private Inventory inventory = new Inventory();

    public CharacterEquipment Equipment => equipment;
    public Inventory Inventory => inventory;

    protected override void OnStatsInitialized()
    {
        if (equipment == null)
        {
            equipment = new CharacterEquipment();
        }

        equipment.OnInit(this);
        if (inventory == null)
        {
            inventory = new Inventory();
        }

        inventory.OnInit(this);
        Stats.RestoreHealthToMax();
    }

    public override void OnDespawn()
    {
        inventory?.OnDespawn();
        equipment?.OnDespawn();
        base.OnDespawn();
    }
}
