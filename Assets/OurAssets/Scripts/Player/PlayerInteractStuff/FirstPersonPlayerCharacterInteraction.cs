using UnityEngine;
using Util.SystemUtils;
using Util.UnityUtils;

public class FirstPersonPlayerCharacterInteractionInitData : IPlayerInteractionInitData
{
    public Camera Camera { get; set; }
    public Camera HoldCamera { get; set; }
    public Camera HoldClipCamera { get; set; }
    public Transform HoldPosTransform { get; set; }

    public Player Player { get; set; }
    public FirstPersonPlayerCharacter FirstPersonPlayerCharacter { get; set; }
}

public class FirstPersonPlayerCharacterInteractionUpdateData : IPlayerInteractionUpdateData
{
    public float DeltaTime { get; set; }
}

public class FirstPersonPlayerCharacterInteraction : PlayerInteraction
{
    public override bool HasBeenInitialised { get; protected set; }

    Camera m_Camera;
    Camera m_HoldCamera;
    Camera m_HoldClipCamera;

    Transform m_HoldPosTransform;

    Player m_Player;
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;

    public override void Init(IPlayerInteractionInitData playerInteractionInitData)
    {
        FirstPersonPlayerCharacterInteractionInitData initData = Sys.AssertType<FirstPersonPlayerCharacterInteractionInitData>(playerInteractionInitData, nameof(playerInteractionInitData));
        m_Camera = initData.Camera;
        m_HoldCamera = initData.HoldCamera;
        m_HoldClipCamera = initData.HoldClipCamera;
        m_HoldPosTransform = initData.HoldPosTransform;
        m_Player = initData.Player;
        m_FirstPersonPlayerCharacter = initData.FirstPersonPlayerCharacter;
        HasBeenInitialised = true;
    }

    public override void UpdateInteraction(ref IPlayerInteractionUpdateData playerInteractionUpdateData)
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacterInteraction hasn't been initialised");
        Sys.AssertType<FirstPersonPlayerCharacterInteractionUpdateData>(playerInteractionUpdateData, nameof(playerInteractionUpdateData));
        if (m_CurrentInteraction == null) return;
        if (m_CurrentInteraction is Holdable heldObject)
        {
            heldObject.LookAtPlayer(m_FirstPersonPlayerCharacter.transform.position);
            int bitMask = ~(m_InteractSettings.HoldLayer | m_FirstPersonPlayerCharacter.gameObject.layer);
            if (Physics.Linecast(m_Camera.transform.position, heldObject.transform.position, bitMask) || Physics.CheckBox(heldObject.transform.position, heldObject.GetComponent<Collider>().bounds.extents, heldObject.transform.rotation, bitMask))
            {
                m_HoldCamera.gameObject.SetActive(true);
                m_HoldClipCamera.gameObject.SetActive(true);
            }
            else
            {
                m_HoldCamera.gameObject.SetActive(false);
                m_HoldClipCamera.gameObject.SetActive(false);
            }
        }
    }

    protected override Interactable CheckForInteraction()
    {
        Vector3 direction = m_Camera.transform.rotation * Vector3.forward; // Rotate forward vector by camera rotation to get camera's forward vector
        if (Physics.Raycast(
            origin: m_Camera.transform.position,
            direction: direction,
            hitInfo: out RaycastHit hit,
            maxDistance: m_InteractSettings.InteractionDistance,
            layerMask: m_InteractSettings.InteractableLayer,
            queryTriggerInteraction: QueryTriggerInteraction.Collide))
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            return interactable;
        }
        return null;
    }

    public override void Interact()
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacterInteraction hasn't been initialised");
        Interactable targetInteraction = CheckForInteraction();
        if (targetInteraction != null)
        {
            if (targetInteraction is QTEInteractable) targetInteraction.Interact(m_Player, m_FirstPersonPlayerCharacter);
            else if (targetInteraction is WireMinigameInteractable or WallKnockInteractable or ChaseMinigameInteract) targetInteraction.Interact();
            else if (targetInteraction is Door or NPC) targetInteraction.Interact(m_FirstPersonPlayerCharacter);
            else if (m_CurrentInteraction != null)
            {
                if (m_CurrentInteraction is Holdable) InteractHoldable();
            }
            else
            {
                m_CurrentInteraction = targetInteraction;
                if (m_CurrentInteraction is Holdable) InteractHoldable();
                else if (m_CurrentInteraction.Interact().EndInteraction) m_CurrentInteraction = null;
            }
        }
        else if (m_CurrentInteraction != null)
        {
            if (m_CurrentInteraction is Holdable) InteractHoldable();
        }
    }

    void InteractHoldable()
    {
        if (m_CurrentInteraction is Holdable holdable)
        {
            bool drop = holdable.Interact(m_HoldPosTransform, m_InteractSettings.HoldLayer, m_FirstPersonPlayerCharacter.GetComponent<CharacterController>(), m_Camera.transform).EndInteraction;
            if (drop) m_CurrentInteraction = null;
        }
    }
}
