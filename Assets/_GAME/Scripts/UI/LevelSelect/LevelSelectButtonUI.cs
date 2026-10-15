using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class LevelSelectButtonUI : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Min(1)] private int levelNumber = 1;
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite pressedSprite;

    private MapType mapType = MapType.NONE;
    private bool isUnlocked;
    private bool isSelected;

    public int LevelIndex => Mathf.Max(0, levelNumber - 1);

    public void OnInit(MapType ownerMapType)
    {
        mapType = ownerMapType;
        if (button != null)
        {
            button.transition = Selectable.Transition.None;
            button.onClick.RemoveListener(OnButtonSelectLevel);
            button.onClick.AddListener(OnButtonSelectLevel);
        }
        Refresh();
    }

    public void Refresh()
    {
        DifficultType difficulty = DataManager.Ins.CurrentDifficulty;
        isUnlocked = DataManager.Ins.IsLevelUnlocked(
            difficulty, mapType, LevelIndex);
        isSelected = mapType == DataManager.Ins.CurrentMapType
            && LevelIndex == DataManager.Ins.CurrentLevelIndex;
        if (button != null) button.interactable = isUnlocked;
        if (levelText != null)
        {
            levelText.text = $"{(int)mapType + 1}-{levelNumber}";
        }
        ApplyCurrentSprite();
    }

    public void OnButtonSelectLevel()
    {
        if (!isUnlocked || mapType != DataManager.Ins.CurrentMapType) return;
        if (DataManager.Ins.SelectLevel(LevelIndex))
        {
            UIManager.Ins.CloseUIDirectly<CanvasLevelSelect>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isUnlocked && buttonImage != null && pressedSprite != null)
        {
            buttonImage.sprite = pressedSprite;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ApplyCurrentSprite();
    }

    public void OnDespawn()
    {
        button?.onClick.RemoveListener(OnButtonSelectLevel);
        mapType = MapType.NONE;
    }

    private void ApplyCurrentSprite()
    {
        if (buttonImage == null) return;
        Sprite target = !isUnlocked
            ? lockedSprite
            : isSelected && selectedSprite != null
                ? selectedSprite
                : normalSprite;
        if (target != null) buttonImage.sprite = target;
    }
}
