using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    void Start()
    {
        PlayerManager.Ins.OnInit();
        LootManager.Ins.OnInit();
        UIManager.Ins.OpenUI<CanvasCombat>();
        UIManager.Ins.OpenUI<CanvasInventoryHero>();
        LevelManager.Ins.OnInit();
    }
}
