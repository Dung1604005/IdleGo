using UnityEngine;
using UnityEngine.UI;

public sealed class CanvasLevelSelect : UICanvas, IMapProgressView
{
    [Header("Map Selection")]
    [SerializeField] private Button previousMapButton;
    [SerializeField] private Button nextMapButton;

    [Header("Selected Map")]
    [SerializeField] private RectTransform mapViewContainer;

    [Header("Controls")]
    [SerializeField] private DifficultyPopupUI difficultyPopup;
    [SerializeField] private Button closeButton;
    [SerializeField] private MapSelectionRuntimeTestTool runtimeTestTool;

    private MapSelectUI activeMapView;
    private MapType displayedMapType = MapType.NONE;
    [System.NonSerialized] private MapType previewMapType;
    [System.NonSerialized] private DifficultType previewDifficulty;
    [System.NonSerialized] private int previewLevelIndex;

    [field: System.NonSerialized]
    public bool IsUnlockAllPreview { get; private set; }
    public MapType SelectedMapType => IsUnlockAllPreview
        ? previewMapType : DataManager.Ins.CurrentMapType;
    public DifficultType SelectedDifficulty => IsUnlockAllPreview
        ? previewDifficulty : DataManager.Ins.CurrentDifficulty;
    public int SelectedLevelIndex => IsUnlockAllPreview
        ? previewLevelIndex : DataManager.Ins.CurrentLevelIndex;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        OnDespawn();
        closeButton?.onClick.AddListener(OnButtonClose);
        previousMapButton?.onClick.AddListener(OnButtonPreviousMap);
        nextMapButton?.onClick.AddListener(OnButtonNextMap);
        difficultyPopup?.OnInit(this);
        MapManager.Ins.RegisterView(this);
        runtimeTestTool?.OnInit();
        RefreshMapProgress();
    }

    public void RefreshMapProgress()
    {
        RefreshMapNavigation();
        RefreshSelectedMap();
        difficultyPopup?.Refresh();
        runtimeTestTool?.Refresh();
    }

    public void OnMapRequested(MapType mapType)
    {
        if (!CanSelectMap(mapType)) return;
        if (IsUnlockAllPreview)
        {
            previewMapType = mapType;
            previewLevelIndex = 0;
            RefreshMapProgress();
            return;
        }
        // Real selections still follow the owner's save/refresh flow.
        DataManager.Ins.SelectMap(mapType);
    }

    public void OnDifficultyRequested(DifficultType difficulty)
    {
        if (!CanSelectDifficulty(difficulty)) return;
        if (IsUnlockAllPreview)
        {
            previewDifficulty = difficulty;
            RefreshMapProgress();
            return;
        }
        DataManager.Ins.SelectDifficulty(difficulty);
    }

    public void OnButtonPreviousMap() => MoveMap(-1);

    public void OnButtonNextMap() => MoveMap(1);

    public bool CanSelectDifficulty(DifficultType difficulty)
    {
        return IsUnlockAllPreview || DataManager.Ins.IsDifficultyUnlocked(difficulty);
    }

    public bool CanSelectMap(MapType mapType)
    {
        return DataManager.Ins.GetMapData(mapType) != null
            && (IsUnlockAllPreview
                || DataManager.Ins.IsMapUnlocked(SelectedDifficulty, mapType));
    }

    public bool CanSelectLevel(MapType mapType, int levelIndex)
    {
        MapDataSO map = DataManager.Ins.GetMapData(mapType);
        return map != null && levelIndex >= 0 && levelIndex < map.TotalLevel
            && (IsUnlockAllPreview || DataManager.Ins.IsLevelUnlocked(
                SelectedDifficulty, mapType, levelIndex));
    }

    public void OnLevelRequested(MapType mapType, int levelIndex)
    {
        if (mapType != SelectedMapType || !CanSelectLevel(mapType, levelIndex)) return;
        if (IsUnlockAllPreview)
        {
            // Preview input must never reach a loading/saving gameplay command.
            previewLevelIndex = levelIndex;
            RefreshMapProgress();
            return;
        }
        if (DataManager.Ins.SelectLevel(levelIndex)) CloseDirectly();
    }

    public void SetUnlockAllPreview(bool enabled)
    {
        if (!Application.isPlaying || IsUnlockAllPreview == enabled) return;
        previewMapType = DataManager.Ins.CurrentMapType;
        previewDifficulty = DataManager.Ins.CurrentDifficulty;
        previewLevelIndex = DataManager.Ins.CurrentLevelIndex;
        IsUnlockAllPreview = enabled;
        RefreshMapProgress();
    }

    public void OnButtonClose()
    {
        CloseDirectly();
        UIManager.Ins.OpenUI<CanvasInventoryHero>();
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
        previousMapButton?.onClick.RemoveListener(OnButtonPreviousMap);
        nextMapButton?.onClick.RemoveListener(OnButtonNextMap);
        difficultyPopup?.OnDespawn();
        runtimeTestTool?.OnDespawn();
        IsUnlockAllPreview = false;
        ClearMapView();
    }

    private void MoveMap(int direction)
    {
        MapType target = MapSelectionNavigation.GetAdjacentMap(SelectedMapType, direction);
        OnMapRequested(target);
    }

    private void RefreshMapNavigation()
    {
        if (previousMapButton != null) previousMapButton.interactable = CanSelectMap(
            MapSelectionNavigation.GetAdjacentMap(SelectedMapType, -1));
        if (nextMapButton != null) nextMapButton.interactable = CanSelectMap(
            MapSelectionNavigation.GetAdjacentMap(SelectedMapType, 1));
    }

    private void RefreshSelectedMap()
    {
        MapType currentMap = SelectedMapType;
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
        activeMapView.SetData(mapData, this);
        displayedMapType = currentMap;
    }

    private void ClearMapView()
    {
        if (activeMapView != null) SimplePool.Despawn(activeMapView);
        activeMapView = null;
        displayedMapType = MapType.NONE;
    }
}
