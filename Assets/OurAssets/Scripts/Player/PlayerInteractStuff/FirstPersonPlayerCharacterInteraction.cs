using UnityEngine;
using Util.SystemUtils;
using Util.UnityUtils;

public class FirstPersonPlayerCharacterInteractionInitData : IPlayerInteractionInitData
{
    public Camera Camera { get; set; }

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

    Player m_Player;
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;

    public override void Init(IPlayerInteractionInitData playerInteractionInitData)
    {
        FirstPersonPlayerCharacterInteractionInitData initData = Sys.AssertType<FirstPersonPlayerCharacterInteractionInitData>(playerInteractionInitData, nameof(playerInteractionInitData));
        m_Camera = initData.Camera;
        m_Player = initData.Player;
        m_FirstPersonPlayerCharacter = initData.FirstPersonPlayerCharacter;
        HasBeenInitialised = true;
    }

    public override void UpdateInteraction(ref IPlayerInteractionUpdateData playerInteractionUpdateData)
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacterInteraction hasn't been initialised");
        Sys.AssertType<FirstPersonPlayerCharacterInteractionUpdateData>(playerInteractionUpdateData, nameof(playerInteractionUpdateData));
        Interactable lookedAtInteraction = CheckForInteraction();
        if (lookedAtInteraction && lookedAtInteraction.CanInteractWith && lookedAtInteraction.InteractPromptTransform)
        {
            string keyLabel = GameUserSettingsManager.Instance?.GetBindingDisplayString("Player", "Interact") ?? "E";
            string interactText;
            if (lookedAtInteraction is TVInteractable && (!PlayerSaveManager.CurrentSaveData?.HasTVLicence ?? true)) interactText = $"Press {keyLabel} to purchase TV Licence";
            else interactText = $"Press {keyLabel} to interact";
            InteractPromptManager.Instance?.Show(lookedAtInteraction.InteractPromptTransform, interactText);
        }
        else InteractPromptManager.Instance?.Hide();
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
            else if (targetInteraction is Door or NPC or PlayerHouseDoor or TutorialHouseDoor) targetInteraction.Interact(m_FirstPersonPlayerCharacter);
            else
            {
                m_CurrentInteraction = targetInteraction;
                if (m_CurrentInteraction.Interact().EndInteraction) m_CurrentInteraction = null;
            }
        }
    }
}
