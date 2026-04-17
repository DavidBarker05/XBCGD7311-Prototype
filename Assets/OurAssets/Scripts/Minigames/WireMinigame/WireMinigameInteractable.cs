using UnityEngine;

public class WireMinigameInteractable : Interactable
{
	[SerializeField]
	bool m_CanBePlayedAgain = false;

	public override object[] Interact(params object[] inputParameters)
	{
		if (inputParameters.Length != 0)
		{
#if UNITY_EDITOR
			Debug.LogWarning($"WARNING: WireMinigameInteractable objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
		}
		else
		{
			WallKnockMinigameStarter.Instance.StartWallKnockMinigame();
			if (!m_CanBePlayedAgain) Destroy(gameObject);
		}
		return null;
	}
}
