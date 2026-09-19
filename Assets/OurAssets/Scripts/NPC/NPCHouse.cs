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

    public HouseProgress Progress { get; private set; }

    GameObject m_ActiveDoorObject;
    readonly List<GameObject> m_SpawnedInteriorObjects = new List<GameObject>();

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
        SpawnDoorSpotForCurrentProgress();
    }

    public void UnloadHouse()
    {
        if (m_ActiveDoorObject) Destroy(m_ActiveDoorObject);
        m_ActiveDoorObject = null;
        foreach (GameObject spawnedObject in m_SpawnedInteriorObjects) if (spawnedObject) Destroy(spawnedObject);
        m_SpawnedInteriorObjects.Clear();
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
            m_ActiveDoorObject = chaseInteract.gameObject;
            m_SpawnedInteriorObjects.Add(m_ActiveDoorObject);
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
    }

    void SpawnMinigameInteractables()
    {
        SpawnInteractablesOfType(MinigameType.Wires, m_PossibleWireMinigameLocations, m_WireMinigameInteractablePrefab);
        SpawnInteractablesOfType(MinigameType.WallKnockAndPipes, m_PossiblePipeMinigameLocations, m_WallKnockInteractablePrefab);
    }

    void SpawnInteractablesOfType<T>(MinigameType type, Transform[] possibleLocations, T prefab) where T : Component
    {
        int count = 0; // Only the ones not already beaten need spawning, matters if this house's interior is ever rebuilt after some were already completed
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
            m_SpawnedInteriorObjects.Add(instance.gameObject);
        }
    }
    #endregion Spawning

    void OnMinigameCompleted(MinigameType type)
    {
        Progress.MarkMinigameBeaten(type);
        if (type == MinigameType.ChaseMinigame) ReplaceChaseInteractableWithExitDoor();
    }

    public void EnterHouse(Door doorUsedToEnter)
    {
        Progress.IsPlayerInside = true;
        HouseProgressTracker.SetActiveHouse(transform.position);
        SpawnNPC();
        SpawnMinigameInteractables();
        if (HasUnbeatenChase())
        {
            Vector3 position = doorUsedToEnter.transform.position;
            Quaternion rotation = doorUsedToEnter.transform.rotation;
            Destroy(doorUsedToEnter.gameObject);

            ChaseMinigameInteract chaseInteract = Instantiate(m_ChaseMinigameInteractPrefab, position, rotation);
            chaseInteract.ChaseSpawn = OutsideTeleportSpot;
            chaseInteract.ReturnSpawn = HouseTeleportSpot;
            m_ActiveDoorObject = chaseInteract.gameObject;
            m_SpawnedInteriorObjects.Add(chaseInteract.gameObject);
        }
        else
        {
            doorUsedToEnter.DoorType = DoorType.Exit;
            Progress.DoorType = DoorType.Exit;
            m_ActiveDoorObject = doorUsedToEnter.gameObject;
        }
    }

    public void ExitHouse()
    {
        Progress.IsPlayerInside = false;
        HouseProgressTracker.SetActiveHouse(null);
        foreach (GameObject spawnedObject in m_SpawnedInteriorObjects) if (spawnedObject) Destroy(spawnedObject);
        m_SpawnedInteriorObjects.Clear();
    }
}
