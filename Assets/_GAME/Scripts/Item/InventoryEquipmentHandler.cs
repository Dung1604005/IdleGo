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
        return IsValidTeamPlayer(player)
            && item is Equipment equipment
            && storage.GetSlot(item) != null
            && player.Equipment.TryEquip(equipment, out _);
    }

    public bool Unequip(Player player, Item item)
    {
        return IsValidTeamPlayer(player)
            && item is Equipment equipment
            && storage.GetSlot(item) != null
            && player.Equipment.Unequip(equipment);
    }

    public bool Unequip(Player player, EquipmentType equipmentType)
    {
        return IsValidTeamPlayer(player)
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

        for (int playerIndex = 0; playerIndex < playerManager.TeamCount; playerIndex++)
        {
            Player player = playerManager.GetPlayer(playerIndex);
            if (player != null)
            {
                ImportPlayerEquipment(player, owner);
            }
        }
    }

    public void RestoreEquippedItems(
        IReadOnlyList<PlayerEquipmentSaveData> savedPlayerEquipments,
        IReadOnlyList<string> legacyEquippedItemInstanceIds)
    {
        UnequipAllPlayers();
        if (playerManager == null)
        {
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

    private void UnequipAllPlayers()
    {
        if (playerManager == null)
        {
            return;
        }

        for (int i = 0; i < playerManager.TeamCount; i++)
        {
            Player player = playerManager.GetPlayer(i);
            player?.Equipment.UnequipAll();
        }
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
        if (!IsValidTeamPlayer(player) || instanceIds == null)
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

    private bool IsValidTeamPlayer(Player player)
    {
        return playerManager != null
            && playerManager.ContainsPlayer(player)
            && player.Equipment != null
            && player.Equipment.IsInitialized;
    }
}
