using System.Collections.Generic;
using UnityEngine;
using Util.ArrayUtils;

public class NPCHouse : MonoBehaviour
{
    [Header("NPC")]
    [SerializeField]
    Transform[] m_PossibleNPCLocations;
    [SerializeField]
    NPC[] m_PossibleNPCs;

    // vvv Minigames vvv

    [Header("Minigame Counts")]
    [SerializeField, Min(2)]
    int m_MinTotalMinigames = 2;
    [SerializeField, Min(0)]
    int m_MaxWireMinigames = 2;
    [SerializeField, Min(0)]
    int m_MaxWallKnockMinigames = 2;
    [SerializeField, Min(0)]
    int m_MaxChaseMinigames = 1;

    [Header("Minigame Spawn Locations")]
    [SerializeField]
    Transform[] m_PossibleWireMinigameLocations;
    [SerializeField]
    Transform[] m_PossiblePipeMinigameLocations;

    [Header("Minigame Interactables")]
    [SerializeField]
    WireMinigameInteractable m_WireMinigameInteractablePrefab;
    [SerializeField]
    WallKnockInteractable m_WallKnockInteractablePrefab;

    // ^^^ Minigames ^^^

    [Header("Door")]
    [SerializeField]
    Door m_Door;
    [SerializeField]
    ChaseMinigameInteract m_ChaseInteract;
    [field: SerializeField]
    public Transform HouseTeleportSpot { get; private set; }
    [field: SerializeField]
    public Transform OutsideTeleportSpot { get; private set; }

    [Header("Lights")]
    [SerializeField]
    CeilingLight[] m_HouseLights;

    [Header("Waypoint Icons")]
    [SerializeField]
    Sprite m_TalkWaypointIcon;
    [SerializeField]
    Vector3 m_TalkWaypointOffset = new Vector3(0f, 1f);
    [SerializeField]
    Sprite m_TaskWaypointIcon;
    [SerializeField]
    Vector3 m_TaskWaypointOffset = Vector3.zero;
    [SerializeField]
    Sprite m_ExitWaypointIcon;
    [SerializeField]
    Vector3 m_ExitWaypointOffset = Vector3.zero;

    public Transform EntryPoint => m_Door.transform;
    public HouseProgress Progress { get; private set; }

    class HouseTaskEntry
    {
        public Transform Target;
        public MinigameType Type;
        public DisplayTask DisplayTask;
    }

    Transform ActiveDoorTransform => m_ChaseInteract.gameObject.activeSelf ? m_ChaseInteract.transform : m_Door.transform;

    Transform m_CurrentNPCTransform;
    bool m_bHouseCompletionReported;
    readonly List<GameObject> m_SpawnedInteriorObjects = new List<GameObject>();
    readonly List<HouseTaskEntry> m_TaskEntries = new List<HouseTaskEntry>();
    DisplayTask m_TalkDisplayTask;
    DisplayTask m_ExitDisplayTask;

    public void LoadHouse()
    {
        m_Door.OwningHouse = this;
        m_ChaseInteract.OwningHouse = this;
        m_Door.ResetForNewDay();
        m_ChaseInteract.ResetForNewDay();
        Progress = HouseProgressTracker.GetOrRegisterHouse(transform.position, GenerateHouseMinigamePlan);
        if (Progress.IsPlayerInside) ResumeInsideHouse();
        else ShowExteriorDoor();
    }

    public void MarkNotNeededToday()
    {
        m_Door.OwningHouse = this;
        m_ChaseInteract.OwningHouse = this;
        m_Door.ResetForNewDay();
        m_ChaseInteract.ResetForNewDay();
        ShowExteriorDoor();
        m_Door.CanInteract = false;
        Progress = null;
    }

    void ResumeInsideHouse()
    {
        HouseProgressTracker.SetActiveHouse(transform.position);
        SpawnNPC();
        SpawnMinigameInteractables();
        RefreshLights();
        ShowInteriorDoorForCurrentProgress();
        RefreshTaskAndTalkMarkers();
        RefreshExitDoorMarker();
    }

