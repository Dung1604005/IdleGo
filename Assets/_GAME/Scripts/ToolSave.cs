using UnityEngine;

public class ToolSave : MonoBehaviour
{
    [SerializeField] private string inventorySaveKey = "PLAYER_INVENTORY";

    public void OnInit()
    {
    }

    public void OnDespawn()
    {
    }

    [ContextMenu("Delete Inventory And Equipment Data")]
    public void DeleteInventoryAndEquipmentData()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before deleting Inventory save. "
                + "Runtime data can save itself again when Play Mode ends.");
            return;
        }

        // Equipment dang mac cua tung nhan vat nam chung trong Inventory JSON.
        string saveKey = InventorySaveSystem.GetSaveKey(inventorySaveKey);
        bool hadSave = PlayerPrefs.HasKey(saveKey);
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();

        string result = hadSave ? "deleted" : "was already empty";
        Debug.Log($"Inventory and equipment data {result}. Key: {saveKey}");
    }
}
