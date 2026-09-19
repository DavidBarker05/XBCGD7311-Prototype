using UnityEngine;
using UnityEngine.Events;
using Util.ArrayUtils;

public class ChaseMinigameInteract : Interactable
{
	[SerializeField]
	bool m_CanBePlayedAgain = false;
	[SerializeField]
	QTEInteractable m_QTEInteractablePrefab;
	[SerializeField]
	Transform[] m_QTEInteractableSpawns;
	[SerializeField, Min(1)]
	int m_NumQTEInteractablesToSpawn;

	[Header("Per-house teleport spots")]
	[SerializeField]
	Transform m_ChaseSpawn;
	[SerializeField]
	Transform m_ReturnSpawn;

	public Transform ChaseSpawn { get => m_ChaseSpawn; set => m_ChaseSpawn = value; }
	public Transform ReturnSpawn { get => m_ReturnSpawn; set => m_ReturnSpawn = value; }

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
		else if (m_QTEInteractableSpawns != null && m_QTEInteractableSpawns.Length > 0)
		{
			if ((!m_HasBeenPlayed || m_CanBePlayedAgain) && !ChaseMinigameStarter.Instance.ChaseMinigameIsRunning)
			{
#if UNITY_EDITOR
				if (!m_ChaseSpawn || !m_ReturnSpawn) Debug.LogWarning($"WARNING: {name} is missing its chase spawn and/or return spawn transform");
#endif
				int numInteractablesToSpawn = Mathf.Clamp(m_NumQTEInteractablesToSpawn, 1, m_QTEInteractableSpawns.Length);
				QTEInteractable[] qteInteractables = new QTEInteractable[numInteractablesToSpawn];
				Transform[] shuffledSpawns = new Transform[m_QTEInteractableSpawns.Length];
				m_QTEInteractableSpawns.CopyTo(shuffledSpawns, 0);
				shuffledSpawns.Shuffle();
				for (int i = 0; i < numInteractablesToSpawn; ++i)
				{
					qteInteractables[i] = Instantiate(m_QTEInteractablePrefab, shuffledSpawns[i].position, shuffledSpawns[i].rotation);
				}
				ChaseMinigameStarter.Instance.StartChaseMinigame(qteInteractables, m_ChaseSpawn, m_ReturnSpawn);
				OnChaseStarted?.Invoke();
			}
			m_HasBeenPlayed = true;
		}
		return new InteractionStatus() { EndInteraction = true };
	}
}
