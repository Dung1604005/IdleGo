using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ItemInformationUISetupFactory
{
    public const string StatLinePath =
        "Assets/_GAME/Prefabs/UI/StatLine.prefab";
    private const string SeparatorPath =
        "Assets/_GAME/Prefabs/UI/Image-Line.prefab";

    public static void PrepareLayoutPrefabs()
    {
        AddLayoutElementToPrefab(StatLinePath, 27f);
        AddLayoutElementToPrefab(SeparatorPath, 5f);
    }

    public static GameObject GetOrCreateSeparator(
        RectTransform parent,
        string name)
    {
        Transform existing = FindChild(parent, name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SeparatorPath);
        GameObject instance = PrefabUtility.InstantiatePrefab(
            prefab, parent) as GameObject;
        Undo.RegisterCreatedObjectUndo(instance, "Create item stat separator");
        instance.name = name;
        return instance;
    }

    public static void RemoveLegacyRows(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            if (child.name.Contains("AttackSpeed") || child.name == "Image-Line")
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
    }

    public static void ConfigureVerticalLayout(
        RectTransform rect,
        float spacing)
    {
        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(rect.gameObject);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    public static void ConfigureHeaderLayout(HorizontalLayoutGroup layout)
    {
        layout.padding = new RectOffset(35, 35, 2, 2);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    public static RectTransform GetOrCreateRect(Transform parent, string name)
    {
        Transform existing = FindChild(parent, name);
        if (existing != null)
        {
            return existing as RectTransform;
        }

        GameObject created = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(created, $"Create {name}");
        created.layer = 5;
        RectTransform rect = created.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = Vector2.zero;
        return rect;
    }

    public static Image GetOrCreateImage(RectTransform parent, string name)
    {
        Transform existing = FindChild(parent, name);
        if (existing != null)
        {
            return existing.GetComponent<Image>();
        }

        RectTransform rect = GetOrCreateRect(parent, name);
        Image image = Undo.AddComponent<Image>(rect.gameObject);
        LayoutElement element = GetOrAdd<LayoutElement>(rect.gameObject);
        element.preferredWidth = 24f;
        element.preferredHeight = 24f;
        image.raycastTarget = false;
        return image;
    }

    public static TextMeshProUGUI GetOrCreateText(
        RectTransform parent,
        string name,
        string value,
        float fontSize,
        TextMeshProUGUI template = null)
    {
        Transform existing = FindChild(parent, name);
        TextMeshProUGUI text;
        if (existing == null)
        {
            RectTransform rect = GetOrCreateRect(parent, name);
            text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
        }
        else
        {
            text = existing.GetComponent<TextMeshProUGUI>();
        }
        text.font = template != null ? template.font : TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.color = template != null ? template.color : Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.text = value;
        text.raycastTarget = false;
        return text;
    }

    public static void AssignHeaderFields(
        BuffStatHeaderUI header,
        Image icon,
        TextMeshProUGUI text)
    {
        SerializedObject serialized = new SerializedObject(header);
        serialized.FindProperty("iconImage").objectReferenceValue = icon;
        serialized.FindProperty("nameText").objectReferenceValue = text;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static TextMeshProUGUI GetPanelText(
        PanelItemInformation panel,
        string field)
    {
        SerializedObject serialized = new SerializedObject(panel);
        return serialized.FindProperty(field).objectReferenceValue
            as TextMeshProUGUI;
    }

    public static void SetLayoutHeight(GameObject target, float height)
    {
        LayoutElement element = GetOrAdd<LayoutElement>(target);
        element.preferredHeight = height;
        element.flexibleHeight = 0f;
    }

    public static void SetTopPivot(RectTransform rect)
    {
        if (rect == null || Mathf.Approximately(rect.pivot.y, 1f))
        {
            return;
        }

        Vector2 delta = new Vector2(0f, 1f) - rect.pivot;
        rect.anchoredPosition += Vector2.Scale(delta, rect.rect.size);
        rect.pivot = new Vector2(rect.pivot.x, 1f);
    }

    public static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(target);
    }

    public static Transform FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform result = FindChild(root.transform, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public static Transform FindChild(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChild(root.GetChild(i), name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private static void AddLayoutElementToPrefab(string path, float height)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        LayoutElement element = root.GetComponent<LayoutElement>();
        if (element == null)
        {
            element = root.AddComponent<LayoutElement>();
        }
        element.preferredHeight = height;
        element.flexibleHeight = 0f;
        Image image = root.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
