using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelItemInformation : PanelView
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI mainStatsText;
    [SerializeField] private TextMeshProUGUI levelRequirementText;
    [SerializeField] private TextMeshProUGUI characterRequirementText;
    [SerializeField] private EquipmentStatsContentUI statsContent;

    private Equipment displayedEquipment;

    public override void OnInit()
    {
        OnInit(null);
    }

    public void OnInit(Equipment equipment)
    {
        displayedEquipment = equipment;
        bool hasEquipment = displayedEquipment?.Data != null;
        gameObject.SetActive(hasEquipment);
        if (!hasEquipment)
        {
            ClearView();
            return;
        }

        // Layout chi tinh dung kich thuoc khi panel dang active.
        statsContent?.OnInit();
        RefreshInformation();
    }

    public override void OnDespawn()
    {
        displayedEquipment = null;
        ClearView();
    }

    private void RefreshInformation()
    {
        EquipmentDataSO data = displayedEquipment?.Data;
        if (data == null)
        {
            ClearView();
            return;
        }

        Color rarityColor = DataManager.Ins.GetRarityColor(data.RarityType);
        SetIcon(data.Icon);
        SetColoredText(itemNameText, data.NameItem, rarityColor);
        SetColoredText(rarityText,
            EquipmentInformationText.GetRarityName(data.RarityType), rarityColor);
        SetText(mainStatsText,
            EquipmentInformationText.BuildMainStats(displayedEquipment));
        statsContent?.Refresh(displayedEquipment);
        SetText(levelRequirementText, $"Level Required: {data.LevelRequired}");
        SetCharacterRequirement(data.CharacterRequirement);
    }

    private void SetCharacterRequirement(CharacterRequirementType requirement)
    {
        string characterType = EquipmentInformationText.GetRequirementName(requirement);
        SetText(characterRequirementText, string.IsNullOrEmpty(characterType)
            ? string.Empty
            : $"Class Required: {characterType}");
    }

    private void SetIcon(Sprite icon)
    {
        if (itemIcon == null)
        {
            return;
        }

        itemIcon.sprite = icon;
        itemIcon.enabled = icon != null;
    }

    private static void SetColoredText(
        TextMeshProUGUI target,
        string value,
        Color color)
    {
        if (target == null)
        {
            return;
        }

        target.text = value;
        target.color = color;
        target.gameObject.SetActive(!string.IsNullOrEmpty(value));
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target == null)
        {
            return;
        }

        target.text = value;
        target.gameObject.SetActive(!string.IsNullOrEmpty(value));
    }

    private void ClearView()
    {
        SetIcon(null);
        SetText(itemNameText, string.Empty);
        SetText(rarityText, string.Empty);
        SetText(mainStatsText, string.Empty);
        SetText(levelRequirementText, string.Empty);
        SetText(characterRequirementText, string.Empty);
        statsContent?.OnDespawn();
    }
}
