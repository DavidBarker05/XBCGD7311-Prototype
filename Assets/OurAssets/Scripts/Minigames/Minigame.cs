using UnityEngine;

public class Minigame : Interactable
{
    public System.Action OnMinigameEnd;

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        return new InteractionStatus() { EndInteraction = true };
    }
}
