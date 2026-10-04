
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : GameUnit
{
    [SerializeField] private Image rarityImage;

    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

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
    }

    public void OnButtonClick()
    {
        Inventory inventory = DataManager.Ins.InventoryData;
        if (inventory == null
            || SlotIndex < 0
            || SlotIndex >= inventory.Slots.Count
            || inventory.Slots[SlotIndex].Item is not Equipment equipment)
        {
            return;
        }

        // Slot chi chon data; Canvas se tu doc equipment qua DataManager.
        DataManager.Ins.SetSelectedEquipment(equipment);
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
    }

    private void SetAmount(int amount)
    {
        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : string.Empty;
        }
    }

    

    


}
