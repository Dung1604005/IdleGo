using UnityEngine;

public static class CombatFormula
{
    public const float ARMOR_CONSTANT = 100f;

    public static float CalculateArmorDamageReduction(float armor)
    {
        float normalizedArmor = Mathf.Max(0f, armor);

        // Diminishing return giup Armor tang dan hieu qua nhung khong bao gio giam 100% damage.
        return normalizedArmor / (normalizedArmor + ARMOR_CONSTANT);
    }
}
