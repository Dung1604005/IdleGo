using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestTypeTabUI : MonoBehaviour
{
    [SerializeField] private ChestType chestType;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject selectedIcon;
    [SerializeField] private GameObject newChestIcon;
    [SerializeField] private TextMeshProUGUI queuedCountText;

    private PanelChestView owner;
    private int lastSeenReceivedVersion;

    public ChestType ChestType => chestType;

    public void OnInit(PanelChestView panel)
    {
        owner = panel;
        selectedIcon?.SetActive(false);
        newChestIcon?.SetActive(false);
    }

    public void OnDespawn()
    {
        owner = null;
    }

    public void Refresh(ChestState state, bool isSelected, bool selectionLocked)
    {
        if (state == null)
        {
            SetUnavailable();
            return;
        }

        // Manager khoi tao lai se dua version ve 0, UI cung phai bo moc cu.
        if (state.ReceivedVersion < lastSeenReceivedVersion)
        {
            lastSeenReceivedVersion = 0;
        }

        if (isSelected)
        {
            lastSeenReceivedVersion = state.ReceivedVersion;
        }

        bool hasNewChest = !isSelected
            && state.Count > 0
            && state.ReceivedVersion > lastSeenReceivedVersion;
        selectedIcon?.SetActive(isSelected);
        newChestIcon?.SetActive(hasNewChest);
        if (queuedCountText != null)
        {
            queuedCountText.text = state.Count.ToString();
        }

        if (selectButton != null)
        {
            selectButton.interactable = !isSelected && !selectionLocked;
        }
    }

    public void OnButtonSelect()
    {
        owner?.SelectChestType(chestType);
    }

    private void SetUnavailable()
    {
        selectedIcon?.SetActive(false);
        newChestIcon?.SetActive(false);
        if (queuedCountText != null)
        {
            queuedCountText.text = "0";
        }

        if (selectButton != null)
        {
            selectButton.interactable = false;
        }
    }
}
