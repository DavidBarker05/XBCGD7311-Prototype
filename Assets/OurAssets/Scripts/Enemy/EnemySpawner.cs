using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints; // assign 3 in the Inspector

    public float spawnInterval = 60f;   // 1 minute
    public float maxDuration = 540f;    // 9 minutes
    public int maxEnemies = 10;

    private int spawnedCount = 0;
    private float elapsedTime = 0f;
    private Coroutine spawnRoutine;

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
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
            spawnedCount = 0;
            elapsedTime = 0f;
        }
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
        GameObject enemyObj = Instantiate(enemyPrefab, point.position, point.rotation);

        ChasePlayer chaseScript = enemyObj.GetComponent<ChasePlayer>();
        if (chaseScript != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                chaseScript.player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("No GameObject tagged 'Player' found in scene!");
            }
        }
        else
        {
            Debug.LogWarning("Spawned enemy prefab is missing the ChasePlayer component!");
        }
    }
}