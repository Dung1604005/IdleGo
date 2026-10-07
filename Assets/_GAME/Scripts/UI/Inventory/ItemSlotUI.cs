
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : GameUnit
{
    [SerializeField] private Image rarityImage;

    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    [SerializeField] private Image iconCantEquip;

    public int SlotIndex { get; private set; }

    public override void OnSpawn()
    {
        SlotIndex = -1;
        Clear();
    }

    public override void OnDespawn()
    {
        SlotIndex = -1;
        Clear();
    }

    public void OnInit(int slotIndex)
    {
        SlotIndex = slotIndex;
    }

    public void SetData(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty || slot.Item.Data == null)
        {
            Clear();
            return;
        }
        

        ChangeItem(slot.Item.RarityType, slot.Item.Data.Icon);
        SetAmount(slot.Amount);
        RefreshCantEquipIcon(slot.Item);
    }

    public void OnButtonClick()
    {
        Inventory inventory = DataManager.Ins.InventoryData;
        if (inventory == null
            || SlotIndex < 0
            || SlotIndex >= inventory.Slots.Count
            || inventory.Slots[SlotIndex].Item == null)
        {
            return;
        }

        Item selectedItem = inventory.Slots[SlotIndex].Item;
        if (selectedItem is Equipment equipment)
        {
            DataManager.Ins.SetSelectedEquipment(
                equipment,
                EquipmentSelectionSource.INVENTORY_SLOT);
        }
        else if (selectedItem is EnchantMaterial)
        {
            DataManager.Ins.SetSelectedItem(selectedItem);
        }
        else
        {
            return;
        }

        UIManager.Ins.OpenUI<CanvasItemInfomationUI>();
    }


    public void ChangeItem(RarityType rarityType, Sprite iconSprite)
    {
        if (rarityImage == null || iconImage == null)
        {
            Debug.LogError("ItemSlotUI is missing its rarity or icon Image reference.");
            return;
        }

        rarityImage.sprite = DataManager.Ins.GetRarityBGSprite(rarityType);

        iconImage.sprite = iconSprite;
        rarityImage.enabled = rarityImage.sprite != null;
        iconImage.enabled = iconImage.sprite != null;
    }

    public void Clear()
    {
        if (rarityImage != null)
        {
            rarityImage.sprite = null;
            rarityImage.enabled = false;
        }

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (amountText != null)
        {
            amountText.text = string.Empty;
        }

        SetCantEquipIcon(false);
    }

    private void SetAmount(int amount)
    {
        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : string.Empty;
        }
    }

    private void RefreshCantEquipIcon(Item item)
    {
        if (item is not Equipment equipment)
        {
            SetCantEquipIcon(false);
            return;
        }

        CharacterEquipment characterEquipment =
            DataManager.Ins.SelectedCharacter?.Equipment;
        bool canEquip = characterEquipment != null
            && characterEquipment.CanEquip(equipment);
        SetCantEquipIcon(!canEquip);
    }

    private void SetCantEquipIcon(bool isActive)
    {
        if (iconCantEquip != null)
        {
            iconCantEquip.gameObject.SetActive(isActive);
        }
    }

    

    


}
