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

    // Minigames vvv

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

    // Minigames ^^^

    [Header("Door")]
    [SerializeField]
    Transform m_DoorSpawnLocation;
    [SerializeField]
    Door m_NonChaseDoorPrefab;
    [field: SerializeField]
    public Transform HouseTeleportSpot { get; private set; }
    [field: SerializeField]
    public Transform OutsideTeleportSpot { get; private set; }

    public (MinigameType Minigame, bool IsDone)[] HouseMinigames { get; private set; }
    public bool AllMinigamesBeaten
    {
        get
        {
            foreach ((MinigameType _, bool IsDone) in HouseMinigames) if (!IsDone) return false;
            return true;
        }
    }

    GameObject m_ActiveDoorObject;
    readonly List<GameObject> m_SpawnedInteriorObjects = new List<GameObject>();
    bool m_bIsEntered;

    void Awake()
    {
        GenerateHouseMinigamePlan();
        SpawnEntryDoor();
    }

    void Update()
    {
        if (!m_bIsEntered) return;
        MinigameType? completed = HouseMinigameProgressTracker.ConsumePendingCompletedMinigame();
        if (completed.HasValue) OnMinigameCompleted(completed.Value);
    }

    #region Plan Generation
    void GenerateHouseMinigamePlan()
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

        HouseMinigames = new (MinigameType, bool)[plan.Count];
        for (int i = 0; i < plan.Count; ++i) HouseMinigames[i] = (plan[i], false);
    }

    int CountOfType(MinigameType type)
    {
        int count = 0;
        foreach ((MinigameType Minigame, bool IsDone) entry in HouseMinigames) if (entry.Minigame == type) ++count;
        return count;
    }

    void MarkMinigameCompleted(MinigameType type)
    {
        for (int i = 0; i < HouseMinigames.Length; ++i)
        {
            if (HouseMinigames[i].Minigame == type && !HouseMinigames[i].IsDone)
            {
                HouseMinigames[i] = (type, true);
                return;
            }
        }
    }
    #endregion Plan Generation

    #region Door
    void SpawnEntryDoor()
    {
        Door door = Instantiate(m_NonChaseDoorPrefab, m_DoorSpawnLocation.position, m_DoorSpawnLocation.rotation);
        door.OwningHouse = this;
        door.DoorType = DoorType.Entry;
        m_ActiveDoorObject = door.gameObject;
    }

    void ReplaceChaseInteractableWithExitDoor()
    {
        Vector3 position = m_ActiveDoorObject.transform.position;
        Quaternion rotation = m_ActiveDoorObject.transform.rotation;
        m_SpawnedInteriorObjects.Remove(m_ActiveDoorObject);
        Destroy(m_ActiveDoorObject);

        Door exitDoor = Instantiate(m_NonChaseDoorPrefab, position, rotation);
        exitDoor.OwningHouse = this;
        exitDoor.DoorType = DoorType.Exit;
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
        int count = CountOfType(type);
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
        MarkMinigameCompleted(type);
        if (type == MinigameType.ChaseMinigame) ReplaceChaseInteractableWithExitDoor();
    }

    public void EnterHouse(Door doorUsedToEnter)
    {
        m_bIsEntered = true;
        SpawnNPC();
        SpawnMinigameInteractables();

        if (CountOfType(MinigameType.ChaseMinigame) > 0)
        {
            Vector3 position = doorUsedToEnter.transform.position;
            Quaternion rotation = doorUsedToEnter.transform.rotation;
            Destroy(doorUsedToEnter.gameObject);

            ChaseMinigameInteract chaseInteract = Instantiate(m_ChaseMinigameInteractPrefab, position, rotation);
            m_ActiveDoorObject = chaseInteract.gameObject;
            m_SpawnedInteriorObjects.Add(chaseInteract.gameObject);
        }
        else
        {
            doorUsedToEnter.DoorType = DoorType.Exit;
            m_ActiveDoorObject = doorUsedToEnter.gameObject;
        }
    }

    public void ExitHouse()
    {
        m_bIsEntered = false;
        foreach (GameObject spawnedObject in m_SpawnedInteriorObjects) if (spawnedObject) Destroy(spawnedObject);
        m_SpawnedInteriorObjects.Clear();
    }
}
