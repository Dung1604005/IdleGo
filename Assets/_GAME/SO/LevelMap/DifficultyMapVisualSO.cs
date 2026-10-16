using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DifficultyMapVisualSO",
    menuName = "IdleGo/UI/Difficulty Map Visual")]
public sealed class DifficultyMapVisualSO : ScriptableObject
{
    [SerializeField] private List<DifficultyMapVisual> visuals =
        new List<DifficultyMapVisual>();

    public Sprite GetBorder(DifficultType difficulty)
    {
        DifficultyMapVisual visual = FindVisual(difficulty);
        return visual != null ? visual.Border : null;
    }

    public Sprite GetNameBackground(DifficultType difficulty)
    {
        DifficultyMapVisual visual = FindVisual(difficulty);
        return visual != null ? visual.NameBackground : null;
    }

    public Sprite GetButtonNormal(DifficultType difficulty)
    {
        return FindVisual(difficulty)?.ButtonNormal;
    }

    public Sprite GetButtonHighlighted(DifficultType difficulty)
    {
        return FindVisual(difficulty)?.ButtonHighlighted;
    }

    private DifficultyMapVisual FindVisual(DifficultType difficulty)
    {
        for (int i = 0; i < visuals.Count; i++)
        {
            if (visuals[i] != null && visuals[i].Difficulty == difficulty)
            {
                return visuals[i];
            }
        }
        return null;
    }
}

[Serializable]
public sealed class DifficultyMapVisual
{
    [SerializeField] private DifficultType difficulty;
    [SerializeField] private Sprite border;
    [SerializeField] private Sprite nameBackground;
    [SerializeField] private Sprite buttonNormal;
    [SerializeField] private Sprite buttonHighlighted;

    public DifficultType Difficulty => difficulty;
    public Sprite Border => border;
    public Sprite NameBackground => nameBackground;
    public Sprite ButtonNormal => buttonNormal;
    public Sprite ButtonHighlighted => buttonHighlighted;
}
