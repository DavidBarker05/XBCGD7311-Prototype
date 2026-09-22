using System.Collections.Generic;
using UnityEngine;

public class QTECheckpointManager : MonoBehaviour
{
    [Header("Checkpoints")]
    public List<QTEInteractable> checkpoints = new List<QTEInteractable>();

    [Header("Marker Settings")]
    public Sprite markerIcon;
    public float markerHeightOffset = 2f;

    [Header("Task List")]
    public string taskDisplayName = "Deal with the illegal connections";

    readonly HashSet<QTEInteractable> completedCheckpoints = new HashSet<QTEInteractable>();

    readonly List<QTEInteractable> m_DynamicCheckpoints = new List<QTEInteractable>();

    DisplayTask m_ChaseDisplayTask;

    void Awake()
    {
        if (checkpoints.Count == 0) return;

        foreach (QTEInteractable checkpoint in checkpoints)
        {
            QTEInteractable capturedCheckpoint = checkpoint;
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
            checkpoint.CanInteract = true;
            SpawnMarkerFor(checkpoint);
        }
        foreach (QTEInteractable checkpoint in m_DynamicCheckpoints)
        {
            checkpoint.CanInteract = true;
            SpawnMarkerFor(checkpoint);
        }

        ClearChaseTask();
        int totalCount = checkpoints.Count + m_DynamicCheckpoints.Count;
        m_ChaseDisplayTask = new DisplayTask(taskDisplayName, totalCount, 0, false);
        TaskList.Instance?.AddTask(m_ChaseDisplayTask);
    }

    public void ClearChaseTask()
    {
        if (m_ChaseDisplayTask == null) return;
        TaskList.Instance?.RemoveTask(m_ChaseDisplayTask);
        m_ChaseDisplayTask = null;
    }

    void OnCheckpointCompleted(QTEInteractable checkpoint)
    {
        completedCheckpoints.Add(checkpoint);
        ClearMarkerFor(checkpoint);
        if (m_ChaseDisplayTask != null) TaskList.Instance?.IncrementAmountDoneForTask(m_ChaseDisplayTask);

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
