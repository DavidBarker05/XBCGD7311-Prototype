using UnityEngine;
using UnityEngine.Events;
using Util.ArrayUtils;

public class ChaseMinigameInteract : Interactable, IHouseTaskInteractable
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

	public NPCHouse OwningHouse { get; set; }
	public int TaskSlotIndex { get; set; } = -1;

	public bool CanInteract { get; set; } = true;

	public UnityEvent OnChaseStarted;

	bool m_HasBeenPlayed = false;

	public void ResetForNewDay()
	{
		m_HasBeenPlayed = false;
		CanInteract = true;
	}

	bool CanStartChase => CanInteract && (OwningHouse == null || OwningHouse.Progress.HasTalkedToNPC)
		&& m_QTEInteractableSpawns != null && m_QTEInteractableSpawns.Length > 0
		&& (!m_HasBeenPlayed || m_CanBePlayedAgain) && !ChaseMinigameStarter.Instance.ChaseMinigameIsRunning;

	public override bool CanInteractWith => CanStartChase;

	public override InteractionStatus Interact(params object[] inputParameters)
	{
		if (inputParameters.Length != 0)
		{
#if UNITY_EDITOR
			Debug.LogWarning($"WARNING: ChaseMinigameInteract objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
		}
		else if (CanStartChase)
		{
#if UNITY_EDITOR
			if (!m_ChaseSpawn || !m_ReturnSpawn) Debug.LogWarning($"WARNING: {name} is missing its chase spawn and/or return spawn transform");
#endif
			TaskSlotIndex = OwningHouse ? OwningHouse.NextUnbeatenSlot(MinigameType.ChaseMinigame) : -1;
			HouseProgressTracker.SetActiveTaskSlot(TaskSlotIndex);
			OwningHouse?.OnChaseTaskStarted(TaskSlotIndex);
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
			m_HasBeenPlayed = true;
		}
		return new InteractionStatus() { EndInteraction = true };
	}
}
