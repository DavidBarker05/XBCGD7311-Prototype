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

    readonly List<NPCHouse> m_LoadedHouses = new List<NPCHouse>();

    DisplayTask m_DailyDisplayTask;

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
        m_DailyDisplayTask = new DisplayTask("Help out the neighbourhood", m_LoadedHouses.Count, 0, strikeThroughOnCompletion: false);
        TaskList.Instance?.AddTask(m_DailyDisplayTask);
        return houseWithPlayer;
    }

    public void ClearHouses()
    {
        foreach (NPCHouse house in m_LoadedHouses) house.UnloadHouse();
        m_LoadedHouses.Clear();
        HouseProgressTracker.ClearAll();
        if (m_DailyDisplayTask != null) { TaskList.Instance?.RemoveTask(m_DailyDisplayTask); m_DailyDisplayTask = null; }
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

    #region House Markers
    public void ShowMarkersForIncompleteHouses()
    {
        foreach (NPCHouse house in m_LoadedHouses)
        {
            if (house.Progress == null || house.Progress.IsPlayerInside || house.Progress.AllMinigamesBeaten || !house.EntryPoint) continue;
            WaypointManager.Instance?.AddWaypoint(house.EntryPoint, m_HouseWaypointIcon);
        }
    }

    public void HideAllHouseMarkers()
    {
        foreach (NPCHouse house in m_LoadedHouses) if (house.EntryPoint) WaypointManager.Instance?.RemoveWaypoint(house.EntryPoint);
    }
    #endregion House Markers
}
