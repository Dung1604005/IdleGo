using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
   [SerializeField] private List<MapDataSO> listMapData = new List<MapDataSO>();

   [SerializeField] private RarityBGSO rarityBGData;

   [SerializeField] private RarityColorSO rarityColorData;

   [SerializeField] private BuffStatVisualSO buffStatVisualSO;



   private Inventory inventoryData;
   private PlayerManager playerData;
   private ChestManager chestData;
   private string selectedCharacterId;
   private Equipment selectedEquipment;
   private EquipmentSelectionSource selectedEquipmentSource;

   public Inventory InventoryData => inventoryData;
   public PlayerManager PlayerData => playerData;
   public ChestManager ChestData => chestData;
   public Equipment SelectedEquipment => selectedEquipment;
   public EquipmentSelectionSource SelectedEquipmentSource => selectedEquipmentSource;
   public Player SelectedCharacter => GetCharacter(selectedCharacterId);

   public BuffStatVisualSO BuffStatVisualSO => buffStatVisualSO;

   public void SetInventoryData(Inventory inventory)
   {
       inventoryData = inventory;
   }

   public void ClearInventoryData(Inventory inventory)
   {
       if (ReferenceEquals(inventoryData, inventory))
       {
           inventoryData = null;
       }
   }

   public void SetPlayerData(PlayerManager playerManager)
   {
       playerData = playerManager;
   }

   public void ClearPlayerData(PlayerManager playerManager)
   {
       if (ReferenceEquals(playerData, playerManager))
       {
           playerData = null;
       }
   }

   public void SetChestData(ChestManager chestManager)
   {
       chestData = chestManager;
   }

   public void ClearChestData(ChestManager chestManager)
   {
       if (ReferenceEquals(chestData, chestManager))
       {
           chestData = null;
       }
   }

   public Player GetTeamPlayer(int teamIndex)
   {
       return playerData?.GetPlayer(teamIndex);
   }

   public Player GetCharacter(string characterId)
   {
       return playerData?.GetCharacter(characterId);
   }

   public string GetNextCharacterId(string characterId)
   {
       return playerData?.GetNextPlayerId(characterId);
   }

   public string GetPreviousCharacterId(string characterId)
   {
       return playerData?.GetPrevPlayerId(characterId);
   }

   public void SetSelectedCharacter(string characterId)
   {
       selectedCharacterId = characterId;
   }

   public void SetSelectedEquipment(
       Equipment equipment,
       EquipmentSelectionSource selectionSource)
   {
       selectedEquipment = equipment;
       selectedEquipmentSource = equipment != null
           ? selectionSource
           : EquipmentSelectionSource.NONE;
   }

   public void ClearSelectedEquipment()
   {
       selectedEquipment = null;
       selectedEquipmentSource = EquipmentSelectionSource.NONE;
   }

   public bool EquipSelectedEquipment()
   {
       Player player = SelectedCharacter;
       if (selectedEquipmentSource != EquipmentSelectionSource.INVENTORY_SLOT
           || player == null
           || selectedEquipment == null
           || playerData == null)
       {
           return false;
       }

       EquipmentSelectionSource previousSource = selectedEquipmentSource;
       // Doi source truoc de Inventory refresh cac view voi trang thai sau khi equip.
       selectedEquipmentSource = EquipmentSelectionSource.EQUIPMENT_SLOT;
       if (playerData.Equip(player, selectedEquipment))
       {
           return true;
       }

       selectedEquipmentSource = previousSource;
       return false;
   }

   public bool UnequipSelectedEquipment()
   {
       Player player = SelectedCharacter;
       if (selectedEquipmentSource != EquipmentSelectionSource.EQUIPMENT_SLOT
           || player == null
           || selectedEquipment == null
           || playerData == null)
       {
           return false;
       }

       EquipmentSelectionSource previousSource = selectedEquipmentSource;
       // Doi source truoc de Inventory refresh cac view voi trang thai sau khi unequip.
       selectedEquipmentSource = EquipmentSelectionSource.INVENTORY_SLOT;
       if (playerData.Unequip(player, selectedEquipment))
       {
           return true;
       }

       selectedEquipmentSource = previousSource;
       return false;
   }

   public MapDataSO GetMapData(MapType mapType)
    {
        
        return mapType == MapType.NONE ? null: listMapData[(int)mapType];
    }

    public Sprite GetRarityBGSprite(RarityType rarityType)
    {
        return rarityBGData.GetRarityBG(rarityType);
    }

    public Color GetRarityColor(RarityType rarityType)
    {
        return rarityColorData.GetRarityColor(rarityType);
    }
}
