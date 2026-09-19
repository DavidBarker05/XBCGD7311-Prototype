using UnityEngine;

public class ChaseMinigameStarter : MonoBehaviour
{
	public static ChaseMinigameStarter Instance { get; private set; }

	[SerializeField]
	Player m_Player;
	[SerializeField]
	FirstPersonPlayerCharacter m_FPPCharacter;
	[SerializeField]
	QTECheckpointManager m_CheckpointManager;

	public bool ChaseMinigameIsRunning { get; private set; }

	QTEInteractable[] m_QTEInteractables;
	Transform m_ChaseSpawn;
	Transform m_HouseSpawn;
	int m_NumInteractables;
	int m_NumInteractablesBeaten;

	void Awake()
	{
		if (Instance && Instance != this) Destroy(gameObject);
		else Instance = this;
	}

	public void StartChaseMinigame(QTEInteractable[] qteInteractables, Transform chaseSpawn, Transform houseSpawn)
	{
		ChaseMinigameIsRunning = true;
		m_ChaseSpawn = chaseSpawn;
		m_HouseSpawn = houseSpawn;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = false;
		m_FPPCharacter.gameObject.transform.position = m_ChaseSpawn.position;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = true;

		if (m_CheckpointManager && qteInteractables != m_QTEInteractables)
		{
			m_CheckpointManager.ClearDynamicCheckpoints();
			foreach (QTEInteractable qte in qteInteractables)
			{
				if (!m_CheckpointManager.checkpoints.Contains(qte)) m_CheckpointManager.RegisterDynamicCheckpoint(qte);
			}
		}

		m_QTEInteractables = qteInteractables;
		m_NumInteractables = m_QTEInteractables.Length;
		foreach (QTEInteractable qte in m_QTEInteractables) qte.gameObject.SetActive(true);
		m_NumInteractablesBeaten = 0;
		m_CheckpointManager?.BeginCheckpoints();
	}

	public void RestartChaseMinigame() => StartChaseMinigame(m_QTEInteractables, m_ChaseSpawn, m_HouseSpawn);

	public void InteractableBeaten()
	{
		++m_NumInteractablesBeaten;
		if (m_NumInteractablesBeaten == m_NumInteractables) EndChaseMinigame();
	}

	public void EndChaseMinigame()
	{
		m_FPPCharacter.GetComponent<CharacterController>().enabled = false;
		m_FPPCharacter.transform.position = m_HouseSpawn.position;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = true;
		MinigameManager.Instance?.OnMinigameBeaten();
		HouseProgressTracker.ReportMinigameCompleted(MinigameType.ChaseMinigame);
		TutorialMinigameManager.Instance?.ReportMinigameCompleted(MinigameType.ChaseMinigame);
		ChaseMinigameIsRunning = false;
		m_CheckpointManager?.ClearDynamicCheckpoints();
		foreach (QTEInteractable qte in m_QTEInteractables) if (qte) Destroy(qte.gameObject);
		m_QTEInteractables = null;
	}
}
