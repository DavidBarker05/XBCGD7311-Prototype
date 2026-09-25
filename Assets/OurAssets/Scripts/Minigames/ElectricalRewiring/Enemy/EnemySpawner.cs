using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    public ChasePlayer[] enemyPrefabs;
    public Transform[] spawnPoints;

    public float spawnInterval = 30f;   // 30 seconds
    public float maxDuration = 540f;    // 9 minutes
    public int maxEnemies = 10;

    [Header("Chunking")]
    [SerializeField, Min(1f)]
    float m_ChunkSize = 30f;
    [SerializeField, Min(0)]
    int m_MinChunkDistanceFromPlayer = 2;

    private int spawnedCount = 0;
    private float elapsedTime = 0f;
    private Coroutine spawnRoutine;
    private readonly List<ChasePlayer> spawnedEnemies = new List<ChasePlayer>();

    readonly Dictionary<Vector2Int, List<Transform>> m_ChunkMap = new Dictionary<Vector2Int, List<Transform>>();

    FirstPersonPlayerCharacter playerObj;

    void Start()
    {
        playerObj = FindAnyObjectByType<FirstPersonPlayerCharacter>();
        BuildChunkMap();
    }

    void BuildChunkMap()
    {
        m_ChunkMap.Clear();
        foreach (Transform point in spawnPoints)
        {
            if (!point) continue;
            Vector2Int chunk = ChunkCoordFor(point.position);
            if (!m_ChunkMap.TryGetValue(chunk, out List<Transform> pointsInChunk))
            {
                pointsInChunk = new List<Transform>();
                m_ChunkMap[chunk] = pointsInChunk;
            }
            pointsInChunk.Add(point);
        }
    }

    Vector2Int ChunkCoordFor(Vector3 worldPosition) =>
        new Vector2Int(Mathf.FloorToInt(worldPosition.x / m_ChunkSize), Mathf.FloorToInt(worldPosition.z / m_ChunkSize));

    private void Update()
    {
        if (ChaseMinigameStarter.Instance.ChaseMinigameIsRunning)
        {
            if (spawnRoutine == null)
            {
                spawnRoutine = StartCoroutine(SpawnLoop());
            }
        }
        else
        {
            StopSpawning();
        }
    }

    void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
        spawnedCount = 0;
        elapsedTime = 0f;
    }

    public void ResetSpawner()
    {
        StopSpawning();
        foreach (ChasePlayer enemy in spawnedEnemies) if (enemy) Destroy(enemy.gameObject);
        spawnedEnemies.Clear();
    }

    private IEnumerator SpawnLoop()
    {
        while (elapsedTime < maxDuration && spawnedCount < maxEnemies)
        {
            SpawnEnemy();
            spawnedCount++;

            yield return new WaitForSeconds(spawnInterval);
            elapsedTime += spawnInterval;
        }

        spawnRoutine = null;
    }

    private void SpawnEnemy()
    {
        Transform point = ChooseSpawnPoint();
        if (!point)
        {
            Debug.LogWarning("No spawn point available to spawn an enemy at");
            return;
        }

        ChasePlayer enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], point.position, point.rotation);
        spawnedEnemies.Add(enemy);
        if (enemy != null)
        {
            if (playerObj != null)
            {
                enemy.Player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("No 'FirstPersonPlayerCharacter' found in scene!");
            }
        }
        else
        {
            Debug.LogWarning("Spawned enemy prefab is missing the ChasePlayer component!");
        }
    }

    Transform ChooseSpawnPoint()
    {
        if (m_ChunkMap.Count == 0) return null;
        Vector2Int playerChunk = playerObj ? ChunkCoordFor(playerObj.transform.position) : default;

        List<Vector2Int> candidates = ChunksMatching(playerChunk, distance => distance >= m_MinChunkDistanceFromPlayer, minimizeDistance: true);
        if (candidates.Count == 0)
        {
            // Map too small for any chunk to satisfy the minimum distance, fall back to the farthest one available.
            candidates = ChunksMatching(playerChunk, distance => true, minimizeDistance: false);
        }
        if (candidates.Count == 0) return null;

        List<Transform> pointsInChunk = m_ChunkMap[candidates[Random.Range(0, candidates.Count)]];
        return pointsInChunk[Random.Range(0, pointsInChunk.Count)];
    }

    List<Vector2Int> ChunksMatching(Vector2Int playerChunk, System.Func<int, bool> distanceFilter, bool minimizeDistance)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        int bestDistance = minimizeDistance ? int.MaxValue : int.MinValue;
        foreach (Vector2Int chunk in m_ChunkMap.Keys)
        {
            int distance = Mathf.Max(Mathf.Abs(chunk.x - playerChunk.x), Mathf.Abs(chunk.y - playerChunk.y));
            if (!distanceFilter(distance)) continue;
            bool bIsBetter = minimizeDistance ? distance < bestDistance : distance > bestDistance;
            if (bIsBetter)
            {
                bestDistance = distance;
                result.Clear();
                result.Add(chunk);
            }
            else if (distance == bestDistance)
            {
                result.Add(chunk);
            }
        }
        return result;
    }
}
