#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class HeroStatusPrototypeBuilder
{
    private const string StatIconAssetPath =
        "Assets/_GAME/Resources/Data/StatIconData.asset";
    [MenuItem("IdleGo/UI/Build Hero Status Prototype")]
    public static void Build()
    {
        EnsureFolders();
        GetOrCreateStatIconData();
        StatProgressLineUI statLine =
            HeroStatusLinePrefabBuilder.BuildStatLine();
        HeroStatusSkillSlotUI skillSlot =
            HeroStatusLinePrefabBuilder.BuildSkillSlot();
        HeroStatusCanvasPrefabBuilder.Build(statLine, skillSlot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Hero Status prototype was generated successfully.");
    }

    public static void BuildFromCommandLine()
    {
        Build();
    }

    private static void EnsureFolders()
    {
        CreateFolder("Assets/_GAME", "Editor");
        CreateFolder("Assets/_GAME/Resources", "Data");
    }

    private static void CreateFolder(string parent, string folderName)
    {
        string path = $"{parent}/{folderName}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static StatIconSO GetOrCreateStatIconData()
    {
        StatIconSO data = AssetDatabase.LoadAssetAtPath<StatIconSO>(
            StatIconAssetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<StatIconSO>();
            AssetDatabase.CreateAsset(data, StatIconAssetPath);
        }
        data.EnsureAllStats();
        EditorUtility.SetDirty(data);
        return data;
    }

}
#endif
