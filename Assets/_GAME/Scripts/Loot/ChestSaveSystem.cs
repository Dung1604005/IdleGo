using System;
using UnityEngine;

public static class ChestSaveSystem
{
    public static bool HasSave(string saveKey)
    {
        return PlayerPrefs.HasKey(NormalizeKey(saveKey));
    }

    public static bool TrySave(string saveKey, ChestManagerSaveData saveData)
    {
        if (saveData == null)
        {
            return false;
        }

        try
        {
            PlayerPrefs.SetString(NormalizeKey(saveKey), JsonUtility.ToJson(saveData));
            PlayerPrefs.Save();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Cannot save chests: {exception.Message}");
            return false;
        }
    }

    public static bool TryLoad(string saveKey, out ChestManagerSaveData saveData)
    {
        saveData = null;
        try
        {
            string key = NormalizeKey(saveKey);
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }

            saveData = JsonUtility.FromJson<ChestManagerSaveData>(
                PlayerPrefs.GetString(key));
            return saveData != null;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Cannot load chests: {exception.Message}");
            return false;
        }
    }

    private static string NormalizeKey(string saveKey)
    {
        return string.IsNullOrWhiteSpace(saveKey) ? "PLAYER_CHESTS" : saveKey.Trim();
    }
}
