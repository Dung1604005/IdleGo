using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "IdleGo/Level/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    [SerializeField] private Sprite miniMap;

    [SerializeField] private MapController mapPrefab;

    [SerializeField] private List<LevelDataSO> listLevelData;
}
