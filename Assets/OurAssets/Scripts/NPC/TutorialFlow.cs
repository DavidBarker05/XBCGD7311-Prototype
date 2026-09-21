using UnityEngine;

public class TutorialFlow : MonoBehaviour
{
    [Header("Own Door")]
    [SerializeField]
    PlayerHouseDoor m_OwnDoor;
    [SerializeField]
    Sprite m_OwnDoorWaypointIcon;

    [Header("Themba's Door")]
    [SerializeField]
    TutorialHouseDoor m_ThembaDoor;
    [SerializeField]
    Sprite m_ThembaDoorWaypointIcon;

    [Header("TV")]
    [SerializeField]
    TVInteractable m_TV;
    [SerializeField]
    Sprite m_TVWaypointIcon;

    [Header("Couch")]
    [SerializeField]
    PlayerEndDay m_Couch;
    [SerializeField]
    Sprite m_CouchWaypointIcon;

    readonly DisplayTask m_OwnDoorTask = new DisplayTask("Go say hello to your neighbour", 1, 0, false);
    readonly DisplayTask m_ThembaDoorTask = new DisplayTask("Head to your neighbour's house", 1, 0, false);
    readonly DisplayTask m_TVTask = new DisplayTask("Check out the TV licence upgrade", 1, 0, false);
    readonly DisplayTask m_CouchTask = new DisplayTask("Get some rest", 1, 0, false);

    void Start() => ShowOwnDoorMarker();

    void ShowOwnDoorMarker()
    {
        if (!m_OwnDoor) return;
        WaypointManager.Instance?.AddWaypoint(m_OwnDoor.transform, m_OwnDoorWaypointIcon);
        TaskList.Instance?.AddTask(m_OwnDoorTask);
    }

    public void OnLeftOwnHouse()
    {
        if (m_OwnDoor) WaypointManager.Instance?.RemoveWaypoint(m_OwnDoor.transform);
        TaskList.Instance?.RemoveTask(m_OwnDoorTask);
        ShowThembaDoorMarker();
    }

    void ShowThembaDoorMarker()
    {
        if (!m_ThembaDoor) return;
        WaypointManager.Instance?.AddWaypoint(m_ThembaDoor.transform, m_ThembaDoorWaypointIcon);
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
        WaypointManager.Instance?.AddWaypoint(m_TV.transform, m_TVWaypointIcon);
        TaskList.Instance?.AddTask(m_TVTask);
    }

    public void OnTVScreenClosed()
    {
        if (m_TV) WaypointManager.Instance?.RemoveWaypoint(m_TV.transform);
        TaskList.Instance?.RemoveTask(m_TVTask);
        ShowCouchMarker();
    }

    void ShowCouchMarker()
    {
        if (!m_Couch) return;
        WaypointManager.Instance?.AddWaypoint(m_Couch.transform, m_CouchWaypointIcon);
        TaskList.Instance?.AddTask(m_CouchTask);
    }
}
