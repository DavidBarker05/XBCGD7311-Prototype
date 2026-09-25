using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EndingNPC : Interactable
{
    public static EndingNPC Instance { get; private set; }

    public bool IsChoiceActive { get; private set; }
    public bool IsEndingDay => PlayerSaveManager.CurrentSaveData != null && PlayerSaveManager.CurrentSaveData.DayNumber == 10;
    public bool HasTalkedToday => m_bChoicePresented;

    [SerializeField]
    Transform m_VisualsRoot;
    [SerializeField]
    TextAsset m_EndingDialogue;
    [SerializeField]
    Sprite m_TalkWaypointIcon;
    [SerializeField]
    Vector3 m_TalkWaypointOffset = new Vector3(0f, 1f);

    [Header("Waypoints")]
    [SerializeField]
    Transform m_CouchTransform;
    [SerializeField]
    Sprite m_CouchWaypointIcon;
    [SerializeField]
    Vector3 m_CouchWaypointOffset = Vector3.zero;
    [SerializeField]
    Transform m_DoorTransform;
    [SerializeField]
    Sprite m_DoorWaypointIcon;
    [SerializeField]
    Vector3 m_DoorWaypointOffset = Vector3.zero;

    Collider m_InteractionCollider;

    bool m_bChoicePresented;
    bool m_bEndingChosen;
    bool m_bVisualActive;
    bool m_bCouchWaypointActive;
    bool m_bDoorWaypointActive;

    readonly DisplayTask m_TalkDisplayTask = new DisplayTask("Talk to Themba", 1, 0, false);
    readonly DisplayTask m_CouchDisplayTask = new DisplayTask("Rest on the couch", 1, 0, false);
    readonly DisplayTask m_DoorDisplayTask = new DisplayTask("Head back out to help more people", 1, 0, false);

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            m_InteractionCollider = GetComponent<Collider>();
            if (m_VisualsRoot) m_VisualsRoot.gameObject.SetActive(false);
        }
    }

    void Update() => Refresh();

    void Refresh()
    {
        bool bIsEndingDay = IsEndingDay;
        if (!bIsEndingDay)
        {
            m_bChoicePresented = false;
            m_bEndingChosen = false;
        }
        SetVisualActive(bIsEndingDay && !m_bChoicePresented);
        IsChoiceActive = bIsEndingDay && m_bChoicePresented && !m_bEndingChosen;
        SetWaypoint(m_CouchTransform, IsChoiceActive, m_CouchWaypointIcon, m_CouchDisplayTask, m_CouchWaypointOffset, ref m_bCouchWaypointActive);
        SetWaypoint(m_DoorTransform, IsChoiceActive, m_DoorWaypointIcon, m_DoorDisplayTask, m_DoorWaypointOffset, ref m_bDoorWaypointActive);
    }

    void SetVisualActive(bool bActive)
    {
        m_InteractionCollider.enabled = bActive;
        if (!m_VisualsRoot || m_bVisualActive == bActive) return;
        m_bVisualActive = bActive;
        m_VisualsRoot.gameObject.SetActive(bActive);
        if (bActive)
        {
            WaypointManager.Instance?.AddWaypoint(m_VisualsRoot, m_TalkWaypointIcon, m_TalkWaypointOffset);
            TaskList.Instance?.AddTask(m_TalkDisplayTask);
        }
        else
        {
            WaypointManager.Instance?.RemoveWaypoint(m_VisualsRoot);
            TaskList.Instance?.RemoveTask(m_TalkDisplayTask);
        }
    }

    void SetWaypoint(Transform target, bool bActive, Sprite icon, DisplayTask task, Vector3 waypointOffset, ref bool currentlyActive)
    {
        if (!target || bActive == currentlyActive) return;
        currentlyActive = bActive;
        if (bActive)
        {
            WaypointManager.Instance?.AddWaypoint(target, icon, waypointOffset);
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
        if (IsEndingDay && !m_bChoicePresented && m_EndingDialogue)
        {
            Dialogue dialogue = JsonUtility.FromJson<SerializedDialogue>(m_EndingDialogue.text).Deserialized;
            DialogueManager.Instance.StartDialogue(dialogue, OnEndingDialogueFinished);
        }
        return new InteractionStatus() { EndInteraction = true };
    }

    void OnEndingDialogueFinished() => m_bChoicePresented = true;

    public void NotifyEndingChosen() => m_bEndingChosen = true;
}
