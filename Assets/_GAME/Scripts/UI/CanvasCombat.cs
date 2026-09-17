using UnityEngine;

public class CanvasCombat : UICanvas
{
    [Header("Damage Popup")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField, Min(0)] private int preloadAmount = 15;
    [SerializeField, Min(0f)] private float spawnRadius = 0.5f;
    [SerializeField] private Camera worldCamera;
    [Tooltip("De trong neu Canvas la Screen Space - Overlay.")]
    [SerializeField] private Camera uiCamera;

    [SerializeField]private RectTransform canvasRect;

    private void OnEnable()
    {
        CacheWorldCamera();
        if (damagePopupPrefab != null)
        {
            SimplePool.PreLoad(damagePopupPrefab, preloadAmount, transform);
        }
    }

    private void OnDisable()
    {

        if (damagePopupPrefab != null)
        {
            SimplePool.Collect(damagePopupPrefab);
        }
    }

    public void ShowDamage(int damage, bool isCritical, Transform target)
    {
        if ( target == null || damage <= 0)
        {
            return;
        }

        SpawnDamagePopup(damage, isCritical, target.position);
    }

    private void SpawnDamagePopup(int damage, bool isCritical, Vector3 targetWorldPosition)
    {
        if (!CanSpawnDamagePopup())
        {
            return;
        }

        if (!TryGetPopupPosition(targetWorldPosition, out Vector2 popupPosition))
        {
            return;
        }

        CreateDamagePopup(damage, isCritical, popupPosition);
    }

    private bool CanSpawnDamagePopup()
    {
        if (damagePopupPrefab != null && canvasRect != null)
        {
            return true;
        }
        return false;
    }

    private bool TryGetPopupPosition(Vector3 targetWorldPosition, out Vector2 popupPosition)
    {
        CacheWorldCamera();

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

    private void CreateDamagePopup(int damage, bool isCritical, Vector2 popupPosition)
    {
        DamagePopup popup = SimplePool.Spawn(
            damagePopupPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );

        if (popup != null)
        {
            popup.Play(damage, isCritical, popupPosition);
        }
    }

    private void CacheWorldCamera()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }
    }
}
