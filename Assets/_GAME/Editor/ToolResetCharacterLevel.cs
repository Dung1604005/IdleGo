using UnityEditor;
using UnityEngine;

public class ToolResetCharacterLevel : EditorWindow
{
    private string statusMessage;
    private string displayedCharacterId;
    private MessageType statusType;

    [MenuItem("IdleGo/Tools/Reset Selected Hero Level")]
    public static void Open()
    {
        ToolResetCharacterLevel window = GetWindow<ToolResetCharacterLevel>();
        window.titleContent = new GUIContent("Reset Hero Level");
        window.minSize = new Vector2(400f, 250f);
        window.Show();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Reset Selected Hero Level", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select a hero in Inventory/Hero during Play Mode. Reset sets level to 1 "
            + "and EXP to 0. Stat points, purchased upgrades and global progress are kept.",
            MessageType.Info);

        if (!EditorApplication.isPlaying)
        {
            statusMessage = null;
            displayedCharacterId = null;
            EditorGUILayout.HelpBox("Enter Play Mode to use this tool.", MessageType.Info);
            return;
        }

        DrawSelectedHero(DataManager.Ins);
    }

    private void DrawSelectedHero(DataManager data)
    {
        string characterId = data.SelectedCharacterId;
        if (displayedCharacterId != characterId)
        {
            displayedCharacterId = characterId;
            statusMessage = null;
        }

        EditorGUILayout.LabelField("Hero", data.GetCharacterName(characterId));
        EditorGUILayout.LabelField("Character ID", characterId ?? "None");
        EditorGUILayout.LabelField("Level", data.GetCharacterStat(
            characterId, StatType.LEVEL).ToString("0"));
        EditorGUILayout.LabelField("EXP", data.GetCharacterStat(
            characterId, StatType.EXPERIENCE).ToString("0"));

        bool canReset = data.CanResetSelectedCharacterLevel(out string reason);
        using (new EditorGUI.DisabledScope(!canReset))
        {
            if (GUILayout.Button("Reset to Level 1 / 0 EXP", GUILayout.Height(32f)))
            {
                ResetSelectedHero(data);
            }
        }

        if (!canReset)
        {
            EditorGUILayout.HelpBox(reason, MessageType.Info);
        }
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, statusType);
        }
    }

    private void ResetSelectedHero(DataManager data)
    {
        bool changed = data.ResetSelectedCharacterLevel(out statusMessage);
        statusType = changed && data.InventoryData.LastSaveSucceeded
            ? MessageType.Info
            : MessageType.Warning;
    }
}
