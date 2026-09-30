using System;
using UnityEngine;

public static class InventorySaveSystem
{
    public static bool HasSave(string saveKey)
    {
        return PlayerPrefs.HasKey(GetSaveKey(saveKey));
    }

    public static bool TrySave(string saveKey, InventorySaveData saveData)
    {
        if (saveData == null)
        {
            return false;
        }

        try
        {
            string normalizedSaveKey = GetSaveKey(saveKey);
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(normalizedSaveKey, json);
            PlayerPrefs.Save();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Cannot save inventory: {exception.Message}");
            return false;
        }
    }

    public static bool TryLoad(string saveKey, out InventorySaveData saveData)
    {
        saveData = null;
        try
        {
            string normalizedSaveKey = GetSaveKey(saveKey);
            if (!PlayerPrefs.HasKey(normalizedSaveKey))
            {
                return false;
            }

            string json = PlayerPrefs.GetString(normalizedSaveKey);
            saveData = JsonUtility.FromJson<InventorySaveData>(json);
            return saveData != null;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Cannot load inventory: {exception.Message}");
            return false;
        }
    }

    public static string GetSaveKey(string saveKey)
    {
        if (string.IsNullOrWhiteSpace(saveKey))
        {
            return "PLAYER_INVENTORY";
        }

        return saveKey.Trim();
    }
}
