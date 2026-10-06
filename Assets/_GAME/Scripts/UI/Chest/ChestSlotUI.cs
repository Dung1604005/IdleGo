using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestSlotUI : MonoBehaviour
{
    [SerializeField] private ChestType chestType;
    [SerializeField] private RectTransform slotRectTransform;
    [SerializeField] private TextMeshProUGUI queuedCountText;
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject fullIcon;

    [Header("Open Effect")]
    [SerializeField] private Animator chestAnimator;
    [Tooltip("Dat Image nay phia tren hinh ruong trong Hierarchy.")]
    [SerializeField] private Image rewardIcon;
    [Tooltip("Dat LootRevealEffect phia sau rewardIcon trong Hierarchy.")]
    [SerializeField] private Image lootEffect;
    [Tooltip("Bat len neu muon goi OnRevealReward bang Animation Event.")]
    [SerializeField] private bool revealAtAnimationEvent;

    private bool isPlayingOpenAnimation;
    private bool isInteractionLocked;
    private bool hasRevealedReward;
    private Equipment pendingEquipment;

    public ChestType ChestType => chestType;
    public RectTransform SlotRectTransform => slotRectTransform != null
        ? slotRectTransform
        : transform as RectTransform;
    public bool IsPlayingOpenAnimation => isPlayingOpenAnimation;
    public bool CanOpen
    {
        get
        {
            ChestState state = DataManager.Ins.ChestData?.GetState(chestType);
            return !isInteractionLocked
                && !isPlayingOpenAnimation
                && state != null
                && state.CanOpen;
        }
    }

    public void OnInit()
    {
        isPlayingOpenAnimation = false;
        isInteractionLocked = false;

        StopOpenVisual();
    }

    public void OnDespawn()
    {
        bool mustUnlock = isPlayingOpenAnimation;
        isPlayingOpenAnimation = false;
        isInteractionLocked = false;
        StopOpenVisual();
        ChestManager chestData = DataManager.Ins.ChestData;
        if (mustUnlock && chestData != null && chestData.IsInitialized)
        {
            chestData.CompleteOpenAnimation(chestType);
        }
    }

    public void Refresh(ChestState state)
    {
        if (state == null || state.ChestType != chestType)
        {
            SetUnavailable();
            return;
        }

        if (queuedCountText != null)
        {
            queuedCountText.text = $"x{state.Count}";
        }

        fullIcon?.SetActive(state.IsInventoryFull);
        RefreshOpenButton();
    }

    public void SetInteractionLocked(bool locked)
    {
        isInteractionLocked = locked;
        RefreshOpenButton();
    }

    public void SetCarouselActive(bool active)
    {
        if (gameObject.activeSelf != active)
        {
            gameObject.SetActive(active);
        }
    }

    public void SetCarouselPose(Vector2 anchoredPosition, float scale)
    {
        RectTransform target = SlotRectTransform;
        if (target == null)
        {
            return;
        }

        target.anchoredPosition = anchoredPosition;
        target.localScale = Vector3.one * Mathf.Max(0f, scale);
    }

    public void OnButtonOpen()
    {
        if (CanOpen)
        {
            DataManager.Ins.ChestData?.TryOpenChest(chestType);
        }
    }

    public bool PlayOpen(Equipment equipment)
    {
        if (isInteractionLocked
            || isPlayingOpenAnimation
            || chestAnimator == null
            || equipment?.Data == null)
        {
            return false;
        }

        isPlayingOpenAnimation = true;
        PrepareRewardReveal(equipment);
        chestAnimator.ResetTrigger(GameConfig.ANIM_OPEN_CHEST);
        chestAnimator.SetTrigger(GameConfig.ANIM_OPEN_CHEST);
        RefreshOpenButton();
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
        DataManager.Ins.ChestData?.CompleteOpenAnimation(chestType);
        RefreshOpenButton();
    }

    // Gan vao Animation Event tai frame item bat dau xuat hien.
    public void OnRevealReward()
    {
        if (!isPlayingOpenAnimation
            || hasRevealedReward
            || pendingEquipment?.Data == null)
        {
            return;
        }

        hasRevealedReward = true;
        ShowReward(pendingEquipment);
        PlayRevealEffect(pendingEquipment.RarityType);
    }

    private void PrepareRewardReveal(Equipment equipment)
    {
        StopOpenVisual();
        pendingEquipment = equipment;
        hasRevealedReward = false;
        if (!revealAtAnimationEvent)
        {
            OnRevealReward();
        }
    }

    private void ShowReward(Equipment equipment)
    {
        if (rewardIcon == null)
        {
            return;
        }




        rewardIcon.sprite = equipment.Data.Icon;
        rewardIcon.enabled = rewardIcon.sprite != null;
    }

    private void StopOpenVisual()
    {
        if (rewardIcon != null)
        {
            rewardIcon.sprite = null;
            rewardIcon.enabled = false;
        }

        if(lootEffect != null)
        {
            lootEffect.gameObject.SetActive(false);
        }

        
        
        pendingEquipment = null;
        hasRevealedReward = false;
    }

    private void PlayRevealEffect(RarityType rarityType)
    {
        

        if(lootEffect != null)
        {
            lootEffect.color = DataManager.Ins.GetRarityColor(rarityType);
            lootEffect.gameObject.SetActive(true);
        }
        
    }

    private void RefreshOpenButton()
    {
        if (openButton != null)
        {
            openButton.interactable = CanOpen;
        }
    }

    private void SetUnavailable()
    {
        if (queuedCountText != null)
        {
            queuedCountText.text = "x0";
        }

        fullIcon?.SetActive(false);
        if (openButton != null)
        {
            openButton.interactable = false;
        }
    }
}
