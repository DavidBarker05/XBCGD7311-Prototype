using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    public ChasePlayer[] enemyPrefabs;
    public Transform[] spawnPoints; // assign 3 in the Inspector

    public float spawnInterval = 60f;   // 1 minute
    public float maxDuration = 540f;    // 9 minutes
    public int maxEnemies = 10;

    private int spawnedCount = 0;
    private float elapsedTime = 0f;
    private Coroutine spawnRoutine;
    private readonly List<ChasePlayer> spawnedEnemies = new List<ChasePlayer>();

    FirstPersonPlayerCharacter playerObj;

    void Start()
    {
        playerObj = FindAnyObjectByType<FirstPersonPlayerCharacter>();
    }

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
        if (spawnedCount >= spawnPoints.Length)
        {
            Debug.LogWarning("Not enough spawn points assigned for spawnedCount: " + spawnedCount);
            return;
        }

        Transform point = spawnPoints[spawnedCount];
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
}
