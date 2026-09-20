using UnityEngine;

public class EndingNPC : Interactable
{
    public static EndingNPC Instance { get; private set; }

    public bool IsChoiceActive { get; private set; }

    [SerializeField]
    Transform m_VisualsRoot;
    [SerializeField]
    TextAsset m_EndingDialogue;
    [SerializeField]
    Sprite m_TalkWaypointIcon;

    [Header("Waypoints")]
    [SerializeField]
    Transform m_CouchTransform;
    [SerializeField]
    Sprite m_CouchWaypointIcon;
    [SerializeField]
    Transform m_DoorTransform;
    [SerializeField]
    Sprite m_DoorWaypointIcon;

    bool m_bChoicePresented;
    bool m_bEndingChosen;

    readonly DisplayTask m_TalkDisplayTask = new DisplayTask("Talk to Nomsa", 1, 0, false);
    readonly DisplayTask m_CouchDisplayTask = new DisplayTask("Rest on the couch", 1, 0, false);
    readonly DisplayTask m_DoorDisplayTask = new DisplayTask("Head back out to help more people", 1, 0, false);

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Update() => Refresh();

    void Refresh()
    {
        bool bIsEndingDay = PlayerSaveManager.CurrentSaveData != null && PlayerSaveManager.CurrentSaveData.DayNumber == 10;
        if (!bIsEndingDay)
        {
            m_bChoicePresented = false;
            m_bEndingChosen = false;
        }
        SetVisualActive(bIsEndingDay && !m_bChoicePresented);
        IsChoiceActive = bIsEndingDay && m_bChoicePresented && !m_bEndingChosen;
        SetWaypoint(m_CouchTransform, IsChoiceActive, m_CouchWaypointIcon, m_CouchDisplayTask);
        SetWaypoint(m_DoorTransform, IsChoiceActive, m_DoorWaypointIcon, m_DoorDisplayTask);
    }

    void SetVisualActive(bool bActive)
    {
        if (!m_VisualsRoot || m_VisualsRoot.gameObject.activeSelf == bActive) return;
        m_VisualsRoot.gameObject.SetActive(bActive);
        if (bActive)
        {
            WaypointManager.Instance?.AddWaypoint(m_VisualsRoot, m_TalkWaypointIcon);
            TaskList.Instance?.AddTask(m_TalkDisplayTask);
        }
        else
        {
            WaypointManager.Instance?.RemoveWaypoint(m_VisualsRoot);
            TaskList.Instance?.RemoveTask(m_TalkDisplayTask);
        }
    }

    void SetWaypoint(Transform target, bool bActive, Sprite icon, DisplayTask task)
    {
        if (!target) return;
        if (bActive)
        {
            WaypointManager.Instance?.AddWaypoint(target, icon);
            TaskList.Instance?.AddTask(task);
        }
        else
        {
            WaypointManager.Instance?.RemoveWaypoint(target);
            TaskList.Instance?.RemoveTask(task);
        }
    }

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: EndingNPC objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
            return new InteractionStatus() { EndInteraction = true };
        }
        if (!m_bChoicePresented && m_EndingDialogue)
        {
            Dialogue dialogue = JsonUtility.FromJson<SerializedDialogue>(m_EndingDialogue.text).Deserialized;
            DialogueManager.Instance.StartDialogue(dialogue, OnEndingDialogueFinished);
        }
        return new InteractionStatus() { EndInteraction = true };
    }

    void OnEndingDialogueFinished() => m_bChoicePresented = true;

    public void NotifyEndingChosen() => m_bEndingChosen = true;
}
