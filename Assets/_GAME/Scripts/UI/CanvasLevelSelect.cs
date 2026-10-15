using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class CanvasLevelSelect : UICanvas, IMapProgressView
{
    [Header("Map Selection")]
    [SerializeField] private List<RectTransform> mapSelectionContents =
        new List<RectTransform>();
    [SerializeField] private MapSelectButtonUI mapButtonPrefab;

    [Header("Selected Map")]
    [SerializeField] private RectTransform mapViewContainer;

    [Header("Controls")]
    [SerializeField] private DifficultyPopupUI difficultyPopup;
    [SerializeField] private Button closeButton;

    private readonly List<MapSelectButtonUI> activeMapButtons =
        new List<MapSelectButtonUI>();
    private MapSelectUI activeMapView;
    private MapType displayedMapType = MapType.NONE;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        OnDespawn();
        closeButton?.onClick.AddListener(OnButtonClose);
        difficultyPopup?.OnInit(this);
        MapManager.Ins.RegisterView(this);
        CreateMapButtons();
        RefreshMapProgress();
    }

    public void RefreshMapProgress()
    {
        RefreshMapButtons();
        RefreshSelectedMap();
        difficultyPopup?.Refresh();
    }

    public void OnMapRequested(MapType mapType)
    {
        // Data owner saves and refreshes this canvas after a valid selection.
        DataManager.Ins.SelectMap(mapType);
    }

    public void OnDifficultyRequested(DifficultType difficulty)
    {
        DataManager.Ins.SelectDifficulty(difficulty);
    }

    public void OnButtonClose()
    {
        CloseDirectly();
    }

    public override void CloseDirectly()
    {
        OnDespawn();
        base.CloseDirectly();
    }

    public void OnDespawn()
    {
        MapManager.Ins.UnregisterView(this);
        closeButton?.onClick.RemoveListener(OnButtonClose);
        difficultyPopup?.OnDespawn();
        ClearMapButtons();
        ClearMapView();
    }

    private void CreateMapButtons()
    {
        IReadOnlyList<MapDataSO> maps = DataManager.Ins.GetMaps();
        if (mapButtonPrefab == null || mapSelectionContents.Count == 0) return;
        SimplePool.PreLoad(mapButtonPrefab, maps.Count, mapSelectionContents[0]);
        for (int i = 0; i < maps.Count; i++)
        {
            RectTransform content = mapSelectionContents[i % mapSelectionContents.Count];
            MapSelectButtonUI button = SimplePool.Spawn(
                mapButtonPrefab, Vector3.zero, Quaternion.identity, content);
            button.AttachTo(content);
            activeMapButtons.Add(button);
        }
    }

    private void RefreshMapButtons()
    {
        IReadOnlyList<MapDataSO> maps = DataManager.Ins.GetMaps();
        int count = Mathf.Min(maps.Count, activeMapButtons.Count);
        for (int i = 0; i < count; i++)
        {
            activeMapButtons[i].SetData(this, maps[i]);
        }
    }

    private void RefreshSelectedMap()
    {
        MapType currentMap = DataManager.Ins.CurrentMapType;
        if (activeMapView != null && displayedMapType == currentMap)
        {
            activeMapView.Refresh();
            return;
        }

        ClearMapView();
        MapDataSO mapData = DataManager.Ins.GetMapData(currentMap);
        MapSelectUI prefab = mapData != null ? mapData.MiniMapPrefab : null;
        if (prefab == null || mapViewContainer == null) return;
        activeMapView = SimplePool.Spawn(
            prefab, Vector3.zero, Quaternion.identity, mapViewContainer);
        activeMapView.AttachTo(mapViewContainer);
        activeMapView.SetData(mapData);
        displayedMapType = currentMap;
    }

    private void ClearMapButtons()
    {
        for (int i = 0; i < activeMapButtons.Count; i++)
        {
            SimplePool.Despawn(activeMapButtons[i]);
        }
        activeMapButtons.Clear();
    }

    private void ClearMapView()
    {
        if (activeMapView != null) SimplePool.Despawn(activeMapView);
        activeMapView = null;
        displayedMapType = MapType.NONE;
    }
}
