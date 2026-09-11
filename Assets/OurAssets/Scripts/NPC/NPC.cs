using UnityEngine;

public class NPC : Interactable
{
    public NPCHouse OwningHouse { get; set; }

    public int TotalMinigamesToBeat => OwningHouse ? OwningHouse.Progress.MinigameTypes.Length : 0;

    public MinigameType[] RequiredMinigameTypes => OwningHouse ? OwningHouse.Progress.MinigameTypes : System.Array.Empty<MinigameType>();

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        // TODO: Start dialogue
        return new InteractionStatus() { EndInteraction = true };
    }
}
