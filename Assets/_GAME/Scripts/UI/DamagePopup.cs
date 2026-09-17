using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DamagePopup : GameUnit
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI damageText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.yellow;

    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float growDuration = 0.12f;
    [SerializeField, Min(0f)] private float holdDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float shrinkDuration = 0.22f;
    [SerializeField, Min(0f)] private float riseDistance = 80f;
    [SerializeField, Range(0f, 1f)] private float startScale = 0.25f;
    [FormerlySerializedAs("peakScale")]
    [SerializeField, Min(1f)] private float normalPeakScale = 1.15f;
    [SerializeField, Min(1f)] private float criticalPeakScale = 1.45f;

    [SerializeField] private RectTransform rectTransform;
    private Vector2 startPosition;
    private Sequence animationSequence;

    public void Play(int damage, bool isCritical, Vector2 anchoredPosition)
    {
        if (!HasRequiredReferences())
        {
            SimplePool.Despawn(this);
            return;
        }

        KillAnimation();
        PreparePopup(damage, isCritical, anchoredPosition);
        CreateAnimation(isCritical);
    }

    private bool HasRequiredReferences()
    {
        if (damageText != null && rectTransform != null)
        {
            return true;
        }

        Debug.LogError("DamagePopup needs TextMeshProUGUI and RectTransform references.", this);
        return false;
    }

    private void PreparePopup(int damage, bool isCritical, Vector2 anchoredPosition)
    {
        startPosition = anchoredPosition;
        SetTransformDefaults();
        SetText(damage, isCritical);
        SetScale(startScale);
    }

    private void SetTransformDefaults()
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = startPosition;
    }

    private void SetText(int damage, bool isCritical)
    {
        damageText.text = damage.ToString();
        damageText.color = isCritical ? criticalColor : normalColor;
        damageText.raycastTarget = false;
    }

    private void CreateAnimation(bool isCritical)
    {
        float totalDuration = growDuration + holdDuration + shrinkDuration;
        animationSequence = DOTween.Sequence()
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);

        AddScaleAnimation(GetPeakScale(isCritical));
        AddRiseAnimation(totalDuration);
        animationSequence.OnComplete(CompleteAnimation);
    }

    private void AddScaleAnimation(float peakScale)
    {
        animationSequence.Append(
            rectTransform.DOScale(peakScale, growDuration).SetEase(Ease.OutBack)
        );
        animationSequence.AppendInterval(holdDuration);
        animationSequence.Append(
            rectTransform.DOScale(0f, shrinkDuration).SetEase(Ease.InBack)
        );
        animationSequence.Join(damageText.DOFade(0f, shrinkDuration));
    }

    private void AddRiseAnimation(float totalDuration)
    {
        animationSequence.Insert(
            0f,
            rectTransform.DOAnchorPosY(startPosition.y + riseDistance, totalDuration)
                .SetEase(Ease.OutQuad)
        );
    }

    private float GetPeakScale(bool isCritical)
    {
        if (!isCritical)
        {
            return Mathf.Max(1f, normalPeakScale);
        }

        // Crit luôn phải lớn hơn đòn thường dù giá trị Inspector bị cấu hình thấp hơn.
        return Mathf.Max(criticalPeakScale, normalPeakScale + 0.01f);
    }

    private void CompleteAnimation()
    {
        animationSequence = null;
        SimplePool.Despawn(this);
    }

    public override void OnSpawn()
    {
        KillAnimation();
    }

    public override void OnDespawn()
    {
        KillAnimation();
    }

    private void SetScale(float scale)
    {
        rectTransform.localScale = Vector3.one * Mathf.Max(0f, scale);
    }

    private void KillAnimation()
    {
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        animationSequence = null;
    }
}
