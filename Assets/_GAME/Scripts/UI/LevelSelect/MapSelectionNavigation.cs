using System.Collections.Generic;

public static class MapSelectionNavigation
{
    public static MapType GetAdjacentMap(MapType current, int direction)
    {
        IReadOnlyList<MapDataSO> maps = DataManager.Ins.GetMaps();
        MapType result = MapType.NONE;
        for (int i = 0; i < maps.Count; i++)
        {
            if (maps[i] == null || maps[i].MapType == MapType.NONE) continue;
            MapType candidate = maps[i].MapType;
            // Compare enum values so Inspector catalog order cannot reorder arrows.
            bool isNext = direction > 0 && (int)candidate > (int)current;
            bool isPrevious = direction < 0 && (int)candidate < (int)current;
            if ((isNext && (result == MapType.NONE || (int)candidate < (int)result))
                || (isPrevious && (result == MapType.NONE || (int)candidate > (int)result)))
            {
                result = candidate;
            }
        }
        return result;
    }
}
