using UnityEngine;
using UnityEngine.UI;

public partial class PanelItemInformation
{
    [SerializeField] private Image mainStatTypeIcon;

    private Color defaultMainStatColor = Color.white;
    private bool hasCachedMainStatColor;

    public void OnInit(Item item)
    {
        if (item is Equipment equipment)
        {
            OnInit(equipment);
            return;
        }

        if (item is EnchantMaterial material)
        {
            OnInitMaterial(material);
            return;
        }

        OnInit((Equipment)null);
    }

    private void OnInitMaterial(EnchantMaterial material)
    {
        EnchantMaterialDataSO data = material?.Data;
        CacheRequirementColors();
        CacheMainStatColor();
        displayedEquipment = null;
        gameObject.SetActive(data != null);
        if (data == null)
        {
            ClearView();
            return;
        }

        statsContent?.OnInit();
        Color rarityColor = DataManager.Ins.GetRarityColor(data.RarityType);
        SetIcon(data.Icon);
        SetColoredText(itemNameText, data.NameItem, rarityColor);
        SetColoredText(rarityText,
            EquipmentInformationText.GetRarityName(data.RarityType), rarityColor);
        SetMaterialMainStat(data.BuffStatType);
        statsContent?.Refresh(data);
        SetText(levelRequirementText, string.Empty);
        SetText(characterRequirementText, string.Empty);
        SetButtonActive(equipButton, false);
        SetButtonActive(unequipButton, false);
    }

    private void SetMaterialMainStat(BuffStatType buffStatType)
    {
        BuffStatVisualSO visual = DataManager.Ins.BuffStatVisualSO;
        Color color = visual != null
            ? visual.GetColorBuffStat(buffStatType)
            : Color.white;
        SetColoredText(mainStatsText,
            EquipmentInformationText.GetBuffStatName(buffStatType), color);
        SetMaterialIcon(visual?.GetIconBuffStat(buffStatType), color);
    }

    private void SetMaterialIcon(Sprite sprite, Color color)
    {
        if (mainStatTypeIcon == null)
        {
            return;
        }

        mainStatTypeIcon.sprite = sprite;
        mainStatTypeIcon.color = color;
        mainStatTypeIcon.gameObject.SetActive(sprite != null);
    }

    private void ResetMaterialVisual()
    {
        CacheMainStatColor();
        SetMaterialIcon(null, Color.white);
        if (mainStatsText != null)
        {
            mainStatsText.color = defaultMainStatColor;
        }
    }

    private void CacheMainStatColor()
    {
        if (!hasCachedMainStatColor && mainStatsText != null)
        {
            defaultMainStatColor = mainStatsText.color;
            hasCachedMainStatColor = true;
        }
    }
}
