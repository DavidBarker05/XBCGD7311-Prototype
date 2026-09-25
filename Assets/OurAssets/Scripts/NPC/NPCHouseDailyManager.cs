using System.Collections.Generic;
using UnityEngine;
using Util.ArrayUtils;

public class NPCHouseDailyManager : MonoBehaviour
{
    public static NPCHouseDailyManager Instance { get; private set; }

    [SerializeField]
    NPCHouse[] m_NPCHouses;
    [SerializeField, Min(0)]
    int m_MinHousesPerDay = 3;
    [SerializeField, Min(0)]
    int m_MaxHousesPerDay = 5;
    [SerializeField]
    Sprite m_HouseWaypointIcon;
    [SerializeField]
    Vector3 m_HouseWaypointOffset = Vector3.zero;

    [Header("Leave House")]
    [SerializeField]
    PlayerHouseDoor m_PlayerHouseDoor;
    [SerializeField]
    Sprite m_LeaveHouseWaypointIcon;
    [SerializeField]
    Vector3 m_LeaveHouseWaypointOffset = Vector3.zero;

    readonly List<NPCHouse> m_LoadedHouses = new List<NPCHouse>();

    DisplayTask m_DailyDisplayTask;
    DisplayTask m_LeaveHouseDisplayTask;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public NPCHouse GenerateHousesForDay()
    {
        if (m_LoadedHouses.Count > 0) ClearHouses();
        int houseCount = Mathf.Clamp(Random.Range(m_MinHousesPerDay, m_MaxHousesPerDay + 1), 0, m_NPCHouses.Length);
        NPCHouse[] shuffledHouses = (NPCHouse[])m_NPCHouses.Clone();
        shuffledHouses.Shuffle();
        NPCHouse houseWithPlayer = null;
        for (int i = 0; i < houseCount; ++i)
        {
            shuffledHouses[i].LoadHouse();
            m_LoadedHouses.Add(shuffledHouses[i]);
            if (shuffledHouses[i].Progress.IsPlayerInside) houseWithPlayer = shuffledHouses[i];
        }
        for (int i = houseCount; i < shuffledHouses.Length; ++i) shuffledHouses[i].MarkNotNeededToday();

        m_DailyDisplayTask = new DisplayTask("Help out the neighbourhood", m_LoadedHouses.Count, 0, strikeThroughOnCompletion: false);
        if (houseWithPlayer != null) OnPlayerLeftOwnHouse();
        else ShowLeaveHouseMarker();
        return houseWithPlayer;
    }

    public void ClearHouses()
    {
        foreach (NPCHouse house in m_LoadedHouses) house.UnloadHouse();
        m_LoadedHouses.Clear();
        HouseProgressTracker.ClearAll();
        if (m_DailyDisplayTask != null) { TaskList.Instance?.RemoveTask(m_DailyDisplayTask); m_DailyDisplayTask = null; }
        HideLeaveHouseMarker();
        m_LeaveHouseDisplayTask = null;
    }

    public void ReportHouseCompleted()
    {
        if (m_DailyDisplayTask != null) TaskList.Instance?.IncrementAmountDoneForTask(m_DailyDisplayTask);
    }

    public bool AllMinigamesBeatenForToday()
    {
        foreach (NPCHouse house in m_LoadedHouses) if (!house.Progress.AllMinigamesBeaten) return false;
        return true;
    }

    #region Leave House
    void ShowLeaveHouseMarker()
    {
        if (!m_PlayerHouseDoor)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: {name} has no player house door assigned");
#endif
            return;
        }
        m_LeaveHouseDisplayTask ??= new DisplayTask("Leave your house to start helping the neighbourhood", 1, 0, false);
        WaypointManager.Instance?.AddWaypoint(m_PlayerHouseDoor.transform, m_LeaveHouseWaypointIcon, m_LeaveHouseWaypointOffset);
        TaskList.Instance?.AddTask(m_LeaveHouseDisplayTask);
    }

    void HideLeaveHouseMarker()
    {
        if (m_PlayerHouseDoor) WaypointManager.Instance?.RemoveWaypoint(m_PlayerHouseDoor.transform);
        if (m_LeaveHouseDisplayTask != null) TaskList.Instance?.RemoveTask(m_LeaveHouseDisplayTask);
    }

    public void OnPlayerLeftOwnHouse()
    {
        HideLeaveHouseMarker();
        ShowMarkersForIncompleteHouses();
    }
    #endregion Leave House

    #region House Markers
    public void ShowMarkersForIncompleteHouses()
    {
        foreach (NPCHouse house in m_LoadedHouses)
        {
            if (house.Progress == null || house.Progress.IsPlayerInside || house.Progress.AllMinigamesBeaten || !house.EntryPoint) continue;
            WaypointManager.Instance?.AddWaypoint(house.EntryPoint, m_HouseWaypointIcon, m_HouseWaypointOffset);
        }
        if (m_DailyDisplayTask != null) TaskList.Instance?.AddTask(m_DailyDisplayTask);
    }

    public void HideAllHouseMarkers()
    {
        foreach (NPCHouse house in m_LoadedHouses) if (house.EntryPoint) WaypointManager.Instance?.RemoveWaypoint(house.EntryPoint);
        if (m_DailyDisplayTask != null) TaskList.Instance?.RemoveTask(m_DailyDisplayTask);
    }
    #endregion House Markers
}
