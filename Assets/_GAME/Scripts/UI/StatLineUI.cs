using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatLineUI : GameUnit
{
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private Image imageRankStat;
    [SerializeField] private TextMeshProUGUI textRankStat;

    private Color defaultRankImageColor;
    private Color defaultRankTextColor;
    private string defaultRankText;
    private bool hasCachedDefaults;

    public override void OnSpawn()
    {
        CacheDefaultRankVisual();
        ResetRankVisual();
        SetStatText(string.Empty);
    }

    public void OnInit(StatValue stat)
    {
        ResetRankVisual();
        SetStatText(EquipmentInformationText.FormatStat(stat));
    }

    public void OnInitEnhancement(StatValue stat, RarityType rarityType)
    {
        OnInitEnhancement(stat, (RarityType?)rarityType);
    }

    public void OnInitEnhancement(StatValue stat)
    {
        RarityType? rarityType = stat is BuffStatValue buffStat
            ? buffStat.SourceRarity
            : null;
        OnInitEnhancement(stat, rarityType);
    }

    private void OnInitEnhancement(StatValue stat, RarityType? rarityType)
    {
        ResetRankVisual();
        SetStatText(stat == null
            ? "Empty"
            : EquipmentInformationText.FormatStat(stat));

        if (stat != null && rarityType.HasValue)
        {
            SetEnhancementRank(rarityType.Value);
        }
    }

    public override void OnDespawn()
    {
        ResetRankVisual();
        SetStatText(string.Empty);
    }

    private void CacheDefaultRankVisual()
    {
        if (hasCachedDefaults)
        {
            return;
        }

        defaultRankImageColor = imageRankStat != null
            ? imageRankStat.color
            : Color.white;
        defaultRankTextColor = textRankStat != null
            ? textRankStat.color
            : Color.white;
        defaultRankText = textRankStat != null ? textRankStat.text : string.Empty;
        hasCachedDefaults = true;
    }

    private void SetEnhancementRank(RarityType rarityType)
    {
        Color rarityColor = DataManager.Ins.GetRarityColor(rarityType);
        if (imageRankStat != null)
        {
            imageRankStat.color = rarityColor;
        }

        if (textRankStat != null)
        {
            textRankStat.text = $"T{(int)rarityType + 1}";
            textRankStat.color = rarityColor;
        }
    }

    private void ResetRankVisual()
    {
        if (imageRankStat != null)
        {
            imageRankStat.color = defaultRankImageColor;
        }

        if (textRankStat != null)
        {
            textRankStat.text = defaultRankText;
            textRankStat.color = defaultRankTextColor;
        }
    }

    private void SetStatText(string value)
    {
        if (statText != null)
        {
            statText.text = value;
        }
    }
}
