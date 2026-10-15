using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DifficultyPopupUI : MonoBehaviour
{
    [SerializeField] private Button popupButton;
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
        DifficultType current = DataManager.Ins.CurrentDifficulty;
        if (selectedDifficultyText != null)
        {
            selectedDifficultyText.text = GetDisplayName(current);
        }
        for (int i = 0; i < options.Count; i++)
        {
            options[i]?.Refresh(current);
        }
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
            bool unlocked = DataManager.Ins.IsDifficultyUnlocked(difficulty);
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
