using System;
using System.Collections.Generic;

public class EquipmentStatHandler
{
    private readonly Character character;
    private readonly EquipmentData equipmentData;
    private readonly CharacterEquipment equipmentOwner;
    private readonly List<StatModifier> modifierBuffer = new List<StatModifier>();
    private readonly List<IStatModifierSource> appliedModifierSources =
        new List<IStatModifierSource>();
    private readonly List<IStatModifierSource> currentModifierSources =
        new List<IStatModifierSource>();

    public EquipmentStatHandler(
        Character owner,
        EquipmentData data,
        CharacterEquipment ownerEquipment)
    {
        character = owner;
        equipmentData = data;
        equipmentOwner = ownerEquipment;
    }

    public void Apply()
    {
        if (character == null || equipmentData == null)
        {
            return;
        }

        modifierBuffer.Clear();
        currentModifierSources.Clear();

        IReadOnlyList<Equipment> equipments = equipmentData.Equipments;
        for (int i = 0; i < equipments.Count; i++)
        {
            Equipment equipment = equipments[i];
            if (equipment == null || !ReferenceEquals(equipment.EquippedBy, equipmentOwner))
            {
                continue;
            }

            equipment.CollectStatModifiers(modifierBuffer);
            currentModifierSources.Add(equipment);
        }

        // Chỉ thay modifier từ equipment; modifier của buff, passive và pet vẫn được giữ nguyên.
        character.Stats.ReplaceModifiersFromSources(appliedModifierSources, modifierBuffer);
        ReplaceAppliedSources();
    }

    public void Clear()
    {
        if (character != null)
        {
            character.Stats.ReplaceModifiersFromSources(
                appliedModifierSources,
                Array.Empty<StatModifier>()
            );
        }

        modifierBuffer.Clear();
        currentModifierSources.Clear();
        appliedModifierSources.Clear();
    }

    private void ReplaceAppliedSources()
    {
        appliedModifierSources.Clear();
        for (int i = 0; i < currentModifierSources.Count; i++)
        {
            appliedModifierSources.Add(currentModifierSources[i]);
        }
    }
}
