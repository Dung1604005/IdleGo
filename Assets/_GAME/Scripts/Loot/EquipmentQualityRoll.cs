using UnityEngine;

public static class EquipmentQualityRoll
{
    public const float MaxRoll = 1.1f;

    public static float Roll(RarityType rarityType)
    {
        return Random.Range(GetMinRoll(rarityType), MaxRoll);
    }

    public static float GetMinRoll(RarityType rarityType)
    {
        return rarityType switch
        {
            RarityType.COMMON => 0.90f,
            RarityType.UNCOMMON => 0.91f,
            RarityType.RARE => 0.92f,
            RarityType.LEGEND => 0.93f,
            RarityType.DEMON => 0.94f,
            RarityType.ARCANA => 0.95f,
            RarityType.BEYOND => 0.96f,
            RarityType.CELESTIAL => 0.97f,
            _ => 1f
        };
    }
}
