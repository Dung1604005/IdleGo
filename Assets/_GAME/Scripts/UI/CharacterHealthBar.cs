using UnityEngine;

public class CharacterHealthBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer healthBarRenderer;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;

    [SerializeField]private Character character;
    private int lastHealth;
    private int lastMaxHealth;
    private float displayedRatio;
    private float targetRatio;
    private float smoothVelocity;

    public bool IsInitialized { get; private set; }

    public void OnInit()
    {
        if (healthBarRenderer == null)
        {
            Debug.LogError("CharacterHealthBar needs a Character and SpriteRenderer before OnInit().");
            return;
        }
        lastHealth = character.CurrentHealth;
        lastMaxHealth = character.MaxHealth;
        targetRatio = CalculateHealthRatio();
        displayedRatio = targetRatio;
        ApplyScale(displayedRatio);
        IsInitialized = true;
    }

    public void OnDespawn()
    {
        character = null;
        smoothVelocity = 0f;
        IsInitialized = false;
    }

    private void Update()
    {
        if (!IsInitialized || character == null)
        {
            return;
        }

        RefreshTargetRatio();
        SmoothScale();
    }

    private void RefreshTargetRatio()
    {
        int currentHealth = character.CurrentHealth;
        int maxHealth = character.MaxHealth;
        if (currentHealth == lastHealth && maxHealth == lastMaxHealth)
        {
            return;
        }

        // Chỉ đổi tỉ lệ đích khi máu hoặc máu tối đa thực sự thay đổi.
        lastHealth = currentHealth;
        lastMaxHealth = maxHealth;
        targetRatio = CalculateHealthRatio();
    }

    private void SmoothScale()
    {
        displayedRatio = Mathf.SmoothDamp(
            displayedRatio,
            targetRatio,
            ref smoothVelocity,
            smoothTime
        );

        // Chốt về giá trị chính xác khi đã đủ gần để tránh sai số kéo dài.
        if (Mathf.Abs(displayedRatio - targetRatio) <= 0.001f)
        {
            displayedRatio = targetRatio;
            smoothVelocity = 0f;
        }

        ApplyScale(displayedRatio);
    }

    private float CalculateHealthRatio()
    {
        if (character == null || character.MaxHealth <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01((float)character.CurrentHealth / character.MaxHealth);
    }

    private void ApplyScale(float ratio)
    {
        Transform barTransform = healthBarRenderer.transform;
        Vector3 localScale = barTransform.localScale;
        localScale.x = Mathf.Clamp01(ratio);
        barTransform.localScale = localScale;
    }
}
