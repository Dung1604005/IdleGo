using System.Collections.Generic;
using UnityEngine;

public class InventoryEquipmentHandler
{
    private readonly InventoryStorage storage;
    private readonly PlayerManager playerManager;
    private readonly Inventory owner;
    public InventoryEquipmentHandler(
        InventoryStorage inventoryStorage,
        PlayerManager ownerPlayerManager,
        Inventory ownerInventory)
    {
        storage = inventoryStorage;
        playerManager = ownerPlayerManager;
        owner = ownerInventory;
    }
    public bool Equip(Player player, Item item)
    {
        if (!IsValidCharacter(player)
            || item is not Equipment equipment
            || !player.Equipment.CanEquip(equipment))
        {
            return false;
        }
        InventorySlot sourceSlot = storage.GetSlot(equipment);
        if (sourceSlot == null || sourceSlot.RemoveItem(1) != 1)
        {
            return false;
        }

        if (!player.Equipment.TryEquip(equipment, out Equipment replacedEquipment))
        {
            sourceSlot.AddItem(equipment, 1);
            return false;
        }

        // Item moi roi kho; item bi thay the dung chinh slot vua duoc giai phong.
        if (replacedEquipment != null)
        {
            sourceSlot.AddItem(replacedEquipment, 1);
            replacedEquipment.SetInventory(owner);
        }

        equipment.SetInventory(owner);
        return true;
    }

    public bool Unequip(Player player, Item item)
    {
        return item is Equipment equipment
            && TryUnequip(player, equipment);
    }

    public bool Unequip(Player player, EquipmentType equipmentType)
    {
        Equipment equipment = IsValidCharacter(player)
            ? player.Equipment.GetEquipment(equipmentType)
            : null;
        return TryUnequip(player, equipment);
    }

    public void UnequipBeforeRemoving(Item item)
    {
        if (item is Equipment equipment && equipment.IsEquipped)
        {
            equipment.EquippedBy?.Unequip(equipment);
        }
    }

    public void ImportCurrentEquipment(Inventory inventory)
    {
        if (playerManager == null)
        {
            return;
        }

        IReadOnlyList<Player> characters = playerManager.AllPlayers;
        for (int i = 0; i < characters.Count; i++)
        {
            IReadOnlyList<Equipment> equipments = characters[i].Equipment.EquippedItems;
            for (int equipmentIndex = 0; equipmentIndex < equipments.Count; equipmentIndex++)
            {
                equipments[equipmentIndex]?.SetInventory(inventory);
            }
        }
    }

    public void RestoreEquippedItems(
        PlayerRosterSaveData rosterSaveData,
        IReadOnlyList<PlayerEquipmentSaveData> legacyPlayerEquipments,
        IReadOnlyList<string> legacyEquipmentIds,
        ItemDatabaseSO itemDatabase,
        HashSet<string> loadedInstanceIds)
    {
        if (playerManager == null)
        {
            return;
        }

        if (PlayerRosterSaveMapper.HasData(rosterSaveData))
        {
            RestoreCharacterEquipments(rosterSaveData.characters, itemDatabase, loadedInstanceIds);
            return;
        }

        RestoreLegacyEquipment(legacyPlayerEquipments, legacyEquipmentIds);
    }

    public void UnequipAllCharacters()
    {
        if (playerManager == null)
        {
            return;
        }

        IReadOnlyList<Player> characters = playerManager.AllPlayers;
        for (int i = 0; i < characters.Count; i++)
        {
            IReadOnlyList<Equipment> equipments = characters[i].Equipment.EquippedItems;
            for (int equipmentIndex = 0; equipmentIndex < equipments.Count; equipmentIndex++)
            {
                equipments[equipmentIndex]?.SetInventory(null);
            }
            characters[i]?.Equipment.UnequipAll();
        }
    }

    private bool TryUnequip(Player player, Equipment equipment)
    {
        if (!IsValidCharacter(player)
            || equipment == null
            || !ReferenceEquals(
                player.Equipment.GetEquipment(equipment.EquipmentType), equipment))
        {
            return false;
        }

        InventorySlot emptySlot = storage.GetFirstEmptySlot();
        if (emptySlot == null || !player.Equipment.Unequip(equipment))
        {
            return false;
        }

        if (emptySlot.AddItem(equipment, 1) == 1)
        {
            equipment.SetInventory(owner);
            return true;
        }

        player.Equipment.Equip(equipment);
        return false;
    }

    private void RestoreCharacterEquipments(
        IReadOnlyList<PlayerCharacterSaveData> characterSaves,
        ItemDatabaseSO itemDatabase,
        HashSet<string> loadedInstanceIds)
    {
        for (int i = 0; i < characterSaves.Count; i++)
        {
            PlayerCharacterSaveData characterSave = characterSaves[i];
            Player player = characterSave != null
                ? playerManager.GetCharacter(characterSave.characterId)
                : null;
            if (player?.Equipment == null || !player.Equipment.IsInitialized)
            {
                continue;
            }

            if (characterSave.equippedEquipments != null
                && characterSave.equippedEquipments.Count > 0)
            {
                RestoreEquipmentData(
                    player, characterSave.equippedEquipments,
                    itemDatabase, loadedInstanceIds);
            }
            else
            {
                RestoreLegacyIds(player, characterSave.equippedItemInstanceIds);
            }
        }
    }

    private void RestoreEquipmentData(
        Player player,
        IReadOnlyList<EquippedEquipmentSaveData> equipmentSaves,
        ItemDatabaseSO itemDatabase,
        HashSet<string> loadedInstanceIds)
    {
        for (int i = 0; i < equipmentSaves.Count; i++)
        {
            Equipment equipment = EquipmentInstanceSaveMapper.Restore(
                equipmentSaves[i], itemDatabase, loadedInstanceIds, owner);
            if (equipment != null && !player.Equipment.Equip(equipment))
            {
                equipment.SetInventory(null);
                Debug.LogWarning($"Cannot restore equipment for {player.name}.");
            }
        }
    }

    private void RestoreLegacyEquipment(
        IReadOnlyList<PlayerEquipmentSaveData> playerEquipmentSaves,
        IReadOnlyList<string> singlePlayerIds)
    {
        if (playerEquipmentSaves != null && playerEquipmentSaves.Count > 0)
        {
            for (int i = 0; i < playerEquipmentSaves.Count; i++)
            {
                PlayerEquipmentSaveData save = playerEquipmentSaves[i];
                RestoreLegacyIds(
                    playerManager.GetPlayer(save.playerIndex),
                    save.equippedItemInstanceIds);
            }
            return;
        }

        RestoreLegacyIds(playerManager.GetPlayer(0), singlePlayerIds);
    }

    private void RestoreLegacyIds(Player player, IReadOnlyList<string> instanceIds)
    {
        if (!IsValidCharacter(player) || instanceIds == null)
        {
            return;
        }

        for (int i = 0; i < instanceIds.Count; i++)
        {
            Item item = storage.GetItemByInstanceId(instanceIds[i]);
            InventorySlot slot = storage.GetSlot(item);
            if (item is Equipment equipment
                && player.Equipment.Equip(equipment))
            {
                // Save cu de equipment trong kho; load mot lan se tach no ra khoi slot.
                slot?.RemoveItem(1);
            }
        }
    }

    private bool IsValidCharacter(Player player)
    {
        return playerManager != null
            && playerManager.ContainsCharacter(player)
            && playerManager.IsCharacterUnlocked(player)
            && player.Equipment != null
            && player.Equipment.IsInitialized;
    }
}
