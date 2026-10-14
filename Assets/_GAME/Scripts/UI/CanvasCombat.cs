using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CanvasCombat : UICanvas
{
    [Header("Damage Popup")]
    [FormerlySerializedAs("damagePopupPrefab")]
    [SerializeField] private TextPopUp textPopupPrefab;
    [SerializeField, Min(0)] private int preloadAmount = 15;
    [SerializeField, Min(0f)] private float spawnRadius = 0.5f;
    [SerializeField] private Camera worldCamera;
    [Tooltip("De trong neu Canvas la Screen Space - Overlay.")]
    [SerializeField] private Camera uiCamera;

    [SerializeField]private RectTransform canvasRect;

    [Header("Level Result")]
    [SerializeField] private GameObject levelResultOverlay;
    [SerializeField] private TextMeshProUGUI levelResultText;
    [SerializeField, Min(0.01f)] private float resultScaleDuration = 0.4f;

    private Sequence levelResultSequence;
    private readonly TextPopUpPool textPopups = new TextPopUpPool();

    public override void SetUp()
    {
        base.SetUp();
        gameObject.SetActive(true);
        SetUpCamera();
        if (textPopupPrefab != null)
        {
            SimplePool.PreLoad(textPopupPrefab, preloadAmount, transform);
        }
        HideLevelResult();
    }

    public override void CloseDirectly()
    {
        HideLevelResult();
        base.CloseDirectly();
        textPopups.OnDespawn(transform);
    }

    public void OnLevelStart()
    {
        HideLevelResult();
    }

    public void OnLevelFail(float displayDuration)
    {
        PlayLevelResult("You Failed", displayDuration);
    }

    public void OnLevelComplete(float displayDuration)
    {
        PlayLevelResult("Level Completed", displayDuration);
    }

    private void PlayLevelResult(string message, float displayDuration)
    {
        KillLevelResultSequence();
        if (levelResultOverlay == null || levelResultText == null)
        {
            return;
        }

        // Damage popup duoc spawn dong, nen overlay luon phai nam tren cung khi hien ket qua.
        levelResultOverlay.transform.SetAsLastSibling();
        levelResultOverlay.SetActive(true);
        levelResultText.text = message;
        levelResultText.rectTransform.localScale = Vector3.zero;
        float totalDuration = Mathf.Max(0f, displayDuration);
        float scaleDuration = Mathf.Min(resultScaleDuration, totalDuration);
        levelResultSequence = DOTween.Sequence();
        if (scaleDuration > 0f)
        {
            levelResultSequence.Append(levelResultText.rectTransform
                .DOScale(1f, scaleDuration).SetEase(Ease.OutBack));
        }
        else
        {
            levelResultText.rectTransform.localScale = Vector3.one;
        }
        levelResultSequence.AppendInterval(
            Mathf.Max(0f, totalDuration - scaleDuration));
        levelResultSequence.OnComplete(CompleteLevelResultVisual);
    }

    private void CompleteLevelResultVisual()
    {
        levelResultSequence = null;
        SetLevelResultVisible(false);
    }

    private void HideLevelResult()
    {
        KillLevelResultSequence();
        SetLevelResultVisible(false);
    }

    private void KillLevelResultSequence()
    {
        levelResultSequence?.Kill();
        levelResultSequence = null;
    }

    private void SetLevelResultVisible(bool visible)
    {
        if (levelResultText != null)
        {
            levelResultText.rectTransform.localScale = Vector3.one;
        }
        levelResultOverlay?.SetActive(visible);
    }

    public void ShowDamage(int damage, bool isCritical, Transform target)
    {
        if ( target == null || damage <= 0)
        {
            return;
        }

        SpawnTextPopUp(damage, isCritical, target.position);
    }

    private void SpawnTextPopUp(int damage, bool isCritical, Vector3 targetWorldPosition)
    {
        if (!CanSpawnTextPopUp())
        {
            return;
        }

        if (!TryGetPopupPosition(targetWorldPosition, out Vector2 popupPosition))
        {
            return;
        }

        CreateTextPopUp(damage, isCritical, popupPosition);
    }

    private bool CanSpawnTextPopUp()
    {
        if (textPopupPrefab != null && canvasRect != null)
        {
            return true;
        }
        return false;
    }

    private bool TryGetPopupPosition(Vector3 targetWorldPosition, out Vector2 popupPosition)
    {

        // Random.insideUnitCircle phan bo popup trong mot hinh tron quanh enemy.
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 popupWorldPosition = targetWorldPosition + new Vector3(offset.x, offset.y, 0f);
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(worldCamera, popupWorldPosition);

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            uiCamera,
            out popupPosition
        );
    }

    private void CreateTextPopUp(int damage, bool isCritical, Vector2 popupPosition)
    {
        TextPopUp popup = textPopups.Spawn(textPopupPrefab, transform);

        if (popup != null)
        {
            popup.PlayDamage(damage, isCritical, popupPosition);
        }
    }

    private void SetUpCamera()
    {
        worldCamera = CameraManager.Ins.GetCamera(CameraType.MAIN);
    }
}
