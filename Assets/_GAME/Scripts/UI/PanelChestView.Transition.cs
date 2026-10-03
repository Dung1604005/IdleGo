using System;
using DG.Tweening;
using UnityEngine;

public partial class PanelChestView
{
    private bool isTransitioning;
    private int transitionDirection;
    private ChestType targetChestType;
    private ChestSlotUI outgoingSlot;
    private ChestSlotUI incomingSlot;
    private Sequence transitionSequence;

    private void TryStartTransition(int direction)
    {
        if (!CanNavigate)
        {
            return;
        }

        ChestType nextType = GetRelativeChestType(direction);
        ChestSlotUI current = GetSlot(selectedChestType);
        ChestSlotUI next = GetSlot(nextType);
        if (!CanAnimateTransition(current, next))
        {
            return;
        }

        BeginTransition(direction, nextType, current, next);
    }

    private void BeginTransition(
        int direction,
        ChestType nextType,
        ChestSlotUI current,
        ChestSlotUI next)
    {
        isTransitioning = true;
        transitionDirection = direction < 0 ? -1 : 1;
        targetChestType = nextType;
        outgoingSlot = current;
        incomingSlot = next;

        LockAllSlots();
        outgoingSlot.SetCarouselPose(centerPosition, 1f);
        incomingSlot.SetCarouselActive(true);
        incomingSlot.SetCarouselPose(GetIncomingStartPosition(), sideScale);
        RefreshNavigation(ChestManager.Ins);
        CreateTransitionSequence();
    }

    private void CreateTransitionSequence()
    {
        float duration = Mathf.Max(0.05f, transitionDuration);
        Vector2 outgoingEnd = centerPosition
            + Vector2.left * transitionDirection * horizontalSlideDistance;
        RectTransform outgoingRect = outgoingSlot.SlotRectTransform;
        RectTransform incomingRect = incomingSlot.SlotRectTransform;

        // Bon tween chay dong thoi de hai ruong doi cho trong cung mot nhip.
        transitionSequence = DOTween.Sequence()
            .SetUpdate(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        transitionSequence.Append(
            outgoingRect.DOAnchorPos(outgoingEnd, duration).SetEase(Ease.InOutSine));
        transitionSequence.Join(
            outgoingRect.DOScale(sideScale, duration).SetEase(Ease.InOutSine));
        transitionSequence.Join(
            incomingRect.DOAnchorPos(centerPosition, duration).SetEase(Ease.InOutSine));
        transitionSequence.Join(
            incomingRect.DOScale(1f, duration).SetEase(Ease.InOutSine));
        transitionSequence.OnComplete(CompleteTransition);
    }

    private void CompleteTransition()
    {
        transitionSequence = null;
        outgoingSlot.SetCarouselActive(false);
        outgoingSlot.SetCarouselPose(centerPosition, 1f);
        incomingSlot.SetCarouselPose(centerPosition, 1f);
        selectedChestType = targetChestType;

        ClearTransitionState();
        UnlockSelectedSlot();
        RefreshChests(ChestManager.Ins);
    }

    private void ResetCarousel()
    {
        ChestSlotUI selectedSlot = GetSlot(selectedChestType);
        centerPosition = selectedSlot?.SlotRectTransform != null
            ? selectedSlot.SlotRectTransform.anchoredPosition
            : Vector2.zero;

        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            if (slot == null)
            {
                continue;
            }
            bool isSelected = slot.ChestType == selectedChestType;
            slot.SetCarouselPose(centerPosition, 1f);
            slot.SetCarouselActive(isSelected);
            slot.SetInteractionLocked(!isSelected);
        }
    }

    private void CancelTransition()
    {
        if (!isTransitioning)
        {
            return;
        }

        KillTransition();
        ClearTransitionState();
        ResetCarousel();
    }

    private void KillTransition()
    {
        if (transitionSequence != null && transitionSequence.IsActive())
        {
            transitionSequence.Kill();
        }
        transitionSequence = null;
    }

    private void ClearTransitionState()
    {
        isTransitioning = false;
        outgoingSlot = null;
        incomingSlot = null;
    }

    private void LockAllSlots()
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            chestSlots[i]?.SetInteractionLocked(true);
        }
    }

    private void UnlockSelectedSlot()
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            ChestSlotUI slot = chestSlots[i];
            slot?.SetInteractionLocked(slot.ChestType != selectedChestType);
        }
    }

    private bool CanAnimateTransition(ChestSlotUI current, ChestSlotUI next)
    {
        return current?.SlotRectTransform != null
            && next?.SlotRectTransform != null
            && current != next;
    }

    private Vector2 GetIncomingStartPosition()
    {
        return centerPosition
            + Vector2.right * transitionDirection * horizontalSlideDistance;
    }

    private ChestType GetRelativeChestType(int offset)
    {
        int index = Array.IndexOf(orderedChestTypes, selectedChestType);
        index = index < 0 ? 0 : index;
        int targetIndex = (index + offset + orderedChestTypes.Length)
            % orderedChestTypes.Length;
        return orderedChestTypes[targetIndex];
    }
}
