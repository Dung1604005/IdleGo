using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapSelectButtonUI : GameUnit
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private GameObject lockedIcon;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.82f, 0.3f);

    private CanvasLevelSelect owner;
    private MapType mapType = MapType.NONE;

    public void AttachTo(RectTransform parent)
    {
        RectTransform target = root != null ? root : TF as RectTransform;
        if (target == null || parent == null) return;
        target.SetParent(parent, false);
        target.localScale = Vector3.one;
    }

    public void SetData(CanvasLevelSelect sourceOwner, MapDataSO mapData)
    {
        owner = sourceOwner;
        mapType = mapData != null ? mapData.MapType : MapType.NONE;
        bool unlocked = mapData != null && DataManager.Ins.IsMapUnlocked(
            DataManager.Ins.CurrentDifficulty, mapType);
        bool selected = unlocked && mapType == DataManager.Ins.CurrentMapType;
        if (mapNameText != null) mapNameText.text = mapType.ToString();
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonSelectMap);
            button.onClick.AddListener(OnButtonSelectMap);
            button.interactable = unlocked;
        }
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
        if (lockedIcon != null) lockedIcon.SetActive(!unlocked);
    }

    public void OnButtonSelectMap()
    {
        owner?.OnMapRequested(mapType);
    }

    public override void OnDespawn()
    {
        button?.onClick.RemoveListener(OnButtonSelectMap);
        owner = null;
        mapType = MapType.NONE;
        if (backgroundImage != null) backgroundImage.color = normalColor;
        if (lockedIcon != null) lockedIcon.SetActive(false);
    }
}
