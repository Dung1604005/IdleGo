using UnityEngine;

public partial class CanvasHeroStatus
{
    private void RefreshStatLines(string characterId)
    {
        int lineIndex = 0;
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            StatType statType = (StatType)i;
            if (!StatTypeUtility.CanHaveModifiers(statType)) continue;
            // Giu nguyen line dang hien de refresh khong ngat ticker va pop tween.
            StatProgressLineUI line = lineIndex < activeStatLines.Count
                ? activeStatLines[lineIndex] : SpawnStatLine();
            if (line == null) continue;
            line.TF.SetAsLastSibling();
            line.SetData(characterId, statType);
            lineIndex++;
        }
    }

    private StatProgressLineUI SpawnStatLine()
    {
        StatProgressLineUI line = SimplePool.Spawn(
            statLinePrefab, Vector3.zero, Quaternion.identity, statContent);
        if (line != null)
        {
            line.TF.SetParent(statContent, false);
            line.TF.SetAsLastSibling();
            activeStatLines.Add(line);
        }
        return line;
    }

    private void ClearStatLines()
    {
        for (int i = activeStatLines.Count - 1; i >= 0; i--)
        {
            SimplePool.Despawn(activeStatLines[i]);
        }
        activeStatLines.Clear();
    }
}
