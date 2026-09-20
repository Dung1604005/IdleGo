using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    void Start()
    {
        UIManager.Ins.OpenUI<CanvasCombat>();
        LevelManager.Ins.OnInit();
    }
}
