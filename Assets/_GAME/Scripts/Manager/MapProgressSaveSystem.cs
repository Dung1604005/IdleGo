using System;
using UnityEngine;

public static class MapProgressSaveSystem
{
    private const string DefaultSaveKey = "PLAYER_MAP_PROGRESS";

    public static bool TrySave(string saveKey, MapProgressSaveData saveData)
    {
        if (saveData == null) return false;
        try
        {
            PlayerPrefs.SetString(GetSaveKey(saveKey), JsonUtility.ToJson(saveData));
            PlayerPrefs.Save();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Cannot save map progress: {exception.Message}");
            return false;
        }
    }

    public static bool TryLoad(string saveKey, out MapProgressSaveData saveData)
    {
        saveData = null;
        try
        {
            string key = GetSaveKey(saveKey);
            if (!PlayerPrefs.HasKey(key)) return false;
            saveData = JsonUtility.FromJson<MapProgressSaveData>(
                PlayerPrefs.GetString(key));
            return saveData != null;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Cannot load map progress: {exception.Message}");
            return false;
        }
    }

    public static string GetSaveKey(string saveKey)
    {
        return string.IsNullOrWhiteSpace(saveKey)
            ? DefaultSaveKey
            : saveKey.Trim();
    }
}
