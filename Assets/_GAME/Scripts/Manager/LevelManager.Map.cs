public partial class LevelManager
{
    private string GetLevelAddress(MapType mapType, int levelIndex)
    {
        // Vi du mapIndex 0, levelIndex 1 tao address "Level_1-2".
        return $"{levelAddressPrefix}{(int)mapType + 1}-{levelIndex + 1}";
    }
}
