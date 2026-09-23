using UnityEngine;

public enum DoorType
{
    Entry,
    Exit
}

public class Door : Interactable
{
    public DoorType DoorType { get; set; } = DoorType.Entry;
    public NPCHouse OwningHouse { get; set; }

    public bool CanInteract { get; set; } = true;

    public void ResetForNewDay()
    {
        DoorType = DoorType.Entry;
        CanInteract = true;
    }

    public override bool CanInteractWith => CanInteract && OwningHouse && OwningHouse.Progress != null
        && (DoorType == DoorType.Entry ? !OwningHouse.Progress.HasBeatenHouse : OwningHouse.Progress.AllMinigamesBeaten);

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: Door objects needs 1 input parameter. Received {inputParameters.Length} input parameters");
#endif
        }
        else if (inputParameters[0] is FirstPersonPlayerCharacter player)
        {
            if (!CanInteract) return new InteractionStatus() { EndInteraction = true };
            if (DoorType == DoorType.Entry && !OwningHouse.Progress.HasBeatenHouse)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                cc.enabled = false;
                player.transform.SetPositionAndRotation(OwningHouse.HouseTeleportSpot.position, OwningHouse.HouseTeleportSpot.rotation);
                cc.enabled = true;
                OwningHouse.EnterHouse();
            }
            else if (DoorType == DoorType.Exit)
            {
                if (!OwningHouse.Progress.AllMinigamesBeaten)
                {
#if UNITY_EDITOR
                    Debug.Log("Door is locked until all of this house's minigames have been beaten");
#endif
                    return new InteractionStatus() { EndInteraction = true };
                }
                CharacterController cc = player.GetComponent<CharacterController>();
                cc.enabled = false;
                player.transform.SetPositionAndRotation(OwningHouse.OutsideTeleportSpot.position, OwningHouse.OutsideTeleportSpot.rotation);
                cc.enabled = true;
                OwningHouse.Progress.HasBeatenHouse = true;
                OwningHouse.ExitHouse();
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: Door expects FirstPersonPlayerCharacter as inputParameters[0]. Received type {inputParameters[0].GetType()}");
#endif
        }
        return new InteractionStatus() { EndInteraction = true };
    }
}
