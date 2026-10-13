#if UNITY_EDITOR
using TMPro;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

internal static class HeroStatusSkillPanelBuilder
{
    public static HeroSkillInformationPanel Build(Transform parent)
    {
        GameObject root = HeroStatusEditorUI.Create(
            "Panel-SkillInformation", parent);
        HeroStatusEditorUI.SetRect(root, Half, Half,
            new Vector2(0f, -675f), new Vector2(900f, 370f));
        HeroStatusEditorUI.AddImage(root, HeroStatusEditorUI.SectionColor);
        HeroSkillInformationPanel panel =
            root.AddComponent<HeroSkillInformationPanel>();

        Image icon = CreateIcon(root.transform);
        TextMeshProUGUI name = CreateText(root.transform, "Skill Name",
            new Vector2(-160f, 142f), new Vector2(500f, 48f), 34f,
            TextAlignmentOptions.MidlineLeft, HeroStatusEditorUI.AccentColor);
        TextMeshProUGUI description = CreateText(root.transform, "Description",
            new Vector2(-40f, 92f), new Vector2(720f, 44f), 23f,
            TextAlignmentOptions.MidlineLeft, Color.white);
        description.textWrappingMode = TextWrappingModes.Normal;
        TextMeshProUGUI currentLevel = CreateText(root.transform, "Current Level",
            new Vector2(-260f, 43f), new Vector2(410f, 38f), 24f,
            TextAlignmentOptions.MidlineLeft, Color.white);
        TextMeshProUGUI details = CreateText(root.transform, "Level Details",
            new Vector2(-55f, -52f), new Vector2(750f, 140f), 21f,
            TextAlignmentOptions.TopLeft, Color.white);
        details.textWrappingMode = TextWrappingModes.Normal;
        TextMeshProUGUI cost = CreateText(root.transform, "Upgrade Cost",
            new Vector2(-250f, -140f), new Vector2(430f, 40f), 22f,
            TextAlignmentOptions.MidlineLeft, HeroStatusEditorUI.AccentColor);

        Button upgrade = AddActionButton(root.transform, panel,
            "Button-Upgrade", "Upgrade", new Vector2(30f, -140f),
            panel.OnButtonUpgrade);
        Button equip = AddActionButton(root.transform, panel,
            "Button-Equip", "Equip", new Vector2(215f, -140f),
            panel.OnButtonEquip);
        Button unequip = AddActionButton(root.transform, panel,
            "Button-Unequip", "Unequip", new Vector2(215f, -140f),
            panel.OnButtonUnequip);
        Button close = HeroStatusEditorUI.AddButton(
            "Button-Close", root.transform, "X",
            new Vector2(410f, 145f), new Vector2(54f, 54f));
        UnityEventTools.AddPersistentListener(close.onClick, panel.OnButtonClose);

        HeroStatusEditorUI.SetObject(panel, "skillIcon", icon);
        HeroStatusEditorUI.SetObject(panel, "skillNameText", name);
        HeroStatusEditorUI.SetObject(panel, "descriptionText", description);
        HeroStatusEditorUI.SetObject(panel, "currentLevelText", currentLevel);
        HeroStatusEditorUI.SetObject(panel, "levelDetailsText", details);
        HeroStatusEditorUI.SetObject(panel, "upgradeCostText", cost);
        HeroStatusEditorUI.SetObject(panel, "upgradeButton", upgrade);
        HeroStatusEditorUI.SetObject(panel, "equipButton", equip);
        HeroStatusEditorUI.SetObject(panel, "unequipButton", unequip);
        root.SetActive(false);
        return panel;
    }

    private static Image CreateIcon(Transform parent)
    {
        GameObject target = HeroStatusEditorUI.Create("Skill Icon", parent);
        HeroStatusEditorUI.SetRect(target, Half, Half,
            new Vector2(-385f, 110f), new Vector2(100f, 100f));
        Image icon = target.AddComponent<Image>();
        icon.preserveAspect = true;
        return icon;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color)
    {
        TextMeshProUGUI text = HeroStatusEditorUI.AddText(
            name, parent, name, fontSize, alignment, color);
        HeroStatusEditorUI.SetRect(
            text.gameObject, Half, Half, position, size);
        return text;
    }

    private static Button AddActionButton(
        Transform parent,
        HeroSkillInformationPanel panel,
        string name,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Button button = HeroStatusEditorUI.AddButton(
            name, parent, label, position, new Vector2(165f, 58f));
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    private static Vector2 Half => new Vector2(0.5f, 0.5f);
}
#endif
