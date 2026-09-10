using System;
using System.Collections.Generic;

[Serializable]
public class SerializedDialogue
{
    public SerializedDialogueItem[] dialogueItems;

    public Dialogue Deserialized
    {
        get
        {
            List<DialogueItem> items = new List<DialogueItem>();
            foreach (SerializedDialogueItem serialisedItem in dialogueItems) items.Add(serialisedItem.Deserialized);
            return new Dialogue() { DialogueItems = items };
        }
    }
}

[Serializable]
public class Dialogue
{
    public List<DialogueItem> DialogueItems;

    public SerializedDialogue Serialised
    {
        get
        {
            List<SerializedDialogueItem> serialisedItems = new List<SerializedDialogueItem>();
            foreach (DialogueItem item in DialogueItems) serialisedItems.Add(item.Serialized);
            return new SerializedDialogue() { dialogueItems = serialisedItems.ToArray() };
        }
    }
}