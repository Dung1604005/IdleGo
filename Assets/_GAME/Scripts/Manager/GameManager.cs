using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    void Start()
    {
        PlayerManager.Ins.OnInit();
        UIManager.Ins.OpenUI<CanvasCombat>();
        UIManager.Ins.OpenUI<CanvasInventoryHero>();
        LevelManager.Ins.OnInit();
    }
}
