using UnityEngine;
using UnityEngine.Events;

public class ChaseMinigameInteract : Interactable
{
	[SerializeField]
	bool m_CanBePlayedAgain = false;
	[SerializeField]
	QTEInteractable[] m_QTEInteractables;

	// Fired right when the chase actually starts lets other systems (waypoints, etc.)
	// react without this class needing to know about them
	public UnityEvent OnChaseStarted;

	bool m_HasBeenPlayed = false;

	public override InteractionStatus Interact(params object[] inputParameters)
	{
		if (inputParameters.Length != 0)
		{
#if UNITY_EDITOR
			Debug.LogWarning($"WARNING: ChaseMinigameInteract objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
		}
		else
		{
			if ((!m_HasBeenPlayed || m_CanBePlayedAgain) && !ChaseMinigameStarter.Instance.ChaseMinigameIsRunning)
			{
				ChaseMinigameStarter.Instance.StartChaseMinigame(m_QTEInteractables);
				OnChaseStarted?.Invoke();
			}
			m_HasBeenPlayed = true;
		}
		return new InteractionStatus() { EndInteraction = true };
	}
}
