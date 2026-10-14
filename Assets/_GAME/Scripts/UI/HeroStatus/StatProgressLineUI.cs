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
    [SerializeField] private StatValueFeedback valueFeedback = new StatValueFeedback();

    private string characterId;
    private StatType statType;
    private int lastUpgradeCount;
    private bool hasData;

    public override void OnSpawn()
    {
        valueFeedback.OnInit(currentValueText);
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
        bool sameStat = hasData && characterId == targetCharacterId && statType == targetStatType;
        if (!sameStat) valueFeedback.OnDespawn();
        characterId = targetCharacterId;
        statType = targetStatType;
        int upgradeCount = DataManager.Ins.GetCharacterStatUpgradeCount(characterId, statType);
        Sprite icon = DataManager.Ins.GetStatIcon(statType);
        SetIcon(icon);
        SetText(statNameText, HeroStatusText.GetStatName(statType));
        valueFeedback.SetValue(statType, DataManager.Ins.GetCharacterStat(characterId, statType),
            sameStat && upgradeCount > lastUpgradeCount);
        lastUpgradeCount = upgradeCount;
        hasData = true;
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
            ? $"+{HeroStatusText.FormatStat(statType, rule.ValuePerUpgrade)} / {rule.StatPointCost} SP"
            : "";
        SetText(increaseValueText, increase);
        if (increaseButton != null)
        {
            // Stat khong co rule chi de xem va khong duoc hien thao tac tang.
            increaseButton.gameObject.SetActive(hasRule);
            increaseButton.interactable = hasRule && points >= rule.StatPointCost;
        }
    }

    private void Clear()
    {
        valueFeedback.OnDespawn();
        hasData = false;
        lastUpgradeCount = 0;
        characterId = string.Empty;
        SetIcon(null);
        SetText(statNameText, string.Empty);
        SetText(currentValueText, string.Empty);
        SetText(increaseValueText, string.Empty);
        if (increaseButton != null)
        {
            increaseButton.gameObject.SetActive(false);
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
