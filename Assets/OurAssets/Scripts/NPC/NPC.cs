using UnityEngine;

public class NPC : Interactable
{
    public NPCHouse OwningHouse { get; set; }

    public int TotalMinigamesToBeat => OwningHouse ? OwningHouse.Progress.MinigameTypes.Length : 0;

    public MinigameType[] RequiredMinigameTypes => OwningHouse ? OwningHouse.Progress.MinigameTypes : System.Array.Empty<MinigameType>();

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: NPC objects needs 1 input parameter. Received {inputParameters.Length} input parameters");
#endif
        }
        else
        {
            // TODO: Start dialogue, for now this just unlocks the house's tasks, same as if dialogue had played and finished
            OwningHouse?.OnNPCTalkedTo();
        }
        return new InteractionStatus() { EndInteraction = true };
    }
}
