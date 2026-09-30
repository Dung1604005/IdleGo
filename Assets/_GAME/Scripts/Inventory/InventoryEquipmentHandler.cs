using System.Collections.Generic;
using UnityEngine;

public class InventoryEquipmentHandler
{
    private readonly InventoryStorage storage;
    private readonly PlayerManager playerManager;

    public InventoryEquipmentHandler(
        InventoryStorage inventoryStorage,
        PlayerManager ownerPlayerManager)
    {
        storage = inventoryStorage;
        playerManager = ownerPlayerManager;
    }

    public bool Equip(Player player, Item item)
    {
        return IsValidCharacter(player)
            && item is Equipment equipment
            && storage.GetSlot(item) != null
            && player.Equipment.TryEquip(equipment, out _);
    }

    public bool Unequip(Player player, Item item)
    {
        return IsValidCharacter(player)
            && item is Equipment equipment
            && storage.GetSlot(item) != null
            && player.Equipment.Unequip(equipment);
    }

    public bool Unequip(Player player, EquipmentType equipmentType)
    {
        return IsValidCharacter(player)
            && player.Equipment.Unequip(equipmentType) != null;
    }

    public void UnequipBeforeRemoving(Item item)
    {
        if (item is not Equipment equipment || !equipment.IsEquipped)
        {
            return;
        }

        // Equipment tu biet CharacterEquipment dang so huu nen remove khong can doan Player nao.
        equipment.EquippedBy?.Unequip(equipment);
    }

    public void ImportCurrentEquipment(Inventory owner)
    {
        if (playerManager == null)
        {
            return;
        }

        IReadOnlyList<Player> characters = playerManager.AllPlayers;
        for (int playerIndex = 0; playerIndex < characters.Count; playerIndex++)
        {
            Player player = characters[playerIndex];
            if (playerManager.IsCharacterUnlocked(player))
            {
                ImportPlayerEquipment(player, owner);
            }
        }
    }

    public void RestoreEquippedItems(
        PlayerRosterSaveData rosterSaveData,
        IReadOnlyList<PlayerEquipmentSaveData> savedPlayerEquipments,
        IReadOnlyList<string> legacyEquippedItemInstanceIds)
    {
        if (playerManager == null)
        {
            return;
        }

        if (PlayerRosterSaveMapper.HasData(rosterSaveData))
        {
            for (int i = 0; i < rosterSaveData.characters.Count; i++)
            {
                RestoreCharacterEquipment(rosterSaveData.characters[i]);
            }
            return;
        }

        if (savedPlayerEquipments != null && savedPlayerEquipments.Count > 0)
        {
            for (int i = 0; i < savedPlayerEquipments.Count; i++)
            {
                RestorePlayerEquipment(savedPlayerEquipments[i]);
            }
            return;
        }

        // Save cu chi co mot Player: gan danh sach cu cho Player o team index 0.
        RestoreEquipmentIds(playerManager.GetPlayer(0), legacyEquippedItemInstanceIds);
    }

    private void ImportPlayerEquipment(Player player, Inventory owner)
    {
        List<Equipment> currentEquipment = new List<Equipment>(player.Equipment.EquippedItems);
        for (int i = 0; i < currentEquipment.Count; i++)
        {
            Equipment equipment = currentEquipment[i];
            if (equipment == null || storage.GetSlot(equipment) != null)
            {
                continue;
            }

            InventorySlot emptySlot = storage.GetFirstEmptySlot();
            if (emptySlot == null)
            {
                Debug.LogWarning("Team inventory does not have room for starting equipment.");
                return;
            }

            emptySlot.AddItem(equipment, 1);
            equipment.SetInventory(owner);
        }
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
            Player player = characters[i];
            player?.Equipment.UnequipAll();
        }
    }

    private void RestoreCharacterEquipment(PlayerCharacterSaveData characterSave)
    {
        if (characterSave == null)
        {
            return;
        }

        Player player = playerManager.GetCharacter(characterSave.characterId);
        RestoreEquipmentIds(player, characterSave.equippedItemInstanceIds);
    }

    private void RestorePlayerEquipment(PlayerEquipmentSaveData playerEquipmentSave)
    {
        if (playerEquipmentSave == null)
        {
            return;
        }

        Player player = playerManager.GetPlayer(playerEquipmentSave.playerIndex);
        RestoreEquipmentIds(player, playerEquipmentSave.equippedItemInstanceIds);
    }

    private void RestoreEquipmentIds(Player player, IReadOnlyList<string> instanceIds)
    {
        if (!IsValidCharacter(player) || instanceIds == null)
        {
            return;
        }

        for (int i = 0; i < instanceIds.Count; i++)
        {
            Item item = storage.GetItemByInstanceId(instanceIds[i]);
            if (item is Equipment equipment && !player.Equipment.Equip(equipment))
            {
                Debug.LogWarning(
                    $"Equipment {equipment.InstanceId} no longer meets Player {player.name} requirements."
                );
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
