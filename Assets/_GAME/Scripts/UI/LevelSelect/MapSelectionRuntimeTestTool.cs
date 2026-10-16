using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapSelectionRuntimeTestTool : MonoBehaviour
{
    [SerializeField] private CanvasLevelSelect levelSelectCanvas;
    [SerializeField] private Button toggleButton;
    [SerializeField] private TextMeshProUGUI statusText;

    public bool IsAvailable => Application.isPlaying
        && (Application.isEditor || Debug.isDebugBuild);
    public bool IsUnlocked => levelSelectCanvas != null
        && levelSelectCanvas.IsUnlockAllPreview;

    public void OnInit()
    {
        toggleButton?.onClick.RemoveListener(ToggleUnlockAll);
        toggleButton?.onClick.AddListener(ToggleUnlockAll);
        if (toggleButton != null) toggleButton.gameObject.SetActive(IsAvailable);
        Refresh();
    }

    public void Refresh()
    {
        if (statusText != null)
        {
            statusText.text = IsUnlocked ? "Unlock All UI: ON" : "Unlock All UI: OFF";
        }
    }

    public void OnDespawn()
    {
        toggleButton?.onClick.RemoveListener(ToggleUnlockAll);
    }

    [ContextMenu("Toggle Unlock All UI (Play Mode Only)")]
    public void ToggleUnlockAll()
    {
        SetUnlockAll(!IsUnlocked);
    }

    [ContextMenu("Enable Unlock All UI (Play Mode Only)")]
    public void EnableUnlockAll() => SetUnlockAll(true);

    [ContextMenu("Disable Unlock All UI (Play Mode Only)")]
    public void DisableUnlockAll() => SetUnlockAll(false);

    private void SetUnlockAll(bool enabled)
    {
        if (!IsAvailable || levelSelectCanvas == null) return;
        // This calls the view only: no manager mutations, load commands or save writes.
        levelSelectCanvas.SetUnlockAllPreview(enabled);
        Refresh();
    }
}
