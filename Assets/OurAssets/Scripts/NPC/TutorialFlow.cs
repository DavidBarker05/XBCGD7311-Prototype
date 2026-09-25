using UnityEngine;

public class TutorialFlow : MonoBehaviour
{
    public static TutorialFlow Instance { get; private set; }

    public bool IsReadyForBed { get; private set; }

    [Header("Own Door")]
    [SerializeField]
    PlayerHouseDoor m_OwnDoor;
    [SerializeField]
    Sprite m_OwnDoorWaypointIcon;
    [SerializeField]
    Vector3 m_OwnDoorWaypointOffset = Vector3.zero;

    [Header("Themba's Door")]
    [SerializeField]
    TutorialHouseDoor m_ThembaDoor;
    [SerializeField]
    Sprite m_ThembaDoorWaypointIcon;
    [SerializeField]
    Vector3 m_ThembaDoorWaypointOffset = Vector3.zero;

    [Header("TV")]
    [SerializeField]
    TVInteractable m_TV;
    [SerializeField]
    Sprite m_TVWaypointIcon;
    [SerializeField]
    Vector3 m_TVWaypointOffset = Vector3.zero;

    readonly DisplayTask m_OwnDoorTask = new DisplayTask("Go say hello to your neighbour", 1, 0, false);
    readonly DisplayTask m_ThembaDoorTask = new DisplayTask("Head to your neighbour's house", 1, 0, false);
    readonly DisplayTask m_TVTask = new DisplayTask("Check out the TV licence upgrade", 1, 0, false);

    void Awake()
    {
        Debug.Log($"[TutorialFlow] Awake, frame={Time.frameCount} time={Time.realtimeSinceStartup:F3}");
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        Debug.Log($"[TutorialFlow] Start, frame={Time.frameCount} time={Time.realtimeSinceStartup:F3} WaypointManager.Instance={WaypointManager.Instance} TaskList.Instance={TaskList.Instance}");
        ShowOwnDoorMarker();
    }

    void ShowOwnDoorMarker()
    {
        if (!m_OwnDoor) return;
        WaypointManager.Instance?.AddWaypoint(m_OwnDoor.transform, m_OwnDoorWaypointIcon, m_OwnDoorWaypointOffset);
        TaskList.Instance?.AddTask(m_OwnDoorTask);
    }

    // Permanent diagnostic — see PlayerHouseDoor.Interact() for context on the bug we're chasing.
    public void OnLeftOwnHouse()
    {
        Debug.Log($"[TutorialFlow] OnLeftOwnHouse called, frame={Time.frameCount} time={Time.realtimeSinceStartup:F3} WaypointManager.Instance={WaypointManager.Instance} TaskList.Instance={TaskList.Instance}");
        if (m_OwnDoor) WaypointManager.Instance?.RemoveWaypoint(m_OwnDoor.transform);
        TaskList.Instance?.RemoveTask(m_OwnDoorTask);
        ShowThembaDoorMarker();
    }

    void ShowThembaDoorMarker()
    {
        if (!m_ThembaDoor) return;
        WaypointManager.Instance?.AddWaypoint(m_ThembaDoor.transform, m_ThembaDoorWaypointIcon, m_ThembaDoorWaypointOffset);
        TaskList.Instance?.AddTask(m_ThembaDoorTask);
    }

    public void OnEnteredThembaHouse()
    {
        if (m_ThembaDoor) WaypointManager.Instance?.RemoveWaypoint(m_ThembaDoor.transform);
        TaskList.Instance?.RemoveTask(m_ThembaDoorTask);
    }

    public void OnFinishedThembaHouse()
    {
        if (!m_TV) return;
        WaypointManager.Instance?.AddWaypoint(m_TV.transform, m_TVWaypointIcon, m_TVWaypointOffset);
        TaskList.Instance?.AddTask(m_TVTask);
    }

    public void OnTVScreenClosed()
    {
        if (m_TV) WaypointManager.Instance?.RemoveWaypoint(m_TV.transform);
        TaskList.Instance?.RemoveTask(m_TVTask);
        IsReadyForBed = true;
    }
}
