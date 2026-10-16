using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DifficultyPopupUI : MonoBehaviour
{
    [SerializeField] private Button popupButton;
    [SerializeField] private Image popupButtonImage;
    [SerializeField] private TextMeshProUGUI selectedDifficultyText;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private List<DifficultyOptionBinding> options =
        new List<DifficultyOptionBinding>();

    private CanvasLevelSelect owner;

    public void OnInit(CanvasLevelSelect sourceOwner)
    {
        OnDespawn();
        owner = sourceOwner;
        popupButton?.onClick.AddListener(TogglePopup);
        for (int i = 0; i < options.Count; i++)
        {
            options[i]?.OnInit(this);
        }
        if (popupPanel != null) popupPanel.SetActive(false);
        Refresh();
    }

    public void Refresh()
    {
        DifficultType current = owner != null
            ? owner.SelectedDifficulty : DataManager.Ins.CurrentDifficulty;
        RefreshButtonVisual(current);
        if (selectedDifficultyText != null)
        {
            selectedDifficultyText.text = GetDisplayName(current);
        }
        for (int i = 0; i < options.Count; i++)
        {
            options[i]?.Refresh(current);
        }
    }

    private void RefreshButtonVisual(DifficultType difficulty)
    {
        if (popupButton == null || popupButtonImage == null) return;
        Sprite normal = DataManager.Ins.GetDifficultyButtonNormal(difficulty);
        Sprite highlighted = DataManager.Ins.GetDifficultyButtonHighlighted(difficulty);
        if (normal == null && highlighted == null) return;
        highlighted = highlighted != null ? highlighted : normal;
        // Normal goes on the Image; hover/press/selection use the matching highlight.
        popupButtonImage.overrideSprite = null;
        if (normal != null) popupButtonImage.sprite = normal;
        popupButton.targetGraphic = popupButtonImage;
        SpriteState state = popupButton.spriteState;
        state.highlightedSprite = highlighted;
        state.pressedSprite = highlighted;
        state.selectedSprite = highlighted;
        popupButton.spriteState = state;
        popupButton.transition = Selectable.Transition.SpriteSwap;
    }

    public void RequestDifficulty(DifficultType difficulty)
    {
        owner?.OnDifficultyRequested(difficulty);
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    public void OnDespawn()
    {
        popupButton?.onClick.RemoveListener(TogglePopup);
        for (int i = 0; i < options.Count; i++)
        {
            options[i]?.OnDespawn();
        }
        owner = null;
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    private void TogglePopup()
    {
        if (popupPanel != null) popupPanel.SetActive(!popupPanel.activeSelf);
    }

    private bool CanSelectDifficulty(DifficultType difficulty)
    {
        return owner != null ? owner.CanSelectDifficulty(difficulty)
            : DataManager.Ins.IsDifficultyUnlocked(difficulty);
    }

    private static string GetDisplayName(DifficultType difficulty)
    {
        string value = difficulty.ToString().ToLowerInvariant();
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo
            .ToTitleCase(value);
    }

    [Serializable]
    private sealed class DifficultyOptionBinding
    {
        [SerializeField] private DifficultType difficulty;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject lockedIcon;
        [SerializeField] private GameObject selectedIcon;

        [NonSerialized] private DifficultyPopupUI owner;

        public void OnInit(DifficultyPopupUI sourceOwner)
        {
            owner = sourceOwner;
            button?.onClick.RemoveListener(OnPressed);
            button?.onClick.AddListener(OnPressed);
            if (label != null) label.text = GetDisplayName(difficulty);
        }

        public void Refresh(DifficultType current)
        {
            bool unlocked = owner != null
                ? owner.CanSelectDifficulty(difficulty)
                : DataManager.Ins.IsDifficultyUnlocked(difficulty);
            if (button != null) button.interactable = unlocked;
            if (lockedIcon != null) lockedIcon.SetActive(!unlocked);
            if (selectedIcon != null) selectedIcon.SetActive(difficulty == current);
        }

        public void OnDespawn()
        {
            button?.onClick.RemoveListener(OnPressed);
            owner = null;
        }

        private void OnPressed()
        {
            owner?.RequestDifficulty(difficulty);
        }
    }
}
