using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;

    [SerializeField] private GameObject lockedEffect;
    

    public  void SetButton(
        bool visible,
        bool interactable)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
            button.interactable = interactable;
            lockedEffect.SetActive(!interactable);
        }
    }
}
