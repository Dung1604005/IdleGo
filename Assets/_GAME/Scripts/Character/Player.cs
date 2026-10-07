using UnityEngine;


public class Player : Character
{
    [SerializeField] protected CharacterEquipment equipment = new CharacterEquipment();
    [SerializeField] private CharacterStatProgress statProgress =
        new CharacterStatProgress();

    public CharacterEquipment Equipment => equipment;
    internal CharacterStatProgress StatProgress => statProgress;

    protected override void OnStatsInitialized()
    {
        statProgress ??= new CharacterStatProgress();
        statProgress.OnInit(characterDataSO.StatProgressSO, this);
        if (equipment == null)
        {
            equipment = new CharacterEquipment();
        }

        equipment.OnInit(this);
        Stats.RestoreHealthToMax();
    }

    public override void OnDespawn()
    {
        statProgress?.OnDespawn();
        equipment?.OnDespawn();
        base.OnDespawn();
    }
}
