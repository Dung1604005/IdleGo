using UnityEngine;

public class ChestAnimationEvent : MonoBehaviour
{
    [SerializeField] private ChestSlotUI chestSlotUI;

    public void OnInit()
    {
    }

    public void OnDespawn()
    {
    }

    // Animation Event tren ChestIcon goi ham nay tai frame item xuat hien.
    public void OnRevealReward()
    {
        chestSlotUI?.OnRevealReward();
    }

    // Animation Event cuoi clip goi ham nay de mo khoa ruong.
    public void OnOpenAnimationFinished()
    {
        chestSlotUI?.OnOpenAnimationFinished();
    }
}
