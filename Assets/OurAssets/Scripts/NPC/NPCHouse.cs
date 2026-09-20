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
    [SerializeField]
    ChaseMinigameInteract m_ChaseMinigameInteractPrefab;

    // ^^^ Minigames ^^^

    [Header("Door")]
    [SerializeField]
    Transform m_DoorSpawnLocation;
    [SerializeField]
    Door m_NonChaseDoorPrefab;
    [field: SerializeField]
    public Transform HouseTeleportSpot { get; private set; }
    [field: SerializeField]
    public Transform OutsideTeleportSpot { get; private set; }

    [Header("Lights")]
    [SerializeField]
    Light[] m_HouseLights;

    [Header("Waypoint Icons")]
    [SerializeField]
    Sprite m_TalkWaypointIcon;
    [SerializeField]
    Sprite m_TaskWaypointIcon;
    [SerializeField]
    Sprite m_ExitWaypointIcon;

    public Transform EntryPoint => m_DoorSpawnLocation;
    public HouseProgress Progress { get; private set; }

    class HouseTaskEntry
    {
        public Transform Target;
        public MinigameType Type;
        public DisplayTask DisplayTask;
    }

    GameObject m_ActiveDoorObject;
    Transform m_CurrentNPCTransform;
    bool m_bHouseCompletionReported;
    readonly List<GameObject> m_SpawnedInteriorObjects = new List<GameObject>();
    readonly List<HouseTaskEntry> m_TaskEntries = new List<HouseTaskEntry>();
    DisplayTask m_TalkDisplayTask;
    DisplayTask m_ExitDisplayTask;

    public void LoadHouse()
    {
        Progress = HouseProgressTracker.GetOrRegisterHouse(transform.position, GenerateHouseMinigamePlan);
        if (Progress.IsPlayerInside) ResumeInsideHouse();
        else SpawnDoor();
    }

    void ResumeInsideHouse()
    {
        HouseProgressTracker.SetActiveHouse(transform.position);
        SpawnNPC();
        SpawnMinigameInteractables();
        RefreshLights();
        SpawnDoorSpotForCurrentProgress();
        RefreshTaskAndTalkMarkers();
        RefreshExitDoorMarker();
    }

    public void UnloadHouse()
    {
        if (m_ActiveDoorObject) Destroy(m_ActiveDoorObject);
        m_ActiveDoorObject = null;
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

    void SpawnDoor()
    {
        Door door = Instantiate(m_NonChaseDoorPrefab, m_DoorSpawnLocation.position, m_DoorSpawnLocation.rotation);
        door.OwningHouse = this;
        door.DoorType = DoorType.Entry;
        m_ActiveDoorObject = door.gameObject;
    }

    void SpawnDoorSpotForCurrentProgress()
    {
        if (HasUnbeatenChase())
        {
            ChaseMinigameInteract chaseInteract = Instantiate(m_ChaseMinigameInteractPrefab, m_DoorSpawnLocation.position, m_DoorSpawnLocation.rotation);
            chaseInteract.ChaseSpawn = OutsideTeleportSpot;
            chaseInteract.ReturnSpawn = HouseTeleportSpot;
            chaseInteract.OwningHouse = this;
            m_ActiveDoorObject = chaseInteract.gameObject;
            m_SpawnedInteriorObjects.Add(m_ActiveDoorObject);
            RegisterTaskEntry(chaseInteract.transform, MinigameType.ChaseMinigame);
        }
        else
        {
            Door exitDoor = Instantiate(m_NonChaseDoorPrefab, m_DoorSpawnLocation.position, m_DoorSpawnLocation.rotation);
            exitDoor.OwningHouse = this;
            exitDoor.DoorType = DoorType.Exit;
            Progress.DoorType = DoorType.Exit;
            m_ActiveDoorObject = exitDoor.gameObject;
        }
    }

    void ReplaceChaseInteractableWithExitDoor()
    {
        m_ActiveDoorObject.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
        m_SpawnedInteriorObjects.Remove(m_ActiveDoorObject);
        Destroy(m_ActiveDoorObject);
        Door exitDoor = Instantiate(m_NonChaseDoorPrefab, position, rotation);
        exitDoor.OwningHouse = this;
        exitDoor.DoorType = DoorType.Exit;
        Progress.DoorType = DoorType.Exit;
        m_ActiveDoorObject = exitDoor.gameObject;
        m_SpawnedInteriorObjects.Add(exitDoor.gameObject);
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

    #region Lights
    void RefreshLights()
    {
        bool bAllWireTasksBeaten = true;
        for (int i = 0; i < Progress.MinigameTypes.Length; ++i)
        {
            if (Progress.MinigameTypes[i] == MinigameType.Wires && !Progress.MinigamesBeaten[i]) { bAllWireTasksBeaten = false; break; }
        }
        foreach (Light light in m_HouseLights) if (light) light.enabled = bAllWireTasksBeaten;
    }
    #endregion Lights

    #region Markers
    void RefreshTaskAndTalkMarkers()
    {
        if (Progress.HasTalkedToNPC) RevealTaskEntries();
        else if (m_CurrentNPCTransform)
        {
            WaypointManager.Instance?.AddWaypoint(m_CurrentNPCTransform, m_TalkWaypointIcon);
            m_TalkDisplayTask ??= new DisplayTask("Talk to the resident", 1, 0, false);
            TaskList.Instance?.AddTask(m_TalkDisplayTask);
        }
    }

    void RevealTaskEntries()
    {
        foreach (HouseTaskEntry entry in m_TaskEntries)
        {
            if (entry.Target) WaypointManager.Instance?.AddWaypoint(entry.Target, m_TaskWaypointIcon);
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
        if (!m_ActiveDoorObject) return;
        if (Progress.AllMinigamesBeaten)
        {
            ClearAllTaskEntries();
            WaypointManager.Instance?.AddWaypoint(m_ActiveDoorObject.transform, m_ExitWaypointIcon);
            m_ExitDisplayTask ??= new DisplayTask("Head back outside", 1, 0, false);
            TaskList.Instance?.AddTask(m_ExitDisplayTask);
            ReportHouseCompletedOnce();
        }
        else
        {
            WaypointManager.Instance?.RemoveWaypoint(m_ActiveDoorObject.transform);
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
            ReplaceChaseInteractableWithExitDoor();
            ShowRemainingTaskMarkers();
        }
        RefreshExitDoorMarker();
    }

    public void EnterHouse(Door doorUsedToEnter)
    {
        Progress.IsPlayerInside = true;
        HouseProgressTracker.SetActiveHouse(transform.position);
        NPCHouseDailyManager.Instance?.HideAllHouseMarkers();
        SpawnNPC();
        SpawnMinigameInteractables();
        RefreshLights();
        if (HasUnbeatenChase())
        {
            Vector3 position = doorUsedToEnter.transform.position;
            Quaternion rotation = doorUsedToEnter.transform.rotation;
            Destroy(doorUsedToEnter.gameObject);
            ChaseMinigameInteract chaseInteract = Instantiate(m_ChaseMinigameInteractPrefab, position, rotation);
            chaseInteract.ChaseSpawn = OutsideTeleportSpot;
            chaseInteract.ReturnSpawn = HouseTeleportSpot;
            chaseInteract.OwningHouse = this;
            m_ActiveDoorObject = chaseInteract.gameObject;
            m_SpawnedInteriorObjects.Add(chaseInteract.gameObject);
            RegisterTaskEntry(chaseInteract.transform, MinigameType.ChaseMinigame);
        }
        else
        {
            doorUsedToEnter.DoorType = DoorType.Exit;
            Progress.DoorType = DoorType.Exit;
            m_ActiveDoorObject = doorUsedToEnter.gameObject;
        }
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
        if (m_ActiveDoorObject) WaypointManager.Instance?.RemoveWaypoint(m_ActiveDoorObject.transform);
        if (m_ExitDisplayTask != null) TaskList.Instance?.RemoveTask(m_ExitDisplayTask);
        foreach (GameObject spawnedObject in m_SpawnedInteriorObjects) if (spawnedObject) Destroy(spawnedObject);
        m_SpawnedInteriorObjects.Clear();
        m_CurrentNPCTransform = null;
        NPCHouseDailyManager.Instance?.ShowMarkersForIncompleteHouses();
    }
}
