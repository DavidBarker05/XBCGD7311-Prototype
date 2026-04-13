using UnityEngine;
using UnityEngine.Events;

public class PowerLineReconnectInteractable : MonoBehaviour
{
    [Header("Difficulty")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField][Range(5f, 180f)] private float successZoneSize = 40f;

    [Header("References")]
    [SerializeField] private PowerLineReconnectMinigameUI powerLineReconnectMinigameUI;

    [Header("Result Events")]
    [SerializeField] private UnityEvent onPowerLinesRestored;
    [SerializeField] private UnityEvent onReconnectionFailed;

    private bool isReconnectionInProgress;

    public float RotationSpeed => rotationSpeed;
    public float SuccessZoneSize => successZoneSize;

    public bool BeginReconnectionMinigame()
    {
        if (isReconnectionInProgress || powerLineReconnectMinigameUI == null || powerLineReconnectMinigameUI.IsReconnectionPanelOpen)
        {
            return false;
        }

        isReconnectionInProgress = true;
        powerLineReconnectMinigameUI.StartReconnectionMinigame(this);
        return true;
    }

    public void ResolveReconnectionAttempt(bool linesRestored)
    {
        isReconnectionInProgress = false;

        if (linesRestored)
        {
            onPowerLinesRestored?.Invoke();
        }
        else
        {
            onReconnectionFailed?.Invoke();
        }
    }
}
