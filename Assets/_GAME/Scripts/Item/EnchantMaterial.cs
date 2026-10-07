using System;

[Serializable]
public class EnchantMaterial : Item
{
    public EnchantMaterial()
    {
    }

    public EnchantMaterial(EnchantMaterialDataSO data) : base(data)
    {
    }

    public new EnchantMaterialDataSO Data => base.Data as EnchantMaterialDataSO;

    public bool TryRollStat(out StatValue rolledStat)
    {
        rolledStat = null;
        return Data != null && Data.TryRollStat(out rolledStat);
    }
}
