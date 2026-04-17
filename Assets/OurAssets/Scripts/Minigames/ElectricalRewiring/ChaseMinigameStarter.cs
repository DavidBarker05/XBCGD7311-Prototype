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

	int numInteractables;
	int numInteractablesBeaten;

	void Awake()
	{
		if (Instance && Instance != this) Destroy(gameObject);
		else Instance = this;
	}

	public void StartChaseMinigame()
	{
		ChaseMinigameIsRunning = true;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = false;
		m_FPPCharacter.gameObject.transform.position = m_ChaseSpawn.position;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = true;
		numInteractables = 0;
		numInteractablesBeaten = 0;
		QTEInteractable[] interactables = FindObjectsByType<QTEInteractable>();
		foreach (QTEInteractable interactable in interactables)
		{
			interactable.gameObject.SetActive(true);
			++numInteractables;
		}
		ChasePlayer[] enemies = FindObjectsByType<ChasePlayer>();
		foreach(ChasePlayer enemy in enemies)
		{
			enemy.ResetToStart();
		}
	}

	public void InteractableBeaten()
	{
		++numInteractablesBeaten;
		if (numInteractablesBeaten == numInteractables) EndChaseMinigame();
	}

	public void EndChaseMinigame()
	{
		m_FPPCharacter.GetComponent<CharacterController>().enabled = false;
		m_FPPCharacter.transform.position = m_HouseSpawn.position;
		m_FPPCharacter.GetComponent<CharacterController>().enabled = true;
		m_Player.OnMinigameBeaten();
		ChaseMinigameIsRunning = false;
	}
}
