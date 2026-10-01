using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ChestVisualData
{
    [SerializeField] private ChestType chestType;
    [SerializeField] private Sprite chestSprite;
    [SerializeField] private RuntimeAnimatorController animatorController;

    public ChestType ChestType => chestType;
    public Sprite ChestSprite => chestSprite;
    public RuntimeAnimatorController AnimatorController => animatorController;
}

public class ChestSlotUI : MonoBehaviour
{
    [SerializeField] private ChestType chestType;
    [SerializeField] private TextMeshProUGUI chestTypeText;
    [SerializeField] private TextMeshProUGUI queuedCountText;
    [SerializeField] private Button openButton;
    [SerializeField] private Image chestImage;
    [SerializeField] private GameObject fullIcon;
    [SerializeField] private GameObject openingBlocker;
    [SerializeField] private List<ChestVisualData> chestVisuals = new List<ChestVisualData>();

    [Header("Open Effect")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openTrigger = "Open";
    [Tooltip("Dat Image nay phia tren hinh ruong trong Hierarchy.")]
    [SerializeField] private Image rewardIcon;
    [Tooltip("Dat Particle nay phia sau hinh ruong trong Hierarchy.")]
    [SerializeField] private ParticleSystem openLightParticle;

    private bool isPlayingOpenAnimation;

    public ChestType ChestType => chestType;

    public void SetChestType(ChestType type)
    {
        if (isPlayingOpenAnimation)
        {
            return;
        }

        chestType = type;
        ApplyChestVisual();
    }

    public void OnInit()
    {
        isPlayingOpenAnimation = false;
        StopOpenVisual();
        ApplyChestVisual();
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

        if (state.ChestType != chestType)
        {
            SetChestType(state.ChestType);
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

    private void ApplyChestVisual()
    {
        for (int i = 0; i < chestVisuals.Count; i++)
        {
            ChestVisualData visual = chestVisuals[i];
            if (visual == null || visual.ChestType != chestType)
            {
                continue;
            }

            if (chestImage != null)
            {
                chestImage.sprite = visual.ChestSprite;
                chestImage.enabled = chestImage.sprite != null;
            }
            if (chestAnimator != null && visual.AnimatorController != null)
            {
                chestAnimator.runtimeAnimatorController = visual.AnimatorController;
            }
            return;
        }
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
