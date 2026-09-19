using System.Collections.Generic;
using UnityEngine;

public class QTECheckpointManager : MonoBehaviour
{
    [Header("Checkpoints (can be completed in any order)")]
    public List<QTEInteractable> checkpoints = new List<QTEInteractable>();

    [Header("Marker Settings")]
    public Sprite markerIcon; // Uses WaypointManager's default icon if left unset
    public float markerHeightOffset = 2f;

    readonly HashSet<QTEInteractable> completedCheckpoints = new HashSet<QTEInteractable>();

    readonly List<QTEInteractable> m_DynamicCheckpoints = new List<QTEInteractable>();

    void Awake()
    {
        if (checkpoints.Count == 0) return;

        foreach (QTEInteractable checkpoint in checkpoints)
        {
            QTEInteractable capturedCheckpoint = checkpoint; // Local copy so each listener closes over its own checkpoint, not whichever one the loop variable ends on
            capturedCheckpoint.OnCompleted.AddListener(() => OnCheckpointCompleted(capturedCheckpoint));
        }
    }

    public void RegisterDynamicCheckpoint(QTEInteractable checkpoint)
    {
        QTEInteractable capturedCheckpoint = checkpoint;
        capturedCheckpoint.OnCompleted.AddListener(() => OnCheckpointCompleted(capturedCheckpoint));
        m_DynamicCheckpoints.Add(capturedCheckpoint);
    }

    public void ClearDynamicCheckpoints()
    {
        foreach (QTEInteractable checkpoint in m_DynamicCheckpoints) ClearMarkerFor(checkpoint);
        m_DynamicCheckpoints.Clear();
    }

    public void BeginCheckpoints()
    {
        ClearAllMarkers();
        completedCheckpoints.Clear();
        foreach (QTEInteractable checkpoint in checkpoints)
        {
            checkpoint.canInteract = true;
            SpawnMarkerFor(checkpoint);
        }
        foreach (QTEInteractable checkpoint in m_DynamicCheckpoints)
        {
            checkpoint.canInteract = true;
            SpawnMarkerFor(checkpoint);
        }
    }

    void OnCheckpointCompleted(QTEInteractable checkpoint)
    {
        completedCheckpoints.Add(checkpoint);
        ClearMarkerFor(checkpoint);

        if (completedCheckpoints.Count >= checkpoints.Count + m_DynamicCheckpoints.Count) Debug.Log("QTECheckpointManager: All checkpoints complete!");
    }

    void SpawnMarkerFor(QTEInteractable target)
    {
        if (target == null || WaypointManager.Instance == null) return;
        WaypointManager.Instance.AddWaypoint(target.transform, markerIcon, Vector3.up * markerHeightOffset);
    }

    void ClearMarkerFor(QTEInteractable target)
    {
        if (target == null || WaypointManager.Instance == null) return;
        WaypointManager.Instance.RemoveWaypoint(target.transform);
    }

    void ClearAllMarkers()
    {
        foreach (QTEInteractable checkpoint in checkpoints) ClearMarkerFor(checkpoint);
        foreach (QTEInteractable checkpoint in m_DynamicCheckpoints) ClearMarkerFor(checkpoint);
    }
}
