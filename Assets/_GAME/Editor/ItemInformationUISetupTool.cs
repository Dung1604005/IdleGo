using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ItemInformationUISetupTool
{
    private const string ScenePath = "Assets/m.unity";

    [InitializeOnLoadMethod]
    private static void QueueFirstSetup()
    {
        EditorApplication.delayCall += SetupWhenNeeded;
    }

    [MenuItem("Tools/IdleGo/Setup Item Information UI")]
    public static void SetupActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        Transform canvas = ItemInformationUISetupFactory.FindInScene(
            scene, "Canvas-ItemInfomationUI");
        Transform selected = canvas != null
            ? ItemInformationUISetupFactory.FindChild(canvas, "Panel-SelectedItem")
            : null;
        if (selected == null)
        {
            Debug.LogError("Cannot find Canvas-ItemInfomationUI/Panel-SelectedItem.");
            return;
        }

        ItemInformationUISetupFactory.PrepareLayoutPrefabs();
        PanelItemInformation selectedPanel = SetupPanel(selected.gameObject);
        PanelItemInformation comparisonPanel = GetOrCreateComparison(
            canvas, selected.gameObject);
        AssignCanvasPanels(canvas, selectedPanel, comparisonPanel);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Item Information UI setup completed.");
    }
    private static void SetupWhenNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        Transform panel = ItemInformationUISetupFactory.FindInScene(
            scene, "Panel-SelectedItem");
        Transform header = panel != null
            ? ItemInformationUISetupFactory.FindChild(panel, "Header-Socket")
            : null;
        HorizontalLayoutGroup headerLayout = header != null
            ? header.GetComponent<HorizontalLayoutGroup>()
            : null;
        if (scene.path == ScenePath && panel != null
            && (ItemInformationUISetupFactory.FindChild(
                panel, "PanelLayoutContent") == null
                || headerLayout == null
                || !headerLayout.childControlWidth))
        {
            SetupActiveScene();
        }
    }
    private static PanelItemInformation SetupPanel(GameObject root)
    {
        PanelItemInformation panel = root.GetComponent<PanelItemInformation>();
        EquipmentStatsContentUI stats =
            root.GetComponent<EquipmentStatsContentUI>();
        RectTransform rootRect = root.GetComponent<RectTransform>();
        ItemInformationUISetupFactory.SetTopPivot(rootRect);
        ItemInformationUISetupFactory.RemoveLegacyRows(root.transform);

        RectTransform layout = ItemInformationUISetupFactory.GetOrCreateRect(
            root.transform, "PanelLayoutContent");
        ItemInformationUISetupFactory.ConfigureVerticalLayout(layout, 5f);
        ItemInformationUISetupFactory.ConfigureContentSizeFitter(layout);
        RectTransform fixedHeader = ItemInformationUISetupFactory.GetOrCreateRect(
            layout, "FixedHeader");
        ItemInformationUISetupFactory.SetLayoutHeight(
            fixedHeader.gameObject, 220f);
        MovePanelHeader(panel, fixedHeader);
        RectTransform dynamicContent = ItemInformationUISetupFactory.GetOrCreateRect(
            layout, "DynamicContent");
        ItemInformationUISetupFactory.ConfigureVerticalLayout(dynamicContent, 4f);
        SetupDynamicContent(panel, stats, rootRect,
            layout, dynamicContent);
        return panel;
    }
    private static void SetupDynamicContent(
        PanelItemInformation panel,
        EquipmentStatsContentUI stats,
        RectTransform rootRect,
        RectTransform layout,
        RectTransform dynamicContent)
    {
        GameObject firstLine = ItemInformationUISetupFactory.GetOrCreateSeparator(
            dynamicContent, "Image-Line-MainToSub");
        GameObject secondLine = ItemInformationUISetupFactory.GetOrCreateSeparator(
            dynamicContent, "Image-Line-SubToEnhancement");
        BuffStatHeaderUI[] headers = CreateBuffHeaders(dynamicContent, panel);
        TextMeshProUGUI[] requirements = CreateRequirementTexts(layout, panel);
        AssignPanelFields(panel, stats, requirements);
        AssignStatsFields(stats, rootRect, layout, dynamicContent,
            firstLine, secondLine, headers);
    }
    private static PanelItemInformation GetOrCreateComparison(
        Transform canvas,
        GameObject selected)
    {
        Transform existing = ItemInformationUISetupFactory.FindChild(
            canvas, "Panel-EquippedComparison");
        GameObject comparison = existing != null
            ? existing.gameObject
            : Object.Instantiate(selected, canvas);
        if (existing == null)
        {
            Undo.RegisterCreatedObjectUndo(
                comparison, "Create equipment comparison panel");
            comparison.name = "Panel-EquippedComparison";
            RectTransform rect = comparison.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(
                -Mathf.Abs(rect.anchoredPosition.x), rect.anchoredPosition.y);
        }
        return SetupPanel(comparison);
    }

    private static void MovePanelHeader(
        PanelItemInformation panel,
        RectTransform fixedHeader)
    {
        SerializedObject serialized = new SerializedObject(panel);
        string[] fields = { "itemIcon", "itemNameText", "rarityText", "mainStatsText" };
        for (int i = 0; i < fields.Length; i++)
        {
            Component component = serialized.FindProperty(fields[i])
                .objectReferenceValue as Component;
            if (component != null && component.transform.parent != fixedHeader)
            {
                Undo.SetTransformParent(
                    component.transform, fixedHeader, "Move item header");
            }
        }
    }

    private static BuffStatHeaderUI[] CreateBuffHeaders(
        RectTransform parent, PanelItemInformation panel)
    {
        BuffStatHeaderUI[] headers = new BuffStatHeaderUI[BuffStatTypeUtility.Count];
        string[] names = { "Socket", "Enchantment", "Decoration" };
        TextMeshProUGUI template = ItemInformationUISetupFactory.GetPanelText(
            panel, "mainStatsText");
        for (int i = 0; i < headers.Length; i++)
        {
            RectTransform root = ItemInformationUISetupFactory.GetOrCreateRect(
                parent, $"Header-{names[i]}");
            ItemInformationUISetupFactory.SetLayoutHeight(root.gameObject, 32f);
            HorizontalLayoutGroup layout = ItemInformationUISetupFactory
                .GetOrAdd<HorizontalLayoutGroup>(root.gameObject);
            ItemInformationUISetupFactory.ConfigureHeaderLayout(layout);
            Image icon = ItemInformationUISetupFactory.GetOrCreateImage(
                root, "Image-Icon");
            TextMeshProUGUI text = ItemInformationUISetupFactory.GetOrCreateText(
                root, "Text-Name", names[i], 25f, template);
            headers[i] = ItemInformationUISetupFactory.GetOrAdd<BuffStatHeaderUI>(
                root.gameObject);
            ItemInformationUISetupFactory.AssignHeaderFields(headers[i], icon, text);
        }
        return headers;
    }

    private static TextMeshProUGUI[] CreateRequirementTexts(
        RectTransform parent,
        PanelItemInformation panel)
    {
        TextMeshProUGUI template = ItemInformationUISetupFactory.GetPanelText(
            panel, "mainStatsText");
        TextMeshProUGUI level = ItemInformationUISetupFactory.GetOrCreateText(
            parent, "Text-LevelRequirement", "Level Required", 24f, template);
        TextMeshProUGUI character = ItemInformationUISetupFactory.GetOrCreateText(
            parent, "Text-CharacterRequirement", "Class Required", 24f, template);
        ItemInformationUISetupFactory.SetLayoutHeight(level.gameObject, 32f);
        ItemInformationUISetupFactory.SetLayoutHeight(character.gameObject, 32f);
        return new[] { level, character };
    }

    private static void AssignPanelFields(
        PanelItemInformation panel,
        EquipmentStatsContentUI stats,
        TextMeshProUGUI[] requirements)
    {
        SerializedObject serialized = new SerializedObject(panel);
        serialized.FindProperty("levelRequirementText").objectReferenceValue =
            requirements[0];
        serialized.FindProperty("characterRequirementText").objectReferenceValue =
            requirements[1];
        serialized.FindProperty("statsContent").objectReferenceValue = stats;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignStatsFields(
        EquipmentStatsContentUI stats,
        RectTransform panel,
        RectTransform layout,
        RectTransform dynamicContent,
        GameObject firstLine,
        GameObject secondLine,
        BuffStatHeaderUI[] headers)
    {
        SerializedObject serialized = new SerializedObject(stats);
        serialized.FindProperty("dynamicContent").objectReferenceValue = dynamicContent;
        serialized.FindProperty("statLinePrefab").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<StatLineUI>(
                ItemInformationUISetupFactory.StatLinePath);
        serialized.FindProperty("mainToSubStatLine").objectReferenceValue = firstLine;
        serialized.FindProperty("subStatToEnhancementLine").objectReferenceValue =
            secondLine;
        serialized.FindProperty("panelRect").objectReferenceValue = panel;
        serialized.FindProperty("panelLayoutContent").objectReferenceValue = layout;
        SerializedProperty list = serialized.FindProperty("buffStatHeaders");
        list.arraySize = headers.Length;
        for (int i = 0; i < headers.Length; i++)
        {
            list.GetArrayElementAtIndex(i).objectReferenceValue = headers[i];
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignCanvasPanels(
        Transform canvas,
        PanelItemInformation selected,
        PanelItemInformation comparison)
    {
        CanvasItemInfomationUI canvasUI = canvas.GetComponent<CanvasItemInfomationUI>();
        SerializedObject serialized = new SerializedObject(canvasUI);
        serialized.FindProperty("selectedItemPanel").objectReferenceValue = selected;
        serialized.FindProperty("equippedComparisonPanel").objectReferenceValue =
            comparison;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
