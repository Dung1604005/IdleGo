using UnityEngine;

public class ChestAnimationEvent : MonoBehaviour
{
    [SerializeField] private ChestSlotUI chestSlot;

    // Goi boi Animation Event tai frame cuoi cua clip mo ruong.
    public void OnOpenAnimationFinished()
    {
        chestSlot?.OnOpenAnimationFinished();
    }
}
