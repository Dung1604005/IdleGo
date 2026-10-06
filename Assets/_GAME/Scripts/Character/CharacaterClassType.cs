public enum CharacaterClassType
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

public static class CharacaterClassTypeUtility
{
    public static bool Matches(
        CharacterRequirementType requirement,
        CharacaterClassType characaterClassType)
    {
        // So sanh ro tung dieu kien de enum co duoc mo rong sau nay cung khong phu thuoc vao phep tru chi so.
        switch (requirement)
        {
            case CharacterRequirementType.ALL:
                return true;
            case CharacterRequirementType.MELEE:
                return characaterClassType == CharacaterClassType.MELEE;
            case CharacterRequirementType.RANGER:
                return characaterClassType == CharacaterClassType.RANGER;
            case CharacterRequirementType.MAGE:
                return characaterClassType == CharacaterClassType.MAGE;
            default:
                return false;
        }
    }
}
