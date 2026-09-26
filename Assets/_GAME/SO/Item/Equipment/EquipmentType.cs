public enum EquipmentType
{
    MAIN_WEAPON = 0,
    OFF_HAND_WEAPON = 1,
    BODY_ARMOR = 2,
    HEAD_ARMOR = 3,
    LEG_ARMOR = 4,
    SHOES = 5,
    RING = 6,
    NECKLACE = 7
}

public static class EquipmentTypeUtility
{
    public const int EquipmentTypeCount = (int)EquipmentType.NECKLACE + 1;

    public static bool IsValid(EquipmentType equipmentType)
    {
        int equipmentIndex = (int)equipmentType;
        return equipmentIndex >= 0 && equipmentIndex < EquipmentTypeCount;
    }
}
