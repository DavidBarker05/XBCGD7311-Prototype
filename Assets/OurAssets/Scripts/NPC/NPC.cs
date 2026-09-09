using UnityEngine;

public class NPC : Interactable
{
    public NPCHouse OwningHouse { get; set; }

    public int TotalMinigamesToBeat => OwningHouse ? OwningHouse.HouseMinigames.Length : 0;

    public MinigameType[] RequiredMinigameTypes
    {
        get
        {
            if (!OwningHouse) return System.Array.Empty<MinigameType>();
            MinigameType[] types = new MinigameType[OwningHouse.HouseMinigames.Length];
            for (int i = 0; i < types.Length; ++i) types[i] = OwningHouse.HouseMinigames[i].Minigame;
            return types;
        }
    }

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        // TODO: Start dialogue
        return new InteractionStatus() { EndInteraction = true };
    }
}
