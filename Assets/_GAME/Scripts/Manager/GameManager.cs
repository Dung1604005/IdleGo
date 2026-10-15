using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    void Start()
    {
        PlayerManager.Ins.OnInit();
        ChestManager.Ins.OnInit();
        LootManager.Ins.OnInit();
        // Map progress phải load trước để các canvas có thể đọc selection hiện tại.
        MapManager.Ins.OnInit();
        UIManager.Ins.OpenUI<CanvasCombat>();
        UIManager.Ins.OpenUI<CanvasInventoryHero>();
        UIManager.Ins.OpenUI<CanvasNavigation>();
        LevelManager.Ins.OnInit();
    }
}
