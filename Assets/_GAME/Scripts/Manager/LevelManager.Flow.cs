using UnityEngine;

public partial class LevelManager
{
    [Header("Level Result")]
    [SerializeField, Min(0f)] private float levelFailDisplayDuration = 2f;
    [SerializeField, Min(0f)] private float levelCompleteDisplayDuration = 2f;

    public void OnLevelStart()
    {
        if (!IsInitialized || levelInfo.IsLevelNull())
        {
            return;
        }

        CancelPendingLevelTransition();
        GetCombatCanvas()?.OnLevelStart();
        LevelDataSO levelData = levelInfo.GetLevelData();

        // Xoa attempt cu hoan toan truoc khi dat lai team va bat dau spawn wave dau.
        EnemyManager.Ins.ResetForLevelStart();
        PlayerManager.Ins.PrepareTeamForLevelStart(
            levelData.TeamSpawnPosition);
        levelInfo.SetWaveData(-1);
        State = LevelPlayState.None;
        if (!StartNextWave())
        {
            OnLevelComplete(levelCompleteDisplayDuration);
        }
    }

    public void OnLevelFail(float displayDuration)
    {
        if (!CanResolveLevelResult())
        {
            return;
        }

        float duration = Mathf.Max(0f, displayDuration);
        State = LevelPlayState.Failed;
        GetCombatCanvas()?.OnLevelFail(duration);
        CancelPendingLevelTransition();
        Invoke(nameof(OnLevelStart), duration);
    }

    public void OnLevelComplete(float displayDuration)
    {
        if (!IsInitialized
            || levelInfo.IsLevelNull()
            || State == LevelPlayState.Completed
            || State == LevelPlayState.Failed)
        {
            return;
        }

        float duration = Mathf.Max(0f, displayDuration);
        State = LevelPlayState.Completed;
        GetCombatCanvas()?.OnLevelComplete(duration);
        CancelPendingLevelTransition();
        Invoke(nameof(OnNextLevel), duration);
    }

    public void OnNextLevel()
    {
        CancelPendingLevelTransition();
        StartNextLevel();
    }

    private bool CanResolveLevelResult()
    {
        return IsInitialized
            && (State == LevelPlayState.PlayingWave
                || State == LevelPlayState.WaitingNextWave);
    }

    private void CancelPendingLevelTransition()
    {
        CancelInvoke(nameof(OnLevelStart));
        CancelInvoke(nameof(OnNextLevel));
    }

    private CanvasCombat GetCombatCanvas()
    {
        return UIManager.Ins != null ? UIManager.Ins.GetUI<CanvasCombat>() : null;
    }
}
