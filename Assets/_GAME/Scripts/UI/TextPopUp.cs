using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(RectTransform))]
public class TextPopUp : GameUnit
{
    [Header("References")]
    [FormerlySerializedAs("damageText")]
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private RectTransform rectTransform;

    [Header("Damage Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.yellow;

    [Header("Burst Animation")]
    [SerializeField, Min(0.01f)] private float growDuration = 0.12f;
    [SerializeField, Min(0f)] private float holdDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float shrinkDuration = 0.22f;
    [SerializeField, Min(0f)] private float riseDistance = 80f;
    [SerializeField, Range(0f, 1f)] private float startScale = 0.25f;
    [FormerlySerializedAs("peakScale")]
    [SerializeField, Min(1f)] private float normalPeakScale = 1.15f;
    [SerializeField, Min(1f)] private float criticalPeakScale = 1.45f;

    private Sequence animationSequence;
    private Transform poolParent;
    private TMP_FontAsset defaultFont;
    private float defaultFontSize;
    private Vector2 defaultSize;
    private bool defaultAutoSize;
    private bool defaultMaskable;
    private bool hasDefaults;

    public void PlayDamage(int damage, bool isCritical, Vector2 position)
    {
        Play(damage.ToString(), isCritical ? criticalColor : normalColor,
            position, isCritical);
    }

    public void Play(string value, Color color, Vector2 position, bool emphasized = false)
    {
        if (!Prepare(value, color, position)) return;
        rectTransform.localScale = Vector3.one * startScale;
        float peak = emphasized
            ? Mathf.Max(criticalPeakScale, normalPeakScale + 0.01f)
            : Mathf.Max(1f, normalPeakScale);
        animationSequence = CreateSequence();
        animationSequence.Append(rectTransform.DOScale(peak, growDuration)
            .SetEase(Ease.OutBack));
        animationSequence.AppendInterval(holdDuration);
        animationSequence.Append(rectTransform.DOScale(0f, shrinkDuration)
            .SetEase(Ease.InBack));
        animationSequence.Join(popupText.DOFade(0f, shrinkDuration));
        AddRise(position, riseDistance, growDuration + holdDuration + shrinkDuration);
    }

    public void PlayFloating(string value, Color color, Vector2 position,
        float duration, float distance, float fontSize, TMP_FontAsset font = null)
    {
        if (!Prepare(value, color, position)) return;
        if (font != null) popupText.font = font;
        popupText.maskable = false;
        popupText.enableAutoSizing = false;
        popupText.fontSize = Mathf.Max(1f, fontSize);
        rectTransform.sizeDelta = new Vector2(300f, popupText.fontSize * 2f);
        duration = Mathf.Max(0.01f, duration);
        animationSequence = CreateSequence().SetUpdate(true);
        animationSequence.AppendInterval(duration * 0.2f);
        animationSequence.Append(popupText.DOFade(0f, duration * 0.8f)
            .SetEase(Ease.InQuad));
        AddRise(position, distance, duration);
    }

    public override void OnSpawn()
    {
        OnInit();
    }

    public void OnInit()
    {
        KillAnimation();
        CacheDefaults();
        RestoreDefaults();
    }

    public override void OnDespawn()
    {
        KillAnimation();
        RestoreDefaults();
        // Tra ve parent cua pool de popup khong mac ket trong mot stat line da despawn.
        TF.SetParent(poolParent, false);
    }

    private bool Prepare(string value, Color color, Vector2 position)
    {
        if (popupText == null || rectTransform == null)
        {
            Debug.LogError("TextPopUp needs TMP text and RectTransform references.", this);
            SimplePool.Despawn(this);
            return false;
        }
        KillAnimation();
        RestoreDefaults();
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        popupText.text = value;
        popupText.color = color;
        popupText.raycastTarget = false;
        return true;
    }

    private void CacheDefaults()
    {
        if (hasDefaults || popupText == null || rectTransform == null) return;
        poolParent = TF.parent;
        defaultFont = popupText.font;
        defaultFontSize = popupText.fontSize;
        defaultAutoSize = popupText.enableAutoSizing;
        defaultMaskable = popupText.maskable;
        defaultSize = rectTransform.sizeDelta;
        hasDefaults = true;
    }

    private void RestoreDefaults()
    {
        if (!hasDefaults) return;
        popupText.font = defaultFont;
        popupText.fontSize = defaultFontSize;
        popupText.enableAutoSizing = defaultAutoSize;
        popupText.maskable = defaultMaskable;
        popupText.color = Color.white;
        popupText.text = string.Empty;
        rectTransform.sizeDelta = defaultSize;
        rectTransform.localScale = Vector3.one;
    }

    private Sequence CreateSequence()
    {
        return DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable)
            .OnComplete(CompleteAnimation);
    }

    private void AddRise(Vector2 position, float distance, float duration)
    {
        animationSequence.Insert(0f, rectTransform
            .DOAnchorPosY(position.y + Mathf.Max(0f, distance), duration)
            .SetEase(Ease.OutQuad));
    }

    private void CompleteAnimation()
    {
        animationSequence = null;
        SimplePool.Despawn(this);
    }

    private void KillAnimation()
    {
        animationSequence?.Kill();
        animationSequence = null;
    }
}