    public void UnloadHouse()
    {
        foreach (GameObject spawnedObject in m_SpawnedInteriorObjects) if (spawnedObject) Destroy(spawnedObject);
        m_SpawnedInteriorObjects.Clear();
        ClearAllTaskEntries();
        m_TalkDisplayTask = null;
        m_ExitDisplayTask = null;
        m_CurrentNPCTransform = null;
        m_bHouseCompletionReported = false;
        Progress = null;
    }

    void Update()
    {
        if (Progress == null || !Progress.IsPlayerInside) return;
        MinigameType? completed = HouseProgressTracker.ConsumePendingCompletedMinigame(transform.position);
        if (completed.HasValue) OnMinigameCompleted(completed.Value);
    }

    #region Plan Generation
    MinigameType[] GenerateHouseMinigamePlan()
    {
        int wireCount, wallKnockCount, chaseCount;
        do
        {
            wireCount = Random.Range(0, m_MaxWireMinigames + 1);
            wallKnockCount = Random.Range(0, m_MaxWallKnockMinigames + 1);
            chaseCount = Random.Range(0, m_MaxChaseMinigames + 1);
        } while (wireCount + wallKnockCount + chaseCount < m_MinTotalMinigames);
        List<MinigameType> plan = new List<MinigameType>();
        for (int i = 0; i < wireCount; ++i) plan.Add(MinigameType.Wires);
        for (int i = 0; i < wallKnockCount; ++i) plan.Add(MinigameType.WallKnockAndPipes);
        for (int i = 0; i < chaseCount; ++i) plan.Add(MinigameType.ChaseMinigame);
        return plan.ToArray();
    }
    #endregion Plan Generation

    #region Door
    bool HasUnbeatenChase()
    {
        for (int i = 0; i < Progress.MinigameTypes.Length; ++i)
        {
            if (Progress.MinigameTypes[i] == MinigameType.ChaseMinigame && !Progress.MinigamesBeaten[i]) return true;
        }
        return false;
    }

    void ShowExteriorDoor()
    {
        m_ChaseInteract.gameObject.SetActive(false);
        m_Door.gameObject.SetActive(true);
        m_Door.DoorType = DoorType.Entry;
    }

    void ShowInteriorDoorForCurrentProgress()
    {
        if (HasUnbeatenChase())
        {
            m_Door.gameObject.SetActive(false);
            m_ChaseInteract.gameObject.SetActive(true);
            if (!HasTaskEntryOfType(MinigameType.ChaseMinigame)) RegisterTaskEntry(m_ChaseInteract.transform, MinigameType.ChaseMinigame);
        }
        else
        {
            m_ChaseInteract.gameObject.SetActive(false);
            m_Door.gameObject.SetActive(true);
            m_Door.DoorType = DoorType.Exit;
            Progress.DoorType = DoorType.Exit;
        }
    }
    #endregion Door

