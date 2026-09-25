using UnityEngine;

public class WallKnockInteractable : Interactable, IHouseTaskInteractable
{
	[SerializeField]
	bool m_CanBePlayedAgain = false;

	bool m_HasBeenPlayed = false;

	public NPCHouse OwningHouse { get; set; }
	public int TaskSlotIndex { get; set; }

	public override bool CanInteractWith => (OwningHouse == null || OwningHouse.Progress.HasTalkedToNPC) && (!m_HasBeenPlayed || m_CanBePlayedAgain);

	public override InteractionStatus Interact(params object[] inputParameters)
	{
		if (inputParameters.Length != 0)
		{
#if UNITY_EDITOR
			Debug.LogWarning($"WARNING: WallKnockInteractable objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
		}
		else if (OwningHouse == null || OwningHouse.Progress.HasTalkedToNPC)
		{
			if (!m_HasBeenPlayed || m_CanBePlayedAgain)
			{
				OwningHouse?.HideTaskMarker(TaskSlotIndex);
				HouseProgressTracker.SetActiveTaskSlot(TaskSlotIndex);
				WallKnockMinigameStarter.Instance.StartWallKnockMinigame();
			}
			m_HasBeenPlayed = true;
		}
		return new InteractionStatus() { EndInteraction = true };
	}
}
