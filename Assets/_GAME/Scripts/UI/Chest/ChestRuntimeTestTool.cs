using TMPro;
using UnityEngine;

public class ChestRuntimeTestTool : MonoBehaviour
{
    [SerializeField] private PanelChestView panelChestView;
    [SerializeField] private GameObject toolRoot;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private bool availableInReleaseBuild;

    public bool IsAvailable => Application.isEditor
        || Debug.isDebugBuild
        || availableInReleaseBuild;

    public bool CanAddSelectedChest
    {
        get
        {
            if (!IsAvailable || panelChestView == null)
            {
                return false;
            }

            ChestManager chestData = DataManager.Ins.ChestData;
            ChestState state = chestData?.GetState(panelChestView.SelectedChestType);
            return LootManager.Ins.IsInitialized
                && chestData != null
                && chestData.IsInitialized
                && state != null
                && !state.IsStorageFull;
        }
    }

    public void OnInit()
    {
        toolRoot?.SetActive(IsAvailable);
        SetResult(string.Empty);
    }

    public void OnDespawn()
    {
        SetResult(string.Empty);
    }

    public void OnButtonAddSelectedChest()
    {
        if (!CanAddSelectedChest)
        {
            SetResult("Queue chest da day hoac manager chua san sang.");
            return;
        }

        ChestType chestType = panelChestView.SelectedChestType;
        if (!LootManager.Ins.TryCreateTestReward(chestType, out ChestReward reward))
        {
            SetResult($"Level hien tai khong co source {chestType} hop le.");
            return;
        }

        ChestManager chestData = DataManager.Ins.ChestData;
        if (chestData == null || !chestData.TryEnqueue(reward))
        {
            SetResult($"Queue {chestType} da day.");
            return;
        }

        SetResult($"Da them 1 {chestType} chest.");
    }

    private void SetResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }
    }
}
