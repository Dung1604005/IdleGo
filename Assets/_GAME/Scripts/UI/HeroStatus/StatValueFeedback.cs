using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

[Serializable]
public sealed class StatValueFeedback
{
    [SerializeField] private TextPopUp popupPrefab;

    [SerializeField] private Transform parentTF;
    [SerializeField, Range(1f, 1.5f)] private float peakScale = 1.28f;
    [SerializeField, Min(0.01f)] private float popDuration = 0.2f;
    [SerializeField, Min(0.01f)] private float tickerDuration = 0.15f;
    [SerializeField, Min(0.01f)] private float floatingDuration = 0.6f;
    [SerializeField, Min(0f)] private float floatingDistance = 40f;
    [SerializeField, Min(0f)] private float floatingOffset = 22f;
    [SerializeField, Range(0.1f, 1f)] private float floatingFontScale = 0.65f;

    [NonSerialized] private TextPopUpPool popupPool;
    private TextMeshProUGUI valueText;
    private Vector3 originalScale;
    private Color originalColor;
    private Sequence popSequence;
    private Tween ticker;
    private float displayedValue;
    private float targetValue;
    private bool hasValue;

    public void OnInit(TextMeshProUGUI text)
    {
        popupPool ??= new TextPopUpPool();
        parentTF = UIManager.Ins.GetUI<CanvasHeroStatus>().transform;
        valueText = text;
        if (text == null) return;
        originalScale = text.rectTransform.localScale;
        originalColor = text.color;
    }

    public void SetValue(StatType statType, float value, bool upgraded)
    {
        if (valueText == null) return;
        if (hasValue && Mathf.Approximately(targetValue, value)) return;
        float previousValue = targetValue;
        targetValue = value;
        if (!hasValue || !upgraded || value <= previousValue)
        {
            SnapValue(statType, value);
            return;
        }

        // Hieu ung chi chay khi refresh doc duoc upgrade da hoan tat, khong chay tu click.
        Color color = DataManager.Ins.GetStatFeedbackColor(statType);
        PlayPop(color);
        PlayTicker(statType, value);
        ShowIncrease(statType, value - previousValue, color);
    }

    public void OnDespawn()
    {
        KillTweens();
        if (valueText != null)
        {
            popupPool?.OnDespawn(parentTF);
            valueText.rectTransform.localScale = originalScale;
            valueText.color = originalColor;
        }
        hasValue = false;
    }

    private void SnapValue(StatType statType, float value)
    {
        KillTweens();
        popupPool?.OnDespawn(valueText.rectTransform);
        displayedValue = value;
        hasValue = true;
        valueText.rectTransform.localScale = originalScale;
        valueText.color = originalColor;
        valueText.text = HeroStatusText.FormatStat(statType, value);
    }

    private void PlayPop(Color color)
    {
        popSequence?.Kill();
        valueText.rectTransform.localScale = originalScale;
        valueText.color = color;
        popSequence = DOTween.Sequence().SetUpdate(true)
            .SetLink(valueText.gameObject, LinkBehaviour.KillOnDisable);
        popSequence.Append(valueText.rectTransform
            .DOScale(originalScale * peakScale, popDuration * 0.3f).SetEase(Ease.OutQuad));
        popSequence.Append(valueText.rectTransform
            .DOScale(originalScale, popDuration * 0.7f).SetEase(Ease.OutQuad));
        popSequence.Insert(0f, valueText.DOColor(originalColor, popDuration));
    }

    private void PlayTicker(StatType statType, float value)
    {
        ticker?.Kill();
        // Click nhanh tiep tuc tu so dang hien; delta popup van dung gia tri data thuc.
        ticker = DOTween.To(() => displayedValue, next =>
        {
            displayedValue = next;
            valueText.text = HeroStatusText.FormatStat(statType, next);
        }, value, tickerDuration).SetEase(Ease.OutQuad).SetUpdate(true)
            .SetLink(valueText.gameObject, LinkBehaviour.KillOnDisable)
            .OnComplete(() =>
            {
                displayedValue = value;
                valueText.text = HeroStatusText.FormatStat(statType, value);
                ticker = null;
            });
    }

    private void ShowIncrease(StatType statType, float delta, Color color)
    {
        popupPool ??= new TextPopUpPool();
        TextPopUp popup = popupPool.Spawn(popupPrefab, parentTF);
        if (popup == null) return;
        popup.transform.position = valueText.rectTransform.position;
        valueText.ForceMeshUpdate();
        Bounds bounds = valueText.textBounds;

        // Đổi vị trí phía trên chữ sang world, rồi sang local của parent popup.
        Vector3 worldPosition = valueText.rectTransform.TransformPoint(
            new Vector3(bounds.center.x, bounds.max.y + floatingOffset, 0f));

        RectTransform parentRect = (RectTransform)parentTF;
        Vector3 localPosition = parentRect.InverseTransformPoint(worldPosition);

        // Prepare dùng anchor giữa nên vị trí phải tính từ tâm rect của parent.
        Vector2 position = new Vector2(localPosition.x, localPosition.y)
            - parentRect.rect.center;
        popup.PlayFloating($"+{HeroStatusText.FormatStat(statType, delta)}", color,
            position, floatingDuration, floatingDistance,
            valueText.fontSize * floatingFontScale, valueText.font);
    }

    private void KillTweens()
    {
        popSequence?.Kill();
        ticker?.Kill();
        popSequence = null;
        ticker = null;
    }
}
