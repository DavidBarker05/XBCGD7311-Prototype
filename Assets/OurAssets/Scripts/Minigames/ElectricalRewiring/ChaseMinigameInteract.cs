using UnityEngine;

public class ChaseMinigameInteract : Interactable
{
	[SerializeField]
	bool m_CanBePlayedAgain = false;
	[SerializeField]
	QTEInteractable[] m_QTEInteractables;

	bool m_HasBeenPlayed = false;

	public override object[] Interact(params object[] inputParameters)
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
			}
			m_HasBeenPlayed = true;
		}
		return null;
	}
}
