using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string powerLineTerminalTag = "Interactable";

    private PowerLineReconnectInteractable currentPowerLineTerminal;
    private bool reconnectionPanelActive;

    private void Update()
    {
        if (reconnectionPanelActive)
        {
            return;
        }

        if (currentPowerLineTerminal != null && Input.GetKeyDown(interactKey))
        {
            bool reconnectionStarted = currentPowerLineTerminal.BeginReconnectionMinigame();
            if (reconnectionStarted)
            {
                reconnectionPanelActive = true;
            }
        }
    }

    public void NotifyReconnectionPanelClosed()
    {
        reconnectionPanelActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPowerLineTerminalCollider(other, out PowerLineReconnectInteractable terminal))
        {
            return;
        }

        currentPowerLineTerminal = terminal;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPowerLineTerminalCollider(other, out PowerLineReconnectInteractable terminal))
        {
            return;
        }

        if (currentPowerLineTerminal == terminal)
        {
            currentPowerLineTerminal = null;
        }
    }

    private bool IsPowerLineTerminalCollider(Collider other, out PowerLineReconnectInteractable terminal)
    {
        terminal = null;

        if (!string.IsNullOrEmpty(powerLineTerminalTag) && !other.CompareTag(powerLineTerminalTag))
        {
            return false;
        }

        terminal = other.GetComponentInParent<PowerLineReconnectInteractable>();
        return terminal != null;
    }
}