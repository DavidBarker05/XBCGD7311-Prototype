using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField]
    DialogueDisplayer m_DialogueDisplayer;

    Queue<DialogueItem> m_DialogueQueue;

    Dialogue m_CurrentDialogue;
    public DialogueItem CurrentDialogueItem { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void StartDialogue(Dialogue dialogue, System.Action callBackFunction = null)
    {
        if (dialogue == null) return;
        Clear();
        m_CurrentDialogue = dialogue;
        m_DialogueQueue = new Queue<DialogueItem>(dialogue.DialogueItems);
        if (m_DialogueQueue.Count != 0) LoadNextItem(); // Load the first item
        m_DialogueDisplayer.StartDisplayingDialogue(callBackFunction);
    }

    public void LoadNextItem()
    {
        if (m_DialogueQueue == null || m_DialogueQueue.Count == 0)
        {
            Clear();
            return;
        }
        CurrentDialogueItem = m_DialogueQueue.Dequeue();
    }

    public void Clear()
    {
        if (m_CurrentDialogue == null) return;
        m_DialogueQueue = null;
        m_CurrentDialogue = null;
        CurrentDialogueItem = null;
    }
}