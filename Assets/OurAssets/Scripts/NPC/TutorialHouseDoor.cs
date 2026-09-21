using UnityEngine;

public class TutorialHouseDoor : Interactable
{
    public DoorType DoorType { get; private set; } = DoorType.Entry;

    [SerializeField]
    TutorialHouse m_TutorialHouse;
    [SerializeField]
    Transform m_InsideTeleportSpot;
    [SerializeField]
    Transform m_OutsideTeleportSpot;

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: TutorialHouseDoor objects needs 1 input parameter. Received {inputParameters.Length} input parameters");
#endif
        }
        else if (inputParameters[0] is FirstPersonPlayerCharacter player)
        {
            if (DoorType == DoorType.Entry)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                cc.enabled = false;
                player.transform.SetPositionAndRotation(m_InsideTeleportSpot.position, m_InsideTeleportSpot.rotation);
                cc.enabled = true;
                m_TutorialHouse.OnPlayerEnteredHouse();
                DoorType = DoorType.Exit;
            }
            else if (m_TutorialHouse.CurrentStage != TutorialHouse.Stage.LeaveHousePending)
            {
#if UNITY_EDITOR
                Debug.Log("Door is locked until it's time to leave");
#endif
                return new InteractionStatus() { EndInteraction = true };
            }
            else
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                cc.enabled = false;
                player.transform.SetPositionAndRotation(m_OutsideTeleportSpot.position, m_OutsideTeleportSpot.rotation);
                cc.enabled = true;
                m_TutorialHouse.OnLeaveHouseInteracted();
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: TutorialHouseDoor expects FirstPersonPlayerCharacter as inputParameters[0]. Received type {inputParameters[0].GetType()}");
#endif
        }
        return new InteractionStatus() { EndInteraction = true };
    }
}
