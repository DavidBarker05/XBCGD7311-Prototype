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
	[SerializeField]
	EnemySpawner m_EnemySpawner;

	[Header("Money Reward")]
	[SerializeField, Min(0f)]
	float m_BaseMoneyReward = 60f;
	[SerializeField, Min(0f)]
	float m_FastCompletionTime = 60f;
	[SerializeField, Min(0f)]
	float m_SlowCompletionTime = 150f;

	public bool ChaseMinigameIsRunning { get; private set; }

	QTEInteractable[] m_QTEInteractables;
	Transform m_ChaseSpawn;
	Transform m_HouseSpawn;
	int m_NumInteractables;
	int m_NumInteractablesBeaten;
	float m_StartTime;

	void Awake()
	{
		if (Instance && Instance != this) Destroy(gameObject);
		else Instance = this;
	}

	public void StartChaseMinigame(QTEInteractable[] qteInteractables, Transform chaseSpawn, Transform houseSpawn)
	{
		bool bIsNewChase = qteInteractables != m_QTEInteractables;
		m_EnemySpawner?.ResetSpawner();
		ChaseMinigameIsRunning = true;
		m_ChaseSpawn = chaseSpawn;
		m_HouseSpawn = houseSpawn;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = false;
		m_FPPCharacter.gameObject.transform.position = m_ChaseSpawn.position;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = true;
		if (bIsNewChase)
		{
			m_StartTime = Time.time;
			MusicManager.Instance?.StartChaseMusic();
		}
		if (m_CheckpointManager && bIsNewChase)
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
		AwardMoney();
		MinigameManager.Instance?.OnMinigameBeaten();
		HouseProgressTracker.ReportMinigameCompleted(MinigameType.ChaseMinigame);
		TutorialMinigameManager.Instance?.ReportMinigameCompleted(MinigameType.ChaseMinigame);
		MusicManager.Instance?.EndChaseMusic();
		ChaseMinigameIsRunning = false;
		m_CheckpointManager?.ClearDynamicCheckpoints();
		m_CheckpointManager?.ClearChaseTask();
		foreach (QTEInteractable qte in m_QTEInteractables) if (qte) Destroy(qte.gameObject);
		m_QTEInteractables = null;
	}

	void AwardMoney()
	{
		if (TutorialMinigameManager.Instance) return;
		float elapsed = Time.time - m_StartTime;
		float timeT = Mathf.InverseLerp(m_FastCompletionTime, m_SlowCompletionTime, elapsed);
		float timeMultiplier = Mathf.Lerp(1.2f, 0.8f, timeT);
		MinigameMoneyReward.Award(m_BaseMoneyReward, timeMultiplier);
	}
}
