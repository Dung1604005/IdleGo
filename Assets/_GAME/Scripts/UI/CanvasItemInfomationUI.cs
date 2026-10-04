using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasItemInfomationUI : UICanvas
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text mainStatsText;
    [SerializeField] private TMP_Text subStatsText;
    [SerializeField] private TMP_Text enhancementSlotsText;
    [SerializeField] private TMP_Text levelRequirementText;
    [SerializeField] private TMP_Text characterRequirementText;

    private Equipment displayedEquipment;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        displayedEquipment = DataManager.Ins.SelectedEquipment;
        RefreshInformation();
    }

    public void OnDespawn()
    {
        DataManager.Ins.ClearSelectedEquipment(displayedEquipment);
        displayedEquipment = null;
    }

    public void OnButtonClose()
    {
        CloseDirectly();
    }

    public override void CloseDirectly()
    {
        OnDespawn();
        base.CloseDirectly();
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
        SetText(subStatsText,
            EquipmentInformationText.BuildSubStats(displayedEquipment));
        SetText(enhancementSlotsText,
            EquipmentInformationText.BuildEnhancementSlots(displayedEquipment));
        SetText(levelRequirementText, $"Yêu cầu cấp: {data.LevelRequired}");
        SetCharacterRequirement(data.CharacterRequirement);
    }

    private void SetCharacterRequirement(CharacterRequirementType requirement)
    {
        string characterType = EquipmentInformationText.GetRequirementName(requirement);
        SetText(characterRequirementText, string.IsNullOrEmpty(characterType)
            ? string.Empty
            : $"Yêu cầu nhân vật: {characterType}");
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

    private static void SetColoredText(TMP_Text target, string value, Color color)
    {
        if (target == null)
        {
            return;
        }

        target.text = value;
        target.color = color;
        target.gameObject.SetActive(!string.IsNullOrEmpty(value));
    }

    private static void SetText(TMP_Text target, string value)
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
        SetText(subStatsText, string.Empty);
        SetText(enhancementSlotsText, string.Empty);
        SetText(levelRequirementText, string.Empty);
        SetText(characterRequirementText, string.Empty);
    }
}
