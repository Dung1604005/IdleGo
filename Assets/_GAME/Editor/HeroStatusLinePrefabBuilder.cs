#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

internal static class HeroStatusLinePrefabBuilder
{
    public const string StatLinePath =
        "Assets/_GAME/Prefabs/UI/StatProgressLine.prefab";
    public const string SkillSlotPath =
        "Assets/_GAME/Prefabs/UI/HeroStatusSkillSlot.prefab";

    public static StatProgressLineUI BuildStatLine()
    {
        GameObject root = HeroStatusEditorUI.Create("StatProgressLine", null);
        HeroStatusEditorUI.SetRect(root, Vector2.zero, Vector2.one,
            Vector2.zero, new Vector2(0f, 88f));
        HeroStatusEditorUI.AddImage(root, HeroStatusEditorUI.SectionColor);
        LayoutElement layout = root.AddComponent<LayoutElement>();
        layout.preferredHeight = 88f;
        StatProgressLineUI line = root.AddComponent<StatProgressLineUI>();

        Image icon = CreateIcon(root.transform);
        TextMeshProUGUI name = CreateText(
            "Stat Name", root.transform, new Vector2(-215f, 0f),
            new Vector2(250f, 70f), TextAlignmentOptions.MidlineLeft);
        TextMeshProUGUI current = CreateText(
            "Current Value", root.transform, new Vector2(80f, 0f),
            new Vector2(150f, 70f), TextAlignmentOptions.Center);
        TextMeshProUGUI increase = CreateText(
            "Increase Value", root.transform, new Vector2(260f, 0f),
            new Vector2(240f, 70f), TextAlignmentOptions.Center);
        Button button = HeroStatusEditorUI.AddButton(
            "Button-Increase", root.transform, "+",
            new Vector2(400f, 0f), new Vector2(68f, 68f));
        UnityEventTools.AddPersistentListener(
            button.onClick, line.OnButtonIncrease);

        HeroStatusEditorUI.SetObject(line, "statIcon", icon);
        HeroStatusEditorUI.SetObject(line, "statNameText", name);
        HeroStatusEditorUI.SetObject(line, "currentValueText", current);
        HeroStatusEditorUI.SetObject(line, "increaseValueText", increase);
        HeroStatusEditorUI.SetObject(line, "increaseButton", button);
        return SavePrefab(root, StatLinePath)
            .GetComponent<StatProgressLineUI>();
    }

    public static HeroStatusSkillSlotUI BuildSkillSlot()
    {
        GameObject root = HeroStatusEditorUI.Create("HeroStatusSkillSlot", null);
        HeroStatusEditorUI.SetRect(root, Vector2.zero, Vector2.one,
            Vector2.zero, new Vector2(130f, 145f));
        Image background = HeroStatusEditorUI.AddImage(
            root, HeroStatusEditorUI.SectionColor);
        Button button = root.AddComponent<Button>();
        button.targetGraphic = background;
        LayoutElement layout = root.AddComponent<LayoutElement>();
        layout.preferredWidth = 130f;
        layout.preferredHeight = 145f;
        HeroStatusSkillSlotUI slot = root.AddComponent<HeroStatusSkillSlotUI>();

        GameObject iconObject = HeroStatusEditorUI.Create("Skill Icon", root.transform);
        HeroStatusEditorUI.SetRect(iconObject, Half, Half,
            new Vector2(0f, 14f), new Vector2(104f, 104f));
        Image icon = iconObject.AddComponent<Image>();
        GameObject selected = CreateFrame(root.transform, "Selected Frame",
            HeroStatusEditorUI.AccentColor, new Vector2(124f, 139f));
        TextMeshProUGUI level = CreateText(
            "Level", root.transform, new Vector2(0f, -56f),
            new Vector2(120f, 28f), TextAlignmentOptions.Center);
        GameObject equipped = CreateEquippedMark(root.transform);
        UnityEventTools.AddPersistentListener(button.onClick, slot.OnButtonSelect);

        HeroStatusEditorUI.SetObject(slot, "skillIcon", icon);
        HeroStatusEditorUI.SetObject(slot, "levelText", level);
        HeroStatusEditorUI.SetObject(slot, "selectedFrame", selected);
        HeroStatusEditorUI.SetObject(slot, "equippedIcon", equipped);
        return SavePrefab(root, SkillSlotPath)
            .GetComponent<HeroStatusSkillSlotUI>();
    }

    private static Image CreateIcon(Transform parent)
    {
        GameObject target = HeroStatusEditorUI.Create("Stat Icon", parent);
        HeroStatusEditorUI.SetRect(target, Half, Half,
            new Vector2(-410f, 0f), new Vector2(58f, 58f));
        return target.AddComponent<Image>();
    }

    private static TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size,
        TextAlignmentOptions alignment)
    {
        TextMeshProUGUI text = HeroStatusEditorUI.AddText(
            name, parent, name, 25f, alignment);
        HeroStatusEditorUI.SetRect(
            text.gameObject, Half, Half, position, size);
        return text;
    }

    private static GameObject CreateFrame(
        Transform parent,
        string name,
        Color color,
        Vector2 size)
    {
        GameObject frame = HeroStatusEditorUI.Create(name, parent);
        HeroStatusEditorUI.SetRect(frame, Half, Half, Vector2.zero, size);
        Image image = HeroStatusEditorUI.AddImage(frame, color);
        image.raycastTarget = false;
        frame.transform.SetAsFirstSibling();
        return frame;
    }

    private static GameObject CreateEquippedMark(Transform parent)
    {
        GameObject mark = HeroStatusEditorUI.Create("Equipped", parent);
        HeroStatusEditorUI.SetRect(mark, Half, Half,
            new Vector2(44f, 48f), new Vector2(34f, 34f));
        HeroStatusEditorUI.AddImage(mark, new Color32(67, 211, 126, 255));
        TextMeshProUGUI text = HeroStatusEditorUI.AddText(
            "Text", mark.transform, "E", 22f, TextAlignmentOptions.Center);
        HeroStatusEditorUI.Stretch(text.rectTransform);
        return mark;
    }

    private static GameObject SavePrefab(GameObject root, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static Vector2 Half => new Vector2(0.5f, 0.5f);
}
#endif
