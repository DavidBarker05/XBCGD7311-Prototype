using UnityEngine;

public class PlayerEndDay : Interactable
{
    [Header("Day 10 ending")]
    [SerializeField]
    GameObject m_SleepEndingScreen;
    [SerializeField]
    MenuCharacter m_MenuCharacter;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;
    [SerializeField]
    GameObject m_HUD;

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: PlayerEndDay objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
        }
        else if (EndingNPC.Instance && EndingNPC.Instance.IsChoiceActive)
        {
            EndingNPC.Instance.NotifyEndingChosen();
            m_MenuCharacter.OnMenuOpen(m_FirstPersonPlayerCharacter, m_HUD, m_SleepEndingScreen);
        }
        else GameManager.Instance.EndDay();
        return new InteractionStatus() { EndInteraction = true };
    }
}
