using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "IdleGo/Level/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    [SerializeField] private MapType mapType;
    [FormerlySerializedAs("miniMap")]
    [SerializeField] private MapSelectUI miniMapPrefab;

    [SerializeField] private MapController mapPrefab;

    [SerializeField] private Vector3 spawnPos;

    [SerializeField, Min(1)] private int totalLevel = 1;

    public MapType MapType => mapType;

    public MapSelectUI MiniMapPrefab => miniMapPrefab;

    public Vector3 SpawnPos => spawnPos;

    public int TotalLevel => Mathf.Max(0, totalLevel);

    public MapController GetMapPrefab()
    {
        return mapPrefab;
    }
}


public enum MapType
{
    NONE = -1,
    FOREST = 0,

    LAVA = 1
}

public enum DifficultType
{
    EASY = 0,

    NORMAL = 1,

    HARD = 2,

    NIGHTMARE = 3,

    LEGEND = 4
}
