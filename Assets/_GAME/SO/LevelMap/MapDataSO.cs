using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "IdleGo/Level/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    [SerializeField] private MapType mapType;
    [SerializeField] private Sprite miniMap;

    [SerializeField] private MapController mapPrefab;

    [SerializeField] private int totalLevel;

    public MapType MapType => mapType;

    public int TotalLevel => totalLevel;

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