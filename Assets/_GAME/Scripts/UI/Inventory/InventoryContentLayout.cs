using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class InventoryContentLayout
{
    [SerializeField] private Vector2 slotSize = new Vector2(100f, 100f);

    [NonSerialized] private RectTransform viewport;
    [NonSerialized] private RectTransform content;
    [NonSerialized] private GridLayoutGroup gridLayout;
    [NonSerialized] private float lastViewportWidth = -1f;
    [NonSerialized] private int currentSlotCount;

    public void OnInit(
        RectTransform viewportRect,
        RectTransform contentRect,
        GridLayoutGroup contentGrid)
    {
        viewport = viewportRect;
        content = contentRect;
        gridLayout = contentGrid;
        lastViewportWidth = -1f;
    }

    public void Refresh(int slotCount)
    {
        currentSlotCount = Mathf.Max(0, slotCount);
        if (viewport == null || content == null || gridLayout == null)
        {
            return;
        }

        float viewportWidth = viewport.rect.width;
        int columnCount = CalculateColumnCount(viewportWidth);
        int rowCount = currentSlotCount == 0
            ? 0
            : Mathf.CeilToInt(currentSlotCount / (float)columnCount);

        gridLayout.cellSize = slotSize;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columnCount;

        // Content bam ngang theo Viewport; chi chieu cao thay doi theo so hang dang mo.
        StretchContentToViewportWidth();
        float contentHeight = CalculateContentHeight(rowCount);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        lastViewportWidth = viewportWidth;
    }

    public void RefreshIfViewportWidthChanged()
    {
        if (viewport == null || Mathf.Approximately(lastViewportWidth, viewport.rect.width))
        {
            return;
        }

        Refresh(currentSlotCount);
    }

    public void OnDespawn()
    {
        viewport = null;
        content = null;
        gridLayout = null;
        lastViewportWidth = -1f;
        currentSlotCount = 0;
    }

    private int CalculateColumnCount(float viewportWidth)
    {
        RectOffset padding = gridLayout.padding;
        float usableWidth = Mathf.Max(0f, viewportWidth - padding.left - padding.right);
        float widthPerColumn = slotSize.x + gridLayout.spacing.x;

        if (widthPerColumn <= 0f)
        {
            return 1;
        }

        return Mathf.Max(
            1,
            Mathf.FloorToInt((usableWidth + gridLayout.spacing.x) / widthPerColumn)
        );
    }

    private float CalculateContentHeight(int rowCount)
    {
        RectOffset padding = gridLayout.padding;
        float rowsHeight = rowCount * slotSize.y;
        float spacingHeight = Mathf.Max(0, rowCount - 1) * gridLayout.spacing.y;
        return padding.top + padding.bottom + rowsHeight + spacingHeight;
    }

    private void StretchContentToViewportWidth()
    {
        Vector2 anchorMin = content.anchorMin;
        Vector2 anchorMax = content.anchorMax;
        Vector2 sizeDelta = content.sizeDelta;

        anchorMin.x = 0f;
        anchorMax.x = 1f;
        sizeDelta.x = 0f;

        content.anchorMin = anchorMin;
        content.anchorMax = anchorMax;
        content.sizeDelta = sizeDelta;
    }
}
