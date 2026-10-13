using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StatProgressLineUI : GameUnit
{
    [SerializeField] private Image statIcon;
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI currentValueText;
    [SerializeField] private TextMeshProUGUI increaseValueText;
    [SerializeField] private Button increaseButton;

    private string characterId;
    private StatType statType;

    public override void OnSpawn()
    {
        Clear();
    }

    public override void OnDespawn()
    {
        Clear();
    }

    public void SetData(
        string targetCharacterId,
        StatType targetStatType)
    {
        characterId = targetCharacterId;
        statType = targetStatType;
        Sprite icon = DataManager.Ins.GetStatIcon(statType);
        SetIcon(icon);
        SetText(statNameText, HeroStatusText.GetStatName(statType));
        SetText(currentValueText, HeroStatusText.FormatStat(
            statType,
            DataManager.Ins.GetCharacterStat(characterId, statType)));
        RefreshUpgradeVisual();
    }

    public void OnButtonIncrease()
    {
        // DataManager se update data, save, roi goi lai view qua Inventory flow.
        DataManager.Ins.UpgradeCharacterStat(characterId, statType);
    }

    private void RefreshUpgradeVisual()
    {
        bool hasRule = DataManager.Ins.TryGetCharacterStatRule(
            characterId,
            statType,
            out CharacterStatUpgradeRule rule);
        int points = DataManager.Ins.GetCharacterAvailableStatPoints(characterId);
        string increase = hasRule
            ? $"+{HeroStatusText.FormatStat(statType, rule.ValuePerUpgrade)}  •  {rule.StatPointCost} SP"
            : "No upgrade";
        SetText(increaseValueText, increase);
        if (increaseButton != null)
        {
            increaseButton.interactable = hasRule
                && points >= rule.StatPointCost;
        }
    }

    private void Clear()
    {
        characterId = string.Empty;
        SetIcon(null);
        SetText(statNameText, string.Empty);
        SetText(currentValueText, string.Empty);
        SetText(increaseValueText, string.Empty);
        if (increaseButton != null)
        {
            increaseButton.interactable = false;
        }
    }

    private void SetIcon(Sprite icon)
    {
        if (statIcon != null)
        {
            statIcon.sprite = icon;
            statIcon.enabled = icon != null;
        }
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
