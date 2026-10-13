#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

internal static class HeroStatusEditorUI
{
    public static readonly Color PanelColor = new Color32(39, 31, 78, 255);
    public static readonly Color SectionColor = new Color32(61, 52, 105, 255);
    public static readonly Color AccentColor = new Color32(238, 178, 59, 255);
    public static readonly Color ButtonColor = new Color32(117, 73, 164, 255);

    public static GameObject Create(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(
            name,
            typeof(RectTransform));
        gameObject.layer = LayerMask.NameToLayer("UI");
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    public static RectTransform SetRect(
        GameObject target,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = target.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    public static Image AddImage(GameObject target, Color color)
    {
        Image image = target.AddComponent<Image>();
        image.color = color;
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(
            "UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        return image;
    }

    public static TextMeshProUGUI AddText(
        string name,
        Transform parent,
        string value,
        float size,
        TextAlignmentOptions alignment,
        Color? color = null)
    {
        GameObject target = Create(name, parent);
        TextMeshProUGUI text = target.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color ?? Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    public static Button AddButton(
        string name,
        Transform parent,
        string label,
        Vector2 position,
        Vector2 size)
    {
        GameObject target = Create(name, parent);
        SetRect(target, Half, Half, position, size);
        Image image = AddImage(target, ButtonColor);
        Button button = target.AddComponent<Button>();
        button.targetGraphic = image;
        TextMeshProUGUI text = AddText(
            "Text",
            target.transform,
            label,
            28f,
            TextAlignmentOptions.Center);
        Stretch(text.rectTransform, 8f);
        return button;
    }

    public static void Stretch(RectTransform rect, float inset = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    public static void SetObject(
        Object target,
        string propertyName,
        Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    public static ScrollRect AddScrollView(
        string name,
        Transform parent,
        bool horizontal,
        out RectTransform content)
    {
        GameObject root = Create(name, parent);
        AddImage(root, SectionColor);
        ScrollRect scroll = root.AddComponent<ScrollRect>();
        GameObject viewport = Create("Viewport", root.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect, 10f);
        AddImage(viewport, new Color(1f, 1f, 1f, 0.01f));
        viewport.AddComponent<RectMask2D>();
        GameObject contentObject = Create("Content", viewport.transform);
        content = contentObject.GetComponent<RectTransform>();
        ConfigureContent(contentObject, horizontal);
        scroll.viewport = viewportRect;
        scroll.content = content;
        scroll.horizontal = horizontal;
        scroll.vertical = !horizontal;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 25f;
        return scroll;
    }

    private static void ConfigureContent(GameObject target, bool horizontal)
    {
        RectTransform rect = target.GetComponent<RectTransform>();
        ContentSizeFitter fitter = target.AddComponent<ContentSizeFitter>();
        if (horizontal)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            HorizontalLayoutGroup layout = target.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18f;
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.childControlHeight = true;
        }
        else
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
        }
    }

    private static Vector2 Half => new Vector2(0.5f, 0.5f);
}
#endif
