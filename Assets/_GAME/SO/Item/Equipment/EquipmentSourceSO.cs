using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EquipmentSource",
    menuName = "IdleGo/Item/Equipment/Equipment Source")]
public class EquipmentSourceSO : ScriptableObject
{
    [SerializeField, Min(1)] private int levelRequired = 1;
    [SerializeField] private List<EquipmentDropEntry> equipments =
        new List<EquipmentDropEntry>();

    public int LevelRequired => Mathf.Max(1, levelRequired);
    public IReadOnlyList<EquipmentDropEntry> Equipments => equipments;

    public bool TryRollEquipment(out EquipmentDataSO equipment)
    {
        equipment = null;
        float totalRate = GetTotalRate();
        if (totalRate <= 0f)
        {
            return false;
        }

        // DropRate la trong so tuong doi, nen bang 60/30/10 hay 6/3/1 cho cung ket qua.
        float roll = Random.value * totalRate;
        for (int i = 0; i < equipments.Count; i++)
        {
            EquipmentDropEntry entry = equipments[i];
            if (entry == null || !entry.CanRoll(LevelRequired))
            {
                continue;
            }

            roll -= entry.DropRate;
            if (roll <= 0f)
            {
                equipment = entry.Equipment;
                return true;
            }
        }

        return TryGetLastValidEquipment(out equipment);
    }

    private float GetTotalRate()
    {
        float totalRate = 0f;
        if (equipments == null)
        {
            return totalRate;
        }

        for (int i = 0; i < equipments.Count; i++)
        {
            EquipmentDropEntry entry = equipments[i];
            if (entry != null && entry.CanRoll(LevelRequired))
            {
                totalRate += entry.DropRate;
            }
        }

        return totalRate;
    }

    private bool TryGetLastValidEquipment(out EquipmentDataSO equipment)
    {
        for (int i = equipments.Count - 1; i >= 0; i--)
        {
            EquipmentDropEntry entry = equipments[i];
            if (entry != null && entry.CanRoll(LevelRequired))
            {
                equipment = entry.Equipment;
                return true;
            }
        }

        equipment = null;
        return false;
    }
}
