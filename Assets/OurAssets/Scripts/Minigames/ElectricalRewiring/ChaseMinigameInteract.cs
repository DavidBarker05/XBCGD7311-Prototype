using UnityEngine;
using UnityEngine.Events;

public class ChaseMinigameInteract : Interactable
{
	[SerializeField]
	bool m_CanBePlayedAgain = false;
	[SerializeField]
	QTEInteractable[] m_QTEInteractables;

	[Header("Per-house teleport spots")]
	[SerializeField]
	Transform m_ChaseSpawn; // Where the player is teleported to when the chase starts (eg. outside the house)
	[SerializeField]
	Transform m_ReturnSpawn; // Where the player is teleported back to once the chase is beaten (eg. inside the house)

	public Transform ChaseSpawn { get => m_ChaseSpawn; set => m_ChaseSpawn = value; }
	public Transform ReturnSpawn { get => m_ReturnSpawn; set => m_ReturnSpawn = value; }

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
#if UNITY_EDITOR
				if (!m_ChaseSpawn || !m_ReturnSpawn) Debug.LogWarning($"WARNING: {name} is missing its chase spawn and/or return spawn transform");
#endif
				ChaseMinigameStarter.Instance.StartChaseMinigame(m_QTEInteractables, m_ChaseSpawn, m_ReturnSpawn);
				OnChaseStarted?.Invoke();
			}
			m_HasBeenPlayed = true;
		}
		return new InteractionStatus() { EndInteraction = true };
	}
}
