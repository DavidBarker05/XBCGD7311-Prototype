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

    readonly List<NPCHouse> m_LoadedHouses = new List<NPCHouse>();

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
        return houseWithPlayer;
    }

    public void ClearHouses()
    {
        foreach (NPCHouse house in m_LoadedHouses) house.UnloadHouse();
        m_LoadedHouses.Clear();
        HouseProgressTracker.ClearAll();
    }

    public bool AllMinigamesBeatenForToday()
    {
        foreach (NPCHouse house in m_LoadedHouses) if (!house.Progress.AllMinigamesBeaten) return false;
        return true;
    }
}
