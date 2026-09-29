using UnityEditor;
using UnityEngine;

public class EnemyBalanceGeneratorWindow : EditorWindow
{
    private EnemyDataSO enemyData;
    private CharacterStatSO statData;
    private LevelDataSO levelData;
    private EnemyType enemyType = EnemyType.NORMAL;
    private int balanceLevel = 1;
    private float adjustment = 1f;
    private Vector2 scrollPosition;
    private string statusMessage;

    [MenuItem("Tools/IdleGo/Enemy Balance Generator")]
    public static void Open()
    {
        EnemyBalanceGeneratorWindow window = GetWindow<EnemyBalanceGeneratorWindow>();
        window.titleContent = new GUIContent("Enemy Balance");
        window.minSize = new Vector2(520f, 620f);
        window.Show();
    }

    private void OnEnable()
    {
        if (Selection.activeObject is EnemyDataSO selectedEnemy)
        {
            SetEnemyData(selectedEnemy);
        }
        else if (Selection.activeObject is CharacterStatSO selectedStats)
        {
            statData = selectedStats;
        }
        else if (Selection.activeObject is LevelDataSO selectedLevel)
        {
            levelData = selectedLevel;
            balanceLevel = selectedLevel.LevelRequired;
        }
    }

    private void OnGUI()
    {
        DrawHeader();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawTargetSection();
        DrawProgressionSection();
        EnemyBalancePreview preview = EnemyBalanceCalculator.Build(
            balanceLevel, enemyType, adjustment);
        DrawPreview(preview);
        DrawGenerateButton(preview);
        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        Rect rect = GUILayoutUtility.GetRect(0f, 72f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, new Color(0.35f, 0.12f, 0.12f));
        GUIStyle title = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            normal = { textColor = Color.white }
        };
        GUI.Label(rect, "ENEMY BALANCE GENERATOR", title);
        GUILayout.Space(8f);
    }

    private void DrawTargetSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("1. Target", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        EnemyDataSO selectedEnemy = (EnemyDataSO)EditorGUILayout.ObjectField(
            "Enemy Data", enemyData, typeof(EnemyDataSO), false);
        if (EditorGUI.EndChangeCheck())
        {
            SetEnemyData(selectedEnemy);
        }

        statData = (CharacterStatSO)EditorGUILayout.ObjectField(
            "Stat Asset", statData, typeof(CharacterStatSO), false);
        enemyType = (EnemyType)EditorGUILayout.EnumPopup("Enemy Type", enemyType);
        if (statData == null)
        {
            EditorGUILayout.HelpBox(
                "Choose a CharacterStatSO or an EnemyDataSO with a configured prefab.",
                MessageType.Warning);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawProgressionSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("2. Level and adjustment", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        LevelDataSO selectedLevel = (LevelDataSO)EditorGUILayout.ObjectField(
            "Level Data", levelData, typeof(LevelDataSO), false);
        if (EditorGUI.EndChangeCheck())
        {
            levelData = selectedLevel;
            if (levelData != null)
            {
                balanceLevel = levelData.LevelRequired;
            }
        }

        balanceLevel = Mathf.Max(1, EditorGUILayout.IntField("Balance Level", balanceLevel));
        adjustment = EditorGUILayout.Slider(
            "Stat Adjustment",
            adjustment,
            EnemyBalanceRules.MinAdjustment,
            EnemyBalanceRules.MaxAdjustment);
        DrawAdjustmentLabels();
        EditorGUILayout.HelpBox(
            "Adjustment scales Health, Damage, Armor and EXP. Speed and percentage stats "
            + "stay tied to Enemy Type to avoid reaching caps too early.",
            MessageType.Info);
        EditorGUILayout.EndVertical();
    }

    private void DrawAdjustmentLabels()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("0.5  Easy", EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("1.0  Standard", EditorStyles.centeredGreyMiniLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("2.0  Hard", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void SetEnemyData(EnemyDataSO value)
    {
        enemyData = value;
        statusMessage = null;
        if (enemyData == null)
        {
            return;
        }

        enemyType = enemyData.EnemyType;
        CharacterStatSO resolvedStats = ResolveStatData(enemyData);
        if (resolvedStats != null)
        {
            statData = resolvedStats;
        }
    }

    private static CharacterStatSO ResolveStatData(EnemyDataSO data)
    {
        if (data == null || data.EnemyPrefab == null)
        {
            return null;
        }

        // Doc reference serialized tren prefab, khong dung GetComponent hay FindByType.
        SerializedObject serializedEnemy = new SerializedObject(data.EnemyPrefab);
        SerializedProperty characterDataProperty =
            serializedEnemy.FindProperty("characterDataSO");
        CharacterDataSO characterData =
            characterDataProperty?.objectReferenceValue as CharacterDataSO;
        return characterData != null ? characterData.StatSO : null;
    }

    private void DrawPreview(EnemyBalancePreview preview)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("3. Preview", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            $"Level power x{preview.ProgressionMultiplier:0.###}   |   "
            + $"Type {enemyType}   |   Adjustment x{preview.Adjustment:0.00}",
            EditorStyles.miniBoldLabel);

        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            DrawStatRow((StatType)i, preview.GetStat((StatType)i));
        }
        EditorGUILayout.EndVertical();
    }

    private static void DrawStatRow(StatType statType, float value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(statType.ToString(), GUILayout.Width(220f));
        string formatted = EnemyBalanceRules.IsPercentage(statType)
            ? $"{value:P1}"
            : statType == StatType.CRITICAL_DAMAGE ? $"x{value:0.00}" : $"{value:0.##}";
        EditorGUILayout.LabelField(formatted, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGenerateButton(EnemyBalancePreview preview)
    {
        EditorGUILayout.Space(8f);
        EditorGUI.BeginDisabledGroup(statData == null);
        if (GUILayout.Button("GENERATE BASE STATS", GUILayout.Height(42f)))
        {
            EnemyBalanceCalculator.Apply(statData, enemyData, enemyType, preview);
            statusMessage = $"Updated {statData.name} at balance level {preview.Level}.";
        }
        EditorGUI.EndDisabledGroup();

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }
    }
}
