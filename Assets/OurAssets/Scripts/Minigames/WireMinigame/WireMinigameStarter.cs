using UnityEngine;

public class WireMinigameStarter : MonoBehaviour
{
	public static WireMinigameStarter Instance { get; private set; }

	[SerializeField]
	WireBoard m_WireBoard;
	[SerializeField]
	Player m_Player;

	void Awake()
	{
		if (Instance && Instance != this) Destroy(gameObject);
		else Instance = this;
	}

	public void StartWireMinigame()
	{
		m_WireBoard.StartWireMinigame();
		m_Player.ChangeActionMap("WirePlayer");
	}
}
