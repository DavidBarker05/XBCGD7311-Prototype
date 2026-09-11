using UnityEngine;

public class PlayerEndDay : Interactable
{
    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: PlayerEndDay objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
        }
        else
        {
            ++PlayerSaveManager.CurrentSaveData.DayNumber;
            if (PlayerSaveManager.CurrentSaveData.DayNumber > 1) PlayerSaveManager.GenerateRandomSeed();
            PlayerSaveManager.SeedRandomForCurrentDay();
        }
        return new InteractionStatus() { EndInteraction = true };
    }
}
