public partial class DataManager
{
    public bool CanResetSelectedCharacterLevel(out string reason)
    {
        if (playerData == null)
        {
            reason = "Player data is not initialized.";
            return false;
        }

        return playerData.CanResetCharacterLevel(SelectedCharacter, out reason);
    }

    public bool ResetSelectedCharacterLevel(out string message)
    {
        if (playerData == null)
        {
            message = "Player data is not initialized.";
            return false;
        }

        // Resolve the current Inventory/Hero selection at click time, not a cached hero.
        return playerData.ResetCharacterLevel(SelectedCharacter, out message);
    }
}
