public interface IChestView
{
    void RefreshChests(ChestManager chestManager);
    bool PlayChestOpen(ChestType chestType, Equipment equipment);
}
