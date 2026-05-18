using UnityEngine;

public class WallKnockMinigameStarter : MonoBehaviour
{
	public static WallKnockMinigameStarter Instance { get; private set; }

	[SerializeField]
	Wall m_Wall;
	[SerializeField]
	Player m_Player;
	[SerializeField]
	WallKnockPlayerCharacter m_WallKnockPlayerCharacter;

	void Awake()
	{
		if (Instance && Instance != this) Destroy(gameObject);
		else Instance = this;
	}

	public void StartWallKnockMinigame()
	{
		m_Wall.StartWallKnockMinigame();
		m_Player.ChangeCharacter(m_WallKnockPlayerCharacter);
	}
}
