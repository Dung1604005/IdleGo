using UnityEditor;
using UnityEngine;

public partial class EquipmentBalanceGeneratorWindow
{
    private void DrawPowerPreview(EquipmentBalancePreview preview)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("4. Power Preview", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        DrawMetric("Level Power", preview.LevelPower.ToString("0.##"));
        DrawMetric("Item Power", preview.ItemPower.ToString("0.##"));
        DrawMetric("Base Stats", preview.BaseStatBudget.ToString("0.##"));
        DrawMetric("Enhancement", preview.EnhancementBudget.ToString("0.##"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawGeneratedStats(EquipmentBalancePreview preview)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("5. Kết quả sẽ tạo", EditorStyles.boldLabel);
        if (!preview.IsValid)
        {
            EditorGUILayout.HelpBox(preview.Error, MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        for (int i = 0; i < preview.Stats.Count; i++)
        {
            DrawGeneratedStatRow(preview.Stats[i], i == 0);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawGeneratedStatRow(
        EquipmentBalanceStatResult result,
        bool isMain)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(isMain ? "MAIN" : "SUB", GUILayout.Width(42f));
        GUILayout.Label(
            EquipmentBalanceRules.GetDisplayName(result.StatType),
            GUILayout.Width(165f));
        GUILayout.Label(FormatValue(result), GUILayout.Width(85f));
        GUILayout.Label(result.Operation.ToString(), GUILayout.Width(110f));
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Power {result.AllocatedPower:0.##}");
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGenerateButton(EquipmentBalancePreview preview)
    {
        EditorGUI.BeginDisabledGroup(!preview.IsValid);
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            fixedHeight = 42f
        };
        if (GUILayout.Button("GENERATE & SAVE STATS", buttonStyle))
        {
            EquipmentBalanceCalculator.ApplyToAsset(equipment, preview);
            statusMessage =
                $"Đã cập nhật {preview.Stats.Count} stat cho {equipment.name}.";
            Selection.activeObject = equipment;
            EditorGUIUtility.PingObject(equipment);
        }
        EditorGUI.EndDisabledGroup();

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }
        GUILayout.Space(12f);
    }

    private EquipmentBalancePreview GetPreview()
    {
        return EquipmentBalanceCalculator.BuildPreview(
            equipment,
            mainStat,
            subStats,
            rollMultiplier);
    }

    private static StatType DrawStatPopup(string label, StatType current)
    {
        StatType[] stats = EquipmentBalanceRules.AvailableStats;
        string[] names = new string[stats.Length];
        int selectedIndex = 0;
        for (int i = 0; i < stats.Length; i++)
        {
            names[i] = EquipmentBalanceRules.GetDisplayName(stats[i]);
            if (stats[i] == current)
            {
                selectedIndex = i;
            }
        }

        int nextIndex = EditorGUILayout.Popup(label, selectedIndex, names);
        return stats[nextIndex];
    }

    private static void DrawMetric(string title, string value)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinWidth(100f));
        GUILayout.Label(title, EditorStyles.miniLabel);
        GUILayout.Label(value, EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
    }

    private static string FormatValue(EquipmentBalanceStatResult result)
    {
        return EquipmentBalanceRules.IsRatioStat(result.StatType)
            ? $"+{result.Value * 100f:0.##}%"
            : $"+{result.Value:0.##}";
    }

    private static GUIStyle CenteredWhiteStyle()
    {
        return new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }
}
