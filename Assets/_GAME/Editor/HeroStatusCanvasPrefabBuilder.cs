#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

internal static class HeroStatusCanvasPrefabBuilder
{
    public const string CanvasPath =
        "Assets/_GAME/Resources/UI/Canvas-HeroStatus.prefab";

    public static CanvasHeroStatus Build(
        StatProgressLineUI statLinePrefab,
        HeroStatusSkillSlotUI skillSlotPrefab)
    {
        GameObject root = CreateCanvasRoot();
        CanvasHeroStatus canvas = root.AddComponent<CanvasHeroStatus>();
        GameObject panel = CreateMainPanel(root.transform);
        Button close = HeroStatusEditorUI.AddButton(
            "Button-Close", panel.transform, "X",
            new Vector2(420f, 820f), new Vector2(66f, 66f));
        UnityEventTools.AddPersistentListener(close.onClick, canvas.OnButtonClose);

        BuildHeader(panel.transform, out Image avatar, out TextMeshProUGUI name,
            out TextMeshProUGUI level, out TextMeshProUGUI experience,
            out Image experienceFill);
        BuildStats(panel.transform, canvas, out TextMeshProUGUI statPoints,
            out RectTransform statContent);
        BuildSkills(panel.transform, out RectTransform skillContent);
        HeroSkillInformationPanel skillPanel =
            HeroStatusSkillPanelBuilder.Build(panel.transform);

        HeroStatusEditorUI.SetObject(canvas, "heroAvatar", avatar);
        HeroStatusEditorUI.SetObject(canvas, "heroNameText", name);
        HeroStatusEditorUI.SetObject(canvas, "levelText", level);
        HeroStatusEditorUI.SetObject(canvas, "experienceText", experience);
        HeroStatusEditorUI.SetObject(canvas, "experienceFill", experienceFill);
        HeroStatusEditorUI.SetObject(canvas, "statPointText", statPoints);
        HeroStatusEditorUI.SetObject(canvas, "statContent", statContent);
        HeroStatusEditorUI.SetObject(canvas, "statLinePrefab", statLinePrefab);
        HeroStatusEditorUI.SetObject(canvas, "skillContent", skillContent);
        HeroStatusEditorUI.SetObject(canvas, "skillSlotPrefab", skillSlotPrefab);
        HeroStatusEditorUI.SetObject(canvas, "skillInformationPanel", skillPanel);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CanvasPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<CanvasHeroStatus>();
    }

    private static GameObject CreateCanvasRoot()
    {
        GameObject root = HeroStatusEditorUI.Create("Canvas-HeroStatus", null);
        RectTransform rect = root.GetComponent<RectTransform>();
        HeroStatusEditorUI.Stretch(rect);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();
        GameObject background = HeroStatusEditorUI.Create("Background", root.transform);
        HeroStatusEditorUI.Stretch(background.GetComponent<RectTransform>());
        HeroStatusEditorUI.AddImage(background, new Color(0f, 0f, 0f, 0.72f));
        return root;
    }

    private static GameObject CreateMainPanel(Transform parent)
    {
        GameObject panel = HeroStatusEditorUI.Create("Panel-HeroStatus", parent);
        HeroStatusEditorUI.SetRect(panel, Half, Half,
            Vector2.zero, new Vector2(980f, 1800f));
        HeroStatusEditorUI.AddImage(panel, HeroStatusEditorUI.PanelColor);
        return panel;
    }

    private static void BuildHeader(
        Transform parent,
        out Image avatar,
        out TextMeshProUGUI name,
        out TextMeshProUGUI level,
        out TextMeshProUGUI experience,
        out Image experienceFill)
    {
        GameObject avatarObject = HeroStatusEditorUI.Create("Hero Avatar", parent);
        HeroStatusEditorUI.SetRect(avatarObject, Half, Half,
            new Vector2(-350f, 700f), new Vector2(190f, 190f));
        avatar = avatarObject.AddComponent<Image>();
        avatar.preserveAspect = true;
        name = CreateText(parent, "Hero Name", "Hero Name",
            new Vector2(-125f, 765f), new Vector2(420f, 60f), 42f,
            TextAlignmentOptions.MidlineLeft, HeroStatusEditorUI.AccentColor);
        level = CreateText(parent, "Level", "Level 1",
            new Vector2(-180f, 700f), new Vector2(310f, 50f), 30f,
            TextAlignmentOptions.MidlineLeft, Color.white);
        experience = CreateText(parent, "Experience", "EXP 0 / 100",
            new Vector2(-115f, 645f), new Vector2(440f, 42f), 26f,
            TextAlignmentOptions.MidlineLeft, Color.white);
        experienceFill = CreateExperienceBar(parent);
    }

    private static Image CreateExperienceBar(Transform parent)
    {
        GameObject background = HeroStatusEditorUI.Create("Experience Bar", parent);
        HeroStatusEditorUI.SetRect(background, Half, Half,
            new Vector2(65f, 600f), new Vector2(560f, 28f));
        HeroStatusEditorUI.AddImage(background, new Color32(22, 18, 46, 255));
        GameObject fillObject = HeroStatusEditorUI.Create("Fill", background.transform);
        HeroStatusEditorUI.Stretch(fillObject.GetComponent<RectTransform>(), 3f);
        Image fill = HeroStatusEditorUI.AddImage(
            fillObject, new Color32(76, 187, 230, 255));
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0.5f;
        return fill;
    }

    private static void BuildStats(
        Transform parent,
        CanvasHeroStatus canvas,
        out TextMeshProUGUI statPoints,
        out RectTransform content)
    {
        CreateText(parent, "Stats Title", "HERO STATS",
            new Vector2(-330f, 535f), new Vector2(300f, 52f), 34f,
            TextAlignmentOptions.MidlineLeft, HeroStatusEditorUI.AccentColor);
        statPoints = CreateText(parent, "Stat Points", "Stat Points: 0",
            new Vector2(95f, 535f), new Vector2(300f, 52f), 28f,
            TextAlignmentOptions.Center, Color.white);
        Button reset = HeroStatusEditorUI.AddButton(
            "Button-ResetStats", parent, "Reset",
            new Vector2(360f, 535f), new Vector2(150f, 56f));
        UnityEventTools.AddPersistentListener(
            reset.onClick, canvas.OnButtonResetStatPoints);
        ScrollRect scroll = HeroStatusEditorUI.AddScrollView(
            "ScrollView-Stats", parent, false, out content);
        HeroStatusEditorUI.SetRect(scroll.gameObject, Half, Half,
            new Vector2(0f, 150f), new Vector2(900f, 700f));
    }

    private static void BuildSkills(
        Transform parent,
        out RectTransform content)
    {
        CreateText(parent, "Skills Title", "SKILLS",
            new Vector2(-365f, -245f), new Vector2(220f, 52f), 34f,
            TextAlignmentOptions.MidlineLeft, HeroStatusEditorUI.AccentColor);
        ScrollRect scroll = HeroStatusEditorUI.AddScrollView(
            "ScrollView-Skills", parent, true, out content);
        HeroStatusEditorUI.SetRect(scroll.gameObject, Half, Half,
            new Vector2(0f, -370f), new Vector2(900f, 190f));
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string value,
        Vector2 position,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color)
    {
        TextMeshProUGUI text = HeroStatusEditorUI.AddText(
            name, parent, value, fontSize, alignment, color);
        HeroStatusEditorUI.SetRect(
            text.gameObject, Half, Half, position, size);
        return text;
    }

    private static Vector2 Half => new Vector2(0.5f, 0.5f);
}
#endif
