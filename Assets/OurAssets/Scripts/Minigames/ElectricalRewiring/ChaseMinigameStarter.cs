using UnityEngine;

public class ChaseMinigameStarter : MonoBehaviour
{
	public static ChaseMinigameStarter Instance { get; private set; }

	[SerializeField]
	Player m_Player;
	[SerializeField]
	FirstPersonPlayerCharacter m_FPPCharacter;
	[SerializeField]
	Transform m_ChaseSpawn;
	[SerializeField]
	Transform m_HouseSpawn;

	public bool ChaseMinigameIsRunning { get; private set; }

	QTEInteractable[] m_QTEInteractables;
	int m_NumInteractables;
	int m_NumInteractablesBeaten;

	void Awake()
	{
		if (Instance && Instance != this) Destroy(gameObject);
		else Instance = this;
	}

	public void StartChaseMinigame(QTEInteractable[] qteInteractables)
	{
		ChaseMinigameIsRunning = true;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = false;
		m_FPPCharacter.gameObject.transform.position = m_ChaseSpawn.position;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = true;
		m_QTEInteractables = qteInteractables;
		m_NumInteractables = m_QTEInteractables.Length;
		foreach (QTEInteractable qte in m_QTEInteractables) qte.gameObject.SetActive(true);
		m_NumInteractablesBeaten = 0;
		ChasePlayer[] enemies = FindObjectsByType<ChasePlayer>();
		foreach (ChasePlayer enemy in enemies)
		{
			enemy.ResetToStart();
		}
	}

	public void RestartChaseMinigame() => StartChaseMinigame(m_QTEInteractables);

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
		ChaseMinigameIsRunning = false;
	}
}
