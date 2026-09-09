// Static so it survives scene loads without needing a DontDestroyOnLoad object,
// letting a minigame report completion even if it runs in a different scene
// than the NPCHouse that's waiting to hear about it
public static class HouseMinigameProgressTracker
{
    public static MinigameType? PendingCompletedMinigame { get; private set; }

    public static void ReportMinigameCompleted(MinigameType minigameType) => PendingCompletedMinigame = minigameType;

    public static MinigameType? ConsumePendingCompletedMinigame()
    {
        MinigameType? pending = PendingCompletedMinigame;
        PendingCompletedMinigame = null;
        return pending;
    }
}