    #region Spawning
    void SpawnNPC()
    {
        if (!Arrays.IsValid(m_PossibleNPCs) || !Arrays.IsValid(m_PossibleNPCLocations))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: {name} has no possible NPCs and/or NPC locations set up");
#endif
            return;
        }
        NPC npcPrefab = m_PossibleNPCs.GetRandomElement<NPC>();
        Transform spawnLocation = m_PossibleNPCLocations.GetRandomElement<Transform>();
        NPC spawnedNPC = Instantiate(npcPrefab, spawnLocation.position, spawnLocation.rotation);
        spawnedNPC.OwningHouse = this;
        m_SpawnedInteriorObjects.Add(spawnedNPC.gameObject);
        m_CurrentNPCTransform = spawnedNPC.transform;
    }

    void SpawnMinigameInteractables()
    {
        SpawnInteractablesOfType(MinigameType.Wires, m_PossibleWireMinigameLocations, m_WireMinigameInteractablePrefab);
        SpawnInteractablesOfType(MinigameType.WallKnockAndPipes, m_PossiblePipeMinigameLocations, m_WallKnockInteractablePrefab);
    }

    void SpawnInteractablesOfType<T>(MinigameType type, Transform[] possibleLocations, T prefab) where T : Component, IHouseTaskInteractable
    {
        int count = 0;
        for (int i = 0; i < Progress.MinigameTypes.Length; ++i) if (Progress.MinigameTypes[i] == type && !Progress.MinigamesBeaten[i]) ++count;
        if (count <= 0) return;
        if (!Arrays.IsValid(possibleLocations) || !prefab)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: {name} needs {count} {type} minigame location(s) and a prefab assigned");
#endif
            return;
        }
        Transform[] shuffledLocations = (Transform[])possibleLocations.Clone();
        shuffledLocations.Shuffle();
        int spawnCount = Mathf.Min(count, shuffledLocations.Length);
        for (int i = 0; i < spawnCount; ++i)
        {
            T instance = Instantiate(prefab, shuffledLocations[i].position, shuffledLocations[i].rotation);
            instance.OwningHouse = this;
            m_SpawnedInteriorObjects.Add(instance.gameObject);
            RegisterTaskEntry(instance.transform, type);
        }
    }
    #endregion Spawning

    static string TaskDisplayNameFor(MinigameType type) => type switch
    {
        MinigameType.Wires => "Fix the wiring",
        MinigameType.WallKnockAndPipes => "Find and patch the leak",
        MinigameType.ChaseMinigame => "Deal with the illegal connections",
        _ => "Do the task"
    };

    void RegisterTaskEntry(Transform target, MinigameType type)
    {
        m_TaskEntries.Add(new HouseTaskEntry
        {
            Target = target,
            Type = type,
            DisplayTask = new DisplayTask(TaskDisplayNameFor(type), 1, 0, strikeThroughOnCompletion: true)
        });
    }

    bool HasTaskEntryOfType(MinigameType type)
    {
        foreach (HouseTaskEntry entry in m_TaskEntries) if (entry.Type == type) return true;
        return false;
    }

    #region Lights
    void RefreshLights()
    {
        bool bAllWireTasksBeaten = true;
        for (int i = 0; i < Progress.MinigameTypes.Length; ++i)
        {
            if (Progress.MinigameTypes[i] == MinigameType.Wires && !Progress.MinigamesBeaten[i]) { bAllWireTasksBeaten = false; break; }
        }
        foreach (CeilingLight light in m_HouseLights)
        {
            if (!light) continue;
            if (bAllWireTasksBeaten) light.TurnOn();
            else light.TurnOff();
        }
    }
    #endregion Lights

    #region Markers
    void RefreshTaskAndTalkMarkers()
    {
        if (Progress.HasTalkedToNPC) RevealTaskEntries();
        else if (m_CurrentNPCTransform)
        {
            WaypointManager.Instance?.AddWaypoint(m_CurrentNPCTransform, m_TalkWaypointIcon, m_TalkWaypointOffset);
            m_TalkDisplayTask ??= new DisplayTask("Talk to the resident", 1, 0, false);
            TaskList.Instance?.AddTask(m_TalkDisplayTask);
        }
    }

    void RevealTaskEntries()
    {
        foreach (HouseTaskEntry entry in m_TaskEntries)
        {
            if (entry.Target) WaypointManager.Instance?.AddWaypoint(entry.Target, m_TaskWaypointIcon, m_TaskWaypointOffset);
            TaskList.Instance?.AddTask(entry.DisplayTask);
        }
    }

    void ShowRemainingTaskMarkers()
    {
        foreach (HouseTaskEntry entry in m_TaskEntries)
        {
            if (entry.Target && entry.DisplayTask.AmountDone == 0) WaypointManager.Instance?.AddWaypoint(entry.Target, m_TaskWaypointIcon);
        }
    }

    void HideAllTaskMarkers()
    {
        foreach (HouseTaskEntry entry in m_TaskEntries) if (entry.Target) WaypointManager.Instance?.RemoveWaypoint(entry.Target);
    }

    public void HideTaskMarker(Transform target) => WaypointManager.Instance?.RemoveWaypoint(target);

    void CompleteTaskEntry(MinigameType type)
    {
        foreach (HouseTaskEntry entry in m_TaskEntries)
        {
            if (entry.Type != type || entry.DisplayTask.AmountDone != 0) continue;
            TaskList.Instance?.IncrementAmountDoneForTask(entry.DisplayTask);
            if (entry.Target) WaypointManager.Instance?.RemoveWaypoint(entry.Target);
            break;
        }
    }

    void ClearAllTaskEntries()
    {
        foreach (HouseTaskEntry entry in m_TaskEntries)
        {
            if (entry.Target) WaypointManager.Instance?.RemoveWaypoint(entry.Target);
            TaskList.Instance?.RemoveTask(entry.DisplayTask);
        }
        m_TaskEntries.Clear();
    }

    void RefreshExitDoorMarker()
    {
        if (Progress.AllMinigamesBeaten)
        {
            ClearAllTaskEntries();
            WaypointManager.Instance?.AddWaypoint(ActiveDoorTransform, m_ExitWaypointIcon, m_ExitWaypointOffset);
            m_ExitDisplayTask ??= new DisplayTask("Head back outside", 1, 0, false);
            TaskList.Instance?.AddTask(m_ExitDisplayTask);
            ReportHouseCompletedOnce();
        }
        else
        {
            WaypointManager.Instance?.RemoveWaypoint(ActiveDoorTransform);
            if (m_ExitDisplayTask != null) TaskList.Instance?.RemoveTask(m_ExitDisplayTask);
        }
    }

    void ReportHouseCompletedOnce()
    {
        if (m_bHouseCompletionReported) return;
        m_bHouseCompletionReported = true;
        NPCHouseDailyManager.Instance?.ReportHouseCompleted();
    }

    public void OnNPCTalkedTo()
    {
        if (Progress.HasTalkedToNPC) return;
        Progress.HasTalkedToNPC = true;
        if (m_CurrentNPCTransform) WaypointManager.Instance?.RemoveWaypoint(m_CurrentNPCTransform);
        if (m_TalkDisplayTask != null) TaskList.Instance?.RemoveTask(m_TalkDisplayTask);
        RevealTaskEntries();
        RefreshExitDoorMarker();
    }

    public void OnChaseTaskStarted(Transform chaseDoorTransform)
    {
        HideTaskMarker(chaseDoorTransform);
        HideAllTaskMarkers();
    }
    #endregion Markers

    void OnMinigameCompleted(MinigameType type)
    {
        Progress.MarkMinigameBeaten(type);
        if (type == MinigameType.Wires) RefreshLights();
        CompleteTaskEntry(type);
        if (type == MinigameType.ChaseMinigame)
        {
            ShowInteriorDoorForCurrentProgress();
            ShowRemainingTaskMarkers();
        }
        RefreshExitDoorMarker();
    }

    public void EnterHouse()
    {
        Progress.IsPlayerInside = true;
        HouseProgressTracker.SetActiveHouse(transform.position);
        NPCHouseDailyManager.Instance?.HideAllHouseMarkers();
        SpawnNPC();
        SpawnMinigameInteractables();
        RefreshLights();
        ShowInteriorDoorForCurrentProgress();
        RefreshTaskAndTalkMarkers();
        RefreshExitDoorMarker();
    }

    public void ExitHouse()
    {
        Progress.IsPlayerInside = false;
        HouseProgressTracker.SetActiveHouse(null);
        ClearAllTaskEntries();
        if (m_CurrentNPCTransform) WaypointManager.Instance?.RemoveWaypoint(m_CurrentNPCTransform);
        if (m_TalkDisplayTask != null) TaskList.Instance?.RemoveTask(m_TalkDisplayTask);
        WaypointManager.Instance?.RemoveWaypoint(ActiveDoorTransform);
        if (m_ExitDisplayTask != null) TaskList.Instance?.RemoveTask(m_ExitDisplayTask);
        foreach (GameObject spawnedObject in m_SpawnedInteriorObjects) if (spawnedObject) Destroy(spawnedObject);
        m_SpawnedInteriorObjects.Clear();
        m_CurrentNPCTransform = null;
        NPCHouseDailyManager.Instance?.ShowMarkersForIncompleteHouses();
    }
}
