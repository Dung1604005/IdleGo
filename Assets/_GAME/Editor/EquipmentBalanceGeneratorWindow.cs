using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class EquipmentBalanceGeneratorWindow : EditorWindow
{
    private EquipmentDataSO equipment;
    private StatType mainStat = StatType.DAMAGE;
    private readonly List<StatType> subStats = new List<StatType>();
    private Vector2 scrollPosition;
    private float rollMultiplier = 1f;
    private string statusMessage;

    [MenuItem("Tools/IdleGo/Equipment Balance Generator")]
    public static void Open()
    {
        EquipmentBalanceGeneratorWindow window =
            GetWindow<EquipmentBalanceGeneratorWindow>();
        window.titleContent = new GUIContent("Equipment Balance");
        window.minSize = new Vector2(540f, 650f);
        window.Show();
    }

    private void OnEnable()
    {
        if (Selection.activeObject is EquipmentDataSO selectedEquipment)
        {
            SetEquipment(selectedEquipment);
        }
    }

    private void OnGUI()
    {
        DrawHeader();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawTargetSection();
        if (equipment != null)
        {
            DrawEquipmentSummary();
            DrawRollSection();
            DrawStatSection();
            EquipmentBalancePreview preview = GetPreview();
            DrawPowerPreview(preview);
            DrawGeneratedStats(preview);
            DrawGenerateButton(preview);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        Rect rect = GUILayoutUtility.GetRect(0f, 74f, GUILayout.ExpandWidth(true));
        Color color = EditorGUIUtility.isProSkin
            ? new Color(0.10f, 0.24f, 0.32f)
            : new Color(0.24f, 0.55f, 0.68f);
        EditorGUI.DrawRect(rect, color);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(rect.x, rect.y + 10f, rect.width, 28f),
            "EQUIPMENT BALANCE GENERATOR", titleStyle);
        GUI.Label(new Rect(rect.x, rect.y + 39f, rect.width, 22f),
            "Chọn stat → xem preview → Generate", CenteredWhiteStyle());
        GUILayout.Space(8f);
    }

    private void DrawTargetSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("1. Equipment Asset", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        EquipmentDataSO selected = (EquipmentDataSO)EditorGUILayout.ObjectField(
            "EquipmentDataSO", equipment, typeof(EquipmentDataSO), false);
        if (EditorGUI.EndChangeCheck())
        {
            SetEquipment(selected);
        }

        if (equipment == null)
        {
            EditorGUILayout.HelpBox(
                "Kéo EquipmentDataSO vào đây để bắt đầu.",
                MessageType.Info);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawEquipmentSummary()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Thông tin đầu vào", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        DrawMetric("Level", equipment.LevelRequired.ToString());
        DrawMetric("Tier", EquipmentBalanceRules.GetEquipmentTier(
            equipment.LevelRequired).ToString());
        DrawMetric("Rarity", equipment.RarityType.ToString());
        DrawMetric("Slot", equipment.EquipmentType.ToString());
        EditorGUILayout.EndHorizontal();
        float damageScale = EquipmentBalanceRules.GetDamageMultiplier(
            equipment.CharacterRequirement);
        EditorGUILayout.LabelField(
            $"Class: {equipment.CharacterRequirement}   •   Damage scale: x{damageScale:0.##}",
            EditorStyles.miniBoldLabel);
        EditorGUILayout.EndVertical();
    }

    private void DrawRollSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("2. Quality Roll", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        rollMultiplier = EditorGUILayout.Slider(
            rollMultiplier,
            EquipmentBalanceRules.MinRoll,
            EquipmentBalanceRules.MaxRoll);
        if (GUILayout.Button("Random", GUILayout.Width(80f)))
        {
            rollMultiplier = Random.Range(
                EquipmentBalanceRules.MinRoll,
                EquipmentBalanceRules.MaxRoll);
            statusMessage = null;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(
            $"Hệ số hiện tại: x{rollMultiplier:0.000}",
            EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }

    private void DrawStatSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("3. Chọn chỉ số", EditorStyles.boldLabel);
        if (GUILayout.Button("Preset theo slot", GUILayout.Width(120f)))
        {
            ApplyRecommendedPreset();
        }
        EditorGUILayout.EndHorizontal();

        StatType previousMain = mainStat;
        mainStat = DrawStatPopup("Main Stat", mainStat);
        if (previousMain != mainStat)
        {
            subStats.Remove(mainStat);
            statusMessage = null;
        }

        int required = EquipmentBalanceRules.GetRequiredSubStatCount(
            equipment.RarityType);
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(
            $"Sub Stats ({subStats.Count}/{required})",
            EditorStyles.boldLabel);
        DrawSubStatGrid(required);
        EditorGUILayout.EndVertical();
    }

    private void DrawSubStatGrid(int requiredCount)
    {
        if (requiredCount == 0)
        {
            subStats.Clear();
            EditorGUILayout.HelpBox("COMMON chỉ có Main Stat.", MessageType.None);
            return;
        }

        StatType[] available = EquipmentBalanceRules.AvailableStats;
        for (int i = 0; i < available.Length; i += 2)
        {
            EditorGUILayout.BeginHorizontal();
            DrawSubStatToggle(available[i], requiredCount);
            if (i + 1 < available.Length)
            {
                DrawSubStatToggle(available[i + 1], requiredCount);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSubStatToggle(StatType statType, int requiredCount)
    {
        if (statType == mainStat)
        {
            GUILayout.Space(position.width * 0.45f);
            return;
        }

        bool selected = subStats.Contains(statType);
        bool next = EditorGUILayout.ToggleLeft(
            EquipmentBalanceRules.GetDisplayName(statType),
            selected,
            GUILayout.MinWidth(210f));
        if (next == selected)
        {
            return;
        }

        if (next && subStats.Count < requiredCount)
        {
            subStats.Add(statType);
        }
        else if (!next)
        {
            subStats.Remove(statType);
        }
        statusMessage = null;
    }

    private void SetEquipment(EquipmentDataSO value)
    {
        equipment = value;
        statusMessage = null;
        if (equipment != null)
        {
            ApplyRecommendedPreset();
        }
        Repaint();
    }

    private void ApplyRecommendedPreset()
    {
        mainStat = EquipmentBalanceRules.GetRecommendedMain(equipment);
        subStats.Clear();
        subStats.AddRange(
            EquipmentBalanceRules.GetRecommendedSubs(equipment, mainStat));
        statusMessage = null;
    }
}
