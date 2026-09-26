using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ItemData", menuName = "IdleGo/Item/Item Data")]
public class ItemSO : ScriptableObject
{
    [SerializeField] private string itemId;
    [FormerlySerializedAs("nameEquipment")]
    [SerializeField] protected String nameItem;
    [SerializeField] protected RarityType rarityType;
    [SerializeField] protected ItemType itemType;
    [SerializeField] protected Sprite icon;

    [SerializeField, Min(1)] protected int maxQuantity = 1;

    public string ItemId => itemId;
    public string NameItem => nameItem;
    public RarityType RarityType => rarityType;
    public ItemType ItemType => itemType;
    public Sprite Icon => icon;
    public int MaxQuantity => Mathf.Max(1, maxQuantity);

    protected virtual void OnValidate()
    {
        // ID khong phu thuoc vao ten asset, vi vay doi ten item se khong lam hong save cu.
        if (string.IsNullOrWhiteSpace(itemId))
        {
            itemId = Guid.NewGuid().ToString("N");
        }

        maxQuantity = Mathf.Max(1, maxQuantity);
    }
}


public enum ItemType
{
    EQUIPMENT = 0,

    ENCHANT_MATERIAL = 1,

    MISC = 2
}
