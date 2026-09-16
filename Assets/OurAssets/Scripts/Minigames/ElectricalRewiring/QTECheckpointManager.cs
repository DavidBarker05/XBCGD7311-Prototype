using UnityEngine;
using System.Collections.Generic;

public class QTECheckpointManager : MonoBehaviour
{
    [Header("Checkpoints (in the order they should be completed)")]
    public List<QTEInteractable> checkpoints = new List<QTEInteractable>();

    [Header("Marker Settings")]
    public GameObject markerPrefab;
    public float markerHeightOffset = 2f;

    private int currentIndex = 0;
    private GameObject currentMarkerInstance;

    void Start()
    {
        if (checkpoints.Count == 0)
        {
            Debug.LogWarning("QTECheckpointManager: No checkpoints assigned.");
            return;
        }

        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].canInteract = (i == 0);
            checkpoints[i].OnCompleted.AddListener(OnCheckpointCompleted);
        }

        SpawnMarkerFor(checkpoints[currentIndex]);
    }

    void OnCheckpointCompleted()
    {
        ClearMarker();
        currentIndex++;

        if (currentIndex >= checkpoints.Count)
        {
            Debug.Log("QTECheckpointManager: All checkpoints complete!");
            return;
        }

        checkpoints[currentIndex].canInteract = true;
        SpawnMarkerFor(checkpoints[currentIndex]);
    }

    void SpawnMarkerFor(QTEInteractable target)
    {
        if (target == null || markerPrefab == null) return;

        Vector3 pos = target.transform.position + Vector3.up * markerHeightOffset;
        currentMarkerInstance = Instantiate(markerPrefab, pos, Quaternion.identity, target.transform);
    }

    void ClearMarker()
    {
        if (currentMarkerInstance != null)
        {
            Destroy(currentMarkerInstance);
        }
    }
}