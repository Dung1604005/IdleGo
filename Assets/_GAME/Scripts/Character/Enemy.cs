using UnityEngine;
using System.Collections.Generic;

public class Enemy : Character
{
    [Header("Target Sorting")]
    [SerializeField] private List<SpriteRenderer> targetSortingRenderers = new List<SpriteRenderer>();
    [SerializeField, Min(1)] private int targetSortingOrderIncrease = 100;

    private int[] originalSortingOrders;
    private bool isPlayerTarget;

    public override void OnInit()
    {
        // Pool có thể tái sử dụng enemy, vì vậy phải bỏ trạng thái target cũ trước khi lưu order gốc.
        RestoreOriginalSortingOrders();
        base.OnInit();
        CacheOriginalSortingOrders();
    }

    public override void OnDespawn()
    {
        RestoreOriginalSortingOrders();
        base.OnDespawn();
    }

    public void SetAsPlayerTarget(bool isTarget)
    {
        if (isPlayerTarget == isTarget)
        {
            return;
        }

        if (originalSortingOrders == null || originalSortingOrders.Length != targetSortingRenderers.Count)
        {
            CacheOriginalSortingOrders();
        }

        isPlayerTarget = isTarget;
        ApplySortingOrders(isTarget ? targetSortingOrderIncrease : 0);
    }

    private void CacheOriginalSortingOrders()
    {
        originalSortingOrders = new int[targetSortingRenderers.Count];
        for (int i = 0; i < targetSortingRenderers.Count; i++)
        {
            SpriteRenderer targetRenderer = targetSortingRenderers[i];
            if (targetRenderer != null)
            {
                originalSortingOrders[i] = targetRenderer.sortingOrder;
            }
        }

        isPlayerTarget = false;
    }

    private void RestoreOriginalSortingOrders()
    {
        if (originalSortingOrders != null && originalSortingOrders.Length == targetSortingRenderers.Count)
        {
            ApplySortingOrders(0);
        }

        isPlayerTarget = false;
    }

    private void ApplySortingOrders(int orderIncrease)
    {
        // Cộng cùng một khoảng giúp giữ nguyên thứ tự tương đối giữa thân, vũ khí và health bar.
        for (int i = 0; i < targetSortingRenderers.Count; i++)
        {
            SpriteRenderer targetRenderer = targetSortingRenderers[i];
            if (targetRenderer != null)
            {
                targetRenderer.sortingOrder = originalSortingOrders[i] + orderIncrease;
            }
        }
    }

    public override void Despawn()
    {
        base.Despawn();
        EnemyManager.Ins.DespawnEnemy(this);
    }


}
