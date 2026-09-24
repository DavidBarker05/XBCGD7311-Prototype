using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    public NPCHouse OwningHouse { get; set; }

    public int TotalMinigamesToBeat => OwningHouse ? OwningHouse.Progress.MinigameTypes.Length : 0;

    public MinigameType[] RequiredMinigameTypes => OwningHouse ? OwningHouse.Progress.MinigameTypes : System.Array.Empty<MinigameType>();

    [Header("Dialogue")]
    [SerializeField, Min(1)]
    int m_FontSize = 24;
    [SerializeField, Min(1)]
    int m_CharactersPerSecond = 50;

    string m_Name;

    void Awake() => m_Name = NPCDialogueLibrary.RandomName();

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: NPC objects needs 1 input parameter. Received {inputParameters.Length} input parameters");
#endif
            return new InteractionStatus() { EndInteraction = true };
        }

        if (!OwningHouse || DialogueManager.Instance == null) return new InteractionStatus() { EndInteraction = true };

        HouseProgress progress = OwningHouse.Progress;
        bool bFirstTalk = !progress.HasTalkedToNPC;

        List<DialogueItem> items = new List<DialogueItem>();
        if (bFirstTalk) items.Add(MakeLine(NPCDialogueLibrary.RandomGreeting()));
        items.Add(
            MakeLine(
                bFirstTalk
                ? NPCDialogueLibrary.RandomTaskIntro(progress)
                : progress.AllMinigamesBeaten
                    ? NPCDialogueLibrary.RandomThanks()
                    : NPCDialogueLibrary.RandomReminder(progress)
            )
        );

        Dialogue dialogue = new Dialogue() { DialogueItems = items };
        System.Action onFinished = bFirstTalk ? OwningHouse.OnNPCTalkedTo : null;
        DialogueManager.Instance.StartDialogue(dialogue, onFinished);

        return new InteractionStatus() { EndInteraction = true };
    }

    DialogueItem MakeLine(string text) => new DialogueItem() { Name = m_Name, Text = text, FontSize = m_FontSize, CharactersPerSecond = m_CharactersPerSecond };
}
