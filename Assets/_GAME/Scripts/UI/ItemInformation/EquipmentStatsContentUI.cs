using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class EquipmentStatsContentUI : MonoBehaviour
{
    [Header("Dynamic Content")]
    [SerializeField] private RectTransform dynamicContent;
    [SerializeField] private StatLineUI statLinePrefab;
    [SerializeField] private GameObject mainToSubStatLine;
    [SerializeField] private GameObject subStatToEnhancementLine;
    [SerializeField]
    private List<BuffStatHeaderUI> buffStatHeaders =
        new List<BuffStatHeaderUI>();

    [Header("Resize")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private RectTransform panelLayoutContent;
    [SerializeField, Min(0f)] private float minimumPanelHeight = 421f;
    [SerializeField, Min(0f)] private float footerReservedHeight = 112f;

    private readonly List<StatLineUI> activeStatLines =
        new List<StatLineUI>();

    public void OnInit()
    {
        SimplePool.PreLoad(statLinePrefab, 8, dynamicContent);
        ClearContent();
    }

    public void Refresh(Equipment equipment)
    {
        ClearContent();
        if (equipment?.Data == null || dynamicContent == null)
        {
            ResizePanel();
            return;
        }

        equipment.EnsureRuntimeState();
        IReadOnlyList<StatValue> subStats = equipment.GetSubStats();
        bool hasSubStats = subStats != null && subStats.Count > 0;
        bool hasEnhancements = HasEnhancementSlots(equipment.Data);

        SetSeparator(mainToSubStatLine, hasSubStats, 0);
        int siblingIndex = hasSubStats ? 1 : 0;
        siblingIndex = AddSubStats(subStats, siblingIndex);
        siblingIndex = SetEnhancementSeparator(
            hasSubStats, hasEnhancements, siblingIndex);
        AddEnhancementSections(equipment, siblingIndex);
        ResizePanel();
    }

    public void OnDespawn()
    {
        ClearContent();
    }

    private int AddSubStats(
        IReadOnlyList<StatValue> subStats,
        int siblingIndex)
    {
        if (subStats == null)
        {
            return siblingIndex;
        }

        for (int i = 0; i < subStats.Count; i++)
        {
            StatLineUI line = SpawnStatLine(siblingIndex++);
            line?.OnInit(subStats[i]);
        }
        return siblingIndex;
    }

    private int SetEnhancementSeparator(
        bool hasSubStats,
        bool hasEnhancements,
        int siblingIndex)
    {
        bool isVisible = hasSubStats && hasEnhancements;
        SetSeparator(
            subStatToEnhancementLine,
            isVisible,
            siblingIndex);
        return isVisible ? siblingIndex + 1 : siblingIndex;
    }

    private void AddEnhancementSections(
        Equipment equipment,
        int siblingIndex)
    {
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            BuffStatType type = (BuffStatType)i;
            int slotCount = equipment.Data.GetBuffSlotCount(type);
            BuffStatHeaderUI header = GetHeader(i);
            SetHeader(header, type, slotCount, ref siblingIndex);
            if (slotCount > 0)
            {
                siblingIndex = AddEnhancementLines(
                    equipment, type, slotCount, siblingIndex);
            }
        }
    }

    private int AddEnhancementLines(
        Equipment equipment,
        BuffStatType buffStatType,
        int slotCount,
        int siblingIndex)
    {
        IReadOnlyList<StatValue> stats = equipment.GetBuffStats(buffStatType);
        for (int i = 0; i < slotCount; i++)
        {
            StatValue stat = stats != null && i < stats.Count ? stats[i] : null;
            StatLineUI line = SpawnStatLine(siblingIndex++);
            line?.OnInitEnhancement(stat);
        }
        return siblingIndex;
    }

    private StatLineUI SpawnStatLine(int siblingIndex)
    {
        StatLineUI line = SimplePool.Spawn(
            statLinePrefab,
            Vector3.zero,
            Quaternion.identity,
            dynamicContent);
        if (line == null)
        {
            return null;
        }

        // Pool dung chung cho hai panel, vi vay luon gan lai parent khi lay ra.
        line.TF.SetParent(dynamicContent, false);
        line.TF.SetSiblingIndex(siblingIndex);
        activeStatLines.Add(line);
        return line;
    }

    private void SetHeader(
        BuffStatHeaderUI header,
        BuffStatType type,
        int slotCount,
        ref int siblingIndex)
    {
        if (header == null)
        {
            return;
        }

        if (slotCount <= 0)
        {
            header.OnDespawn();
            return;
        }

        header.OnInit(type);
        header.transform.SetSiblingIndex(siblingIndex++);
    }

    private void ClearContent()
    {
        for (int i = activeStatLines.Count - 1; i >= 0; i--)
        {
            SimplePool.Despawn(activeStatLines[i]);
        }
        activeStatLines.Clear();
        SetSeparator(mainToSubStatLine, false, 0);
        SetSeparator(subStatToEnhancementLine, false, 0);
        for (int i = 0; i < buffStatHeaders.Count; i++)
        {
            buffStatHeaders[i]?.OnDespawn();
        }
    }

    private void ResizePanel()
    {
        if (panelRect == null || panelLayoutContent == null)
        {
            return;
        }

        // Rebuild ngay sau khi thay doi pool de panel lay dung preferred height.
        Canvas.ForceUpdateCanvases();
        if (dynamicContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(dynamicContent);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelLayoutContent);
        float layoutHeight = panelLayoutContent.rect.height;
        if (layoutHeight > 0f)
        {
            // PanelLayoutContent da gom header 220, dynamic content va spacing.
            float contentHeight = layoutHeight + footerReservedHeight;
            float targetHeight = Mathf.Max(minimumPanelHeight, contentHeight);
            panelRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                targetHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }
    }
    private BuffStatHeaderUI GetHeader(int index)
    {
        return index >= 0 && index < buffStatHeaders.Count
            ? buffStatHeaders[index]
            : null;
    }

    private static bool HasEnhancementSlots(EquipmentDataSO data)
    {
        for (int i = 0; i < BuffStatTypeUtility.Count; i++)
        {
            if (data.GetBuffSlotCount((BuffStatType)i) > 0)
            {
                return true;
            }
        }
        return false;
    }

    private static void SetSeparator(
        GameObject separator,
        bool isVisible,
        int siblingIndex)
    {
        if (separator == null)
        {
            return;
        }

        separator.SetActive(isVisible);
        if (isVisible)
        {
            separator.transform.SetSiblingIndex(siblingIndex);
        }
    }
}
