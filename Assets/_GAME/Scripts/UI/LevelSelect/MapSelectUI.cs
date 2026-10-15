using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapSelectUI : GameUnit
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image nameMapBackground;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private List<LevelSelectButtonUI> levelButtons =
        new List<LevelSelectButtonUI>();

    private MapDataSO mapData;

    public override void OnSpawn()
    {
        mapData = null;
    }

    public void AttachTo(RectTransform parent)
    {
        RectTransform target = root != null ? root : TF as RectTransform;
        if (target == null || parent == null) return;
        target.SetParent(parent, false);
        target.anchorMin = Vector2.zero;
        target.anchorMax = Vector2.one;
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
        target.localScale = Vector3.one;
    }

    public void SetData(MapDataSO sourceMap)
    {
        mapData = sourceMap;
        for (int i = 0; i < levelButtons.Count; i++)
        {
            levelButtons[i]?.OnInit(sourceMap != null
                ? sourceMap.MapType
                : MapType.NONE);
        }
        Refresh();
    }

    public void Refresh()
    {
        if (mapData == null) return;
        DifficultType difficulty = DataManager.Ins.CurrentDifficulty;
        if (mapNameText != null) mapNameText.text = GetMapName(mapData.MapType);
        if (borderImage != null)
        {
            borderImage.sprite = DataManager.Ins.GetDifficultyBorder(difficulty);
        }
        if (nameMapBackground != null)
        {
            nameMapBackground.sprite =
                DataManager.Ins.GetDifficultyNameBackground(difficulty);
        }
        for (int i = 0; i < levelButtons.Count; i++)
        {
            levelButtons[i]?.Refresh();
        }
    }

    public override void OnDespawn()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            levelButtons[i]?.OnDespawn();
        }
        mapData = null;
    }

    private static string GetMapName(MapType mapType)
    {
        string value = mapType.ToString().Replace('_', ' ').ToLowerInvariant();
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo
            .ToTitleCase(value);
    }
}
