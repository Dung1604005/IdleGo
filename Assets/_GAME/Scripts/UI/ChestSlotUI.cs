using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestSlotUI : MonoBehaviour
{
    [SerializeField] private ChestType chestType;
    [SerializeField] private TextMeshProUGUI chestTypeText;
    [SerializeField] private TextMeshProUGUI queuedCountText;
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject fullIcon;
    [SerializeField] private GameObject openingBlocker;

    [Header("Open Effect")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openTrigger = "Open";
    [Tooltip("Dat Image nay phia tren hinh ruong trong Hierarchy.")]
    [SerializeField] private Image rewardIcon;
    [Tooltip("Dat Particle nay phia sau hinh ruong trong Hierarchy.")]
    [SerializeField] private ParticleSystem openLightParticle;

    private bool isPlayingOpenAnimation;

    public ChestType ChestType => chestType;

    public void SetVisible(bool visible)
    {
        if (gameObject.activeSelf != visible)
        {
            gameObject.SetActive(visible);
        }
    }

    public void OnInit()
    {
        isPlayingOpenAnimation = false;
        StopOpenVisual();
    }

    public void OnDespawn()
    {
        bool mustUnlock = isPlayingOpenAnimation;
        isPlayingOpenAnimation = false;
        StopOpenVisual();
        if (mustUnlock && ChestManager.Ins.IsInitialized)
        {
            ChestManager.Ins.CompleteOpenAnimation(chestType);
        }
    }

    public void Refresh(ChestState state)
    {
        if (state == null)
        {
            SetUnavailable();
            return;
        }

        if (chestTypeText != null)
        {
            chestTypeText.text = state.ChestType.ToString();
        }

        if (queuedCountText != null)
        {
            queuedCountText.text = $"x{state.Count}";
        }

        fullIcon?.SetActive(state.IsInventoryFull);
        openingBlocker?.SetActive(state.IsOpening);
        if (openButton != null)
        {
            openButton.interactable = state.CanOpen;
        }
    }

    public void OnButtonOpen()
    {
        if (!isPlayingOpenAnimation)
        {
            ChestManager.Ins.TryOpenChest(chestType);
        }
    }

    public bool PlayOpen(Equipment equipment)
    {
        if (isPlayingOpenAnimation
            || chestAnimator == null
            || string.IsNullOrWhiteSpace(openTrigger)
            || equipment?.Data == null)
        {
            return false;
        }

        isPlayingOpenAnimation = true;
        if (rewardIcon != null)
        {
            rewardIcon.sprite = equipment.Data.Icon;
            rewardIcon.enabled = rewardIcon.sprite != null;
        }

        openLightParticle?.Play();
        chestAnimator.ResetTrigger(openTrigger);
        chestAnimator.SetTrigger(openTrigger);
        return true;
    }

    // Gan ham nay vao Animation Event o frame cuoi cua clip mo ruong.
    public void OnOpenAnimationFinished()
    {
        if (!isPlayingOpenAnimation)
        {
            return;
        }

        isPlayingOpenAnimation = false;
        StopOpenVisual();
        ChestManager.Ins.CompleteOpenAnimation(chestType);
    }

    private void StopOpenVisual()
    {
        if (rewardIcon != null)
        {
            rewardIcon.sprite = null;
            rewardIcon.enabled = false;
        }

        openLightParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        openingBlocker?.SetActive(false);
    }

    private void SetUnavailable()
    {
        if (queuedCountText != null)
        {
            queuedCountText.text = "x0";
        }

        fullIcon?.SetActive(false);
        openingBlocker?.SetActive(false);
        if (openButton != null)
        {
            openButton.interactable = false;
        }
    }
}
