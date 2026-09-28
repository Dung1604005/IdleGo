public enum CharacterType
{
    MELEE = 0,
    RANGER = 1,
    MAGE = 2
}

public enum CharacterRequirementType
{
    ALL = 0,
    MELEE = 1,
    RANGER = 2,
    MAGE = 3
}

public static class CharacterTypeUtility
{
    public static bool Matches(
        CharacterRequirementType requirement,
        CharacterType characterType)
    {
        // So sanh ro tung dieu kien de enum co duoc mo rong sau nay cung khong phu thuoc vao phep tru chi so.
        switch (requirement)
        {
            case CharacterRequirementType.ALL:
                return true;
            case CharacterRequirementType.MELEE:
                return characterType == CharacterType.MELEE;
            case CharacterRequirementType.RANGER:
                return characterType == CharacterType.RANGER;
            case CharacterRequirementType.MAGE:
                return characterType == CharacterType.MAGE;
            default:
                return false;
        }
    }
}
