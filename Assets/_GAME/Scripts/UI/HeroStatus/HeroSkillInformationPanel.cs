using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HeroSkillInformationPanel : PanelView
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI levelDetailsText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private ButtonUI upgradeButton;
    [SerializeField] private ButtonUI equipButton;
    [SerializeField] private ButtonUI unequipButton;

    private CanvasHeroStatus owner;
    private string characterId;
    private CombatSkill skill;
    private Color defaultUpgradeCostColor = Color.white;
    private bool hasCachedUpgradeCostColor;

    public void Refresh(
        CanvasHeroStatus canvas,
        string targetCharacterId,
        CombatSkill targetSkill)
    {
        owner = canvas;
        characterId = targetCharacterId;
        skill = targetSkill;
        bool hasSkill = skill != null;
        gameObject.SetActive(hasSkill);
        if (!hasSkill)
        {
            return;
        }

        int currentLevel = DataManager.Ins.GetCharacterSkillLevel(
            characterId, skill);
        SetIcon(skill.IconSkill);
        SetText(skillNameText, skill.DisplayName);
        SetText(descriptionText, skill.Description);
        SetText(currentLevelText,
            $"Current Level: {currentLevel} / {skill.MaxLevel}");
        SetText(levelDetailsText, BuildLevelDetails(skill, currentLevel));
        CacheUpgradeCostColor();
        RefreshButtons(currentLevel);
    }

    public void Hide()
    {
        owner = null;
        characterId = string.Empty;
        skill = null;
        gameObject.SetActive(false);
    }

    public void OnButtonUpgrade()
    {
        // Khong tu sua UI; data flow se refresh panel sau khi save.
        DataManager.Ins.UpgradeCharacterSkill(characterId, skill);
    }

    public void OnButtonEquip()
    {
        DataManager.Ins.EquipCharacterSkill(characterId, skill);
    }

    public void OnButtonUnequip()
    {
        DataManager.Ins.UnequipCharacterSkill(characterId, skill);
    }

    public void OnButtonClose()
    {
        owner?.CloseSkillInformation();
    }

    private void RefreshButtons(int currentLevel)
    {
        bool canUpgrade = DataManager.Ins.TryGetCharacterSkillUpgrade(
            characterId,
            skill,
            out int nextLevel,
            out int pointCost);
        int availablePoints = DataManager.Ins.GetCharacterAvailableStatPoints(
            characterId);
        RefreshUpgradeCost(currentLevel, canUpgrade, nextLevel, pointCost);
        upgradeButton.SetButton(true,
            canUpgrade && availablePoints >= pointCost);
        bool isEquipped = DataManager.Ins.IsCharacterSkillEquipped(
            characterId, skill);
        equipButton.SetButton( !isEquipped,
            currentLevel > 0
            && DataManager.Ins.CanEquipCharacterSkill(characterId, skill));
        unequipButton.SetButton(isEquipped, isEquipped);
    }

    private void RefreshUpgradeCost(
        int currentSkillLevel,
        bool canUpgrade,
        int nextLevel,
        int pointCost)
    {
        int characterLevel = Mathf.RoundToInt(DataManager.Ins.GetCharacterStat(
            characterId, StatType.LEVEL));
        bool isMaxLevel = currentSkillLevel >= skill.MaxLevel;
        if (!isMaxLevel && !skill.IsUnlocked(characterLevel))
        {
            SetColoredText(upgradeCostText,
                $"Level Require: {skill.LevelRequire}", Color.red);
            return;
        }

        SetColoredText(upgradeCostText,
            canUpgrade
                ? $"Upgrade Lv.{nextLevel}: {pointCost} Stat Points"
                : "Max Level",
            defaultUpgradeCostColor);
    }

    private void CacheUpgradeCostColor()
    {
        if (hasCachedUpgradeCostColor)
        {
            return;
        }

        if (upgradeCostText != null)
        {
            defaultUpgradeCostColor = upgradeCostText.color;
        }
        hasCachedUpgradeCostColor = true;
    }

    private static string BuildLevelDetails(
        CombatSkill targetSkill,
        int currentLevel)
    {
        StringBuilder builder = new StringBuilder();
        for (int level = 1; level <= targetSkill.MaxLevel; level++)
        {
            builder.Append("Lv.").Append(level).Append(": ")
                .Append(targetSkill.GetLevelDescription(level))
                .Append("  |  ").Append(targetSkill.GetStatPointCost(level))
                .Append(" SP");
            if (level < targetSkill.MaxLevel)
            {
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }

    private void SetIcon(Sprite icon)
    {
        if (skillIcon != null)
        {
            skillIcon.sprite = icon;
            skillIcon.enabled = icon != null;
        }
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    private static void SetColoredText(
        TextMeshProUGUI text,
        string value,
        Color color)
    {
        SetText(text, value);
        if (text != null)
        {
            text.color = color;
        }
    }
}
