using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DamagePopup : GameUnit
{
    [Header("References")]
    [SerializeField] private Text damageText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.yellow;

    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float growDuration = 0.12f;
    [SerializeField, Min(0f)] private float holdDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float shrinkDuration = 0.22f;
    [SerializeField, Min(0f)] private float riseDistance = 80f;
    [SerializeField, Range(0f, 1f)] private float startScale = 0.25f;
    [SerializeField, Min(1f)] private float peakScale = 1.15f;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Sequence animationSequence;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    public void Play(int damage, bool isCritical, Vector2 anchoredPosition)
    {
        if (damageText == null)
        {
            Debug.LogError("DamagePopup needs a UI Text reference.", this);
            SimplePool.Despawn(this);
            return;
        }

        startPosition = anchoredPosition;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = startPosition;
        damageText.text = damage.ToString();
        damageText.color = isCritical ? criticalColor : normalColor;
        damageText.raycastTarget = false;
        SetScale(startScale);

        KillAnimation();

        float totalDuration = growDuration + holdDuration + shrinkDuration;
        animationSequence = DOTween.Sequence()
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);

        animationSequence.Append(
            rectTransform.DOScale(peakScale, growDuration).SetEase(Ease.OutBack)
        );
        animationSequence.AppendInterval(holdDuration);
        animationSequence.Append(
            rectTransform.DOScale(0f, shrinkDuration).SetEase(Ease.InBack)
        );
        animationSequence.Join(damageText.DOFade(0f, shrinkDuration));
        animationSequence.Insert(
            0f,
            rectTransform.DOAnchorPosY(startPosition.y + riseDistance, totalDuration)
                .SetEase(Ease.OutQuad)
        );
        animationSequence.OnComplete(() =>
        {
            animationSequence = null;
            SimplePool.Despawn(this);
        });
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
