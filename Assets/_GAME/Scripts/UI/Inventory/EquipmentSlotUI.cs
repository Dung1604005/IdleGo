using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : GameUnit
{
    [SerializeField] private Image rarityImage;

    [SerializeField] private Image iconImage;

    [SerializeField] private EquipmentType equipmentType;

     public override void OnSpawn()
    {
        Clear();
    }

    public override void OnDespawn()
    {
        Clear();
    }

    public void SetData()
    {
        Equipment equipment = GetEquipment();
        if (equipment?.Data == null)
        {
            Clear();
            return;
        }
        ChangeItem(equipment.RarityType, equipment.Data.Icon);
    }

    public void OnButtonClick()
    {
        Equipment equipment = GetEquipment();
        if (equipment == null)
        {
            return;
        }

        DataManager.Ins.SetSelectedEquipment(
            equipment,
            EquipmentSelectionSource.EQUIPMENT_SLOT);
        UIManager.Ins.OpenUI<CanvasItemInfomationUI>();
    }


    public void ChangeItem(RarityType rarityType, Sprite iconSprite)
    {
        if (rarityImage == null || iconImage == null)
        {
            Debug.LogError("EquipmentUI is missing its rarity or icon Image reference.");
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

    }

    private Equipment GetEquipment()
    {
        return DataManager.Ins.SelectedCharacter?.Equipment?.GetEquipment(equipmentType);
    }
}
