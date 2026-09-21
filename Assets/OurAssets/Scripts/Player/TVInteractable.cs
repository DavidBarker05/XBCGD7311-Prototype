using UnityEngine;
using UnityEngine.Events;

public class TVInteractable : Interactable
{
    [SerializeField]
    GameObject m_UpgradeScreen;
    [SerializeField]
    MenuCharacter m_MenuCharacter;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;
    [SerializeField]
    GameObject m_HUD;

    public UnityEvent OnScreenClosed;

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: TutorialTVInteractable objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
        }
        else m_MenuCharacter.OnMenuOpen(m_FirstPersonPlayerCharacter, m_HUD, m_UpgradeScreen);
        return new InteractionStatus() { EndInteraction = true };
    }

    public void NotifyScreenClosed() => OnScreenClosed?.Invoke();
}
