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
    [SerializeField] private Button equipButton;
    [SerializeField] private Button unequipButton;

    private Equipment displayedEquipment;
    private Color defaultLevelRequirementColor = Color.white;
    private Color defaultCharacterRequirementColor = Color.white;
    private bool hasCachedRequirementColors;

    public override void OnInit()
    {
        OnInit(null);
    }

    public void OnInit(Equipment equipment)
    {
        CacheRequirementColors();
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
        RefreshActionButtons();
    }

    public override void OnDespawn()
    {
        displayedEquipment = null;
        ClearView();
    }

    public void OnButtonEquip()
    {
        if (DataManager.Ins.EquipSelectedEquipment())
        {
            UIManager.Ins.CloseUIDirectly<CanvasItemInfomationUI>();
        }
    }

    public void OnButtonUnequip()
    {
        if (DataManager.Ins.UnequipSelectedEquipment())
        {
            UIManager.Ins.CloseUIDirectly<CanvasItemInfomationUI>();
        }
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
        Player selectedPlayer = DataManager.Ins.SelectedCharacter;
        SetLevelRequirement(data.LevelRequired, selectedPlayer);
        SetCharacterRequirement(data.CharacterRequirement, selectedPlayer);
    }

    private void SetLevelRequirement(int requiredLevel, Player selectedPlayer)
    {
        bool meetsRequirement = selectedPlayer != null
            && selectedPlayer.Stats.CurrentLevel >= requiredLevel;
        SetColoredText(
            levelRequirementText,
            $"Level Required: {requiredLevel}",
            meetsRequirement ? defaultLevelRequirementColor : Color.red);
    }

    private void SetCharacterRequirement(
        CharacterRequirementType requirement,
        Player selectedPlayer)
    {
        string requirementName = EquipmentInformationText.GetRequirementName(requirement);
        bool meetsRequirement = selectedPlayer != null
            && CharacaterClassTypeUtility.Matches(
                requirement,
                selectedPlayer.CharacaterClassType);
        SetColoredText(
            characterRequirementText,
            string.IsNullOrEmpty(requirementName)
            ? string.Empty
            : $"Class Required: {requirementName}",
            meetsRequirement ? defaultCharacterRequirementColor : Color.red);
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

    private void RefreshActionButtons()
    {
        EquipmentSelectionSource source = DataManager.Ins.SelectedEquipmentSource;
        SetButtonActive(equipButton,
            source == EquipmentSelectionSource.INVENTORY_SLOT);
        SetButtonActive(unequipButton,
            source == EquipmentSelectionSource.EQUIPMENT_SLOT);
    }

    private void CacheRequirementColors()
    {
        if (hasCachedRequirementColors)
        {
            return;
        }

        if (levelRequirementText != null)
        {
            defaultLevelRequirementColor = levelRequirementText.color;
        }
        if (characterRequirementText != null)
        {
            defaultCharacterRequirementColor = characterRequirementText.color;
        }
        hasCachedRequirementColors = true;
    }

    private static void SetButtonActive(Button button, bool isActive)
    {
        if (button != null)
        {
            button.gameObject.SetActive(isActive);
        }
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
        SetButtonActive(equipButton, false);
        SetButtonActive(unequipButton, false);
    }
}
