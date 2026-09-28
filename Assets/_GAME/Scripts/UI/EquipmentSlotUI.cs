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

    public void SetData(EquipmentDataSO data)
    {
        if (data == null )
        {
            Clear();
            return;
        }
        ChangeItem(data.RarityType, data.Icon);
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
}
