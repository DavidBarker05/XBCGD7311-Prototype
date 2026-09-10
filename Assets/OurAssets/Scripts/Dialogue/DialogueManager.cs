using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            // Lazy instantiation
            if (!_instance)
            {
                GameObject go = new GameObject(nameof(DialogueManager));
                _instance = go.AddComponent<DialogueManager>();
            }
            return _instance;
        }
    }

    Queue<DialogueItem> dialogueQueue;

    Dialogue currentDialogue;
    public DialogueItem CurrentDialogueItem { get; private set; }

    void Awake()
    {
        if (_instance && _instance != this) Destroy(gameObject);
        else _instance = this;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null) return;
        Clear();
        currentDialogue = dialogue;
        dialogueQueue = new Queue<DialogueItem>(dialogue.DialogueItems);
        if (dialogueQueue.Count != 0) LoadNextItem(); // Load the first item
    }

    public void LoadNextItem()
    {
        if (dialogueQueue == null || dialogueQueue.Count == 0)
        {
            Clear();
            return;
        }
        CurrentDialogueItem = dialogueQueue.Dequeue();
    }

    public void Clear()
    {
        if (currentDialogue == null) return;
        dialogueQueue = null;
        currentDialogue = null;
        CurrentDialogueItem = null;
    }
}