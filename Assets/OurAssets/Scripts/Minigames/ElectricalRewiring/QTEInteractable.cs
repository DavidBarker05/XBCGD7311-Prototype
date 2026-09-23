using UnityEngine;
using UnityEngine.Events;

public class QTEInteractable : Interactable
{
    private bool hasTriggered = false;
    private Player player;

    private PlayerCharacter lastPlayer;

    public bool CanInteract { get; set; } = true;
    public UnityEvent OnCompleted;

    public override bool CanInteractWith => CanInteract && !hasTriggered;

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (!CanInteract)
        {
            return new InteractionStatus() { EndInteraction = true };
        }

        if (inputParameters.Length != 2)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: QTEInteractable objects needs 2 input parameters. Received {inputParameters.Length} input parameters");
#endif
        }
        else
        {
            if (inputParameters[0] is Player player && inputParameters[1] is PlayerCharacter currentPlayer)
            {
                if (!hasTriggered)
                {
                    hasTriggered = true;
                    this.player = player;
                    lastPlayer = currentPlayer;
                    QTEManager.Instance.StartQTE(this);
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"WARNING: Input parameter 0 needs to be a QTEPlayerCharacter. Received {inputParameters[0]} type {inputParameters[0].GetType()} as input parameter 0");
#endif
            }
        }
        return new InteractionStatus() { EndInteraction = true };
    }

    public void OnQTESuccess()
    {
        Debug.Log("SUCCESS - Objective completed");
        player.ChangeCharacter(lastPlayer);
        player = null;
        lastPlayer = null;
        ChaseMinigameStarter.Instance.InteractableBeaten();
        OnCompleted.Invoke();
        gameObject.SetActive(false);
    }

    public void OnQTEFailure()
    {
        Debug.Log("FAILURE - Try again");
        player.ChangeCharacter(lastPlayer);
        player = null;
        lastPlayer = null;
        hasTriggered = false;
    }
}