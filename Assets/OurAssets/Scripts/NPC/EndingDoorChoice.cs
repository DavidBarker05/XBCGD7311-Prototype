using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EndingDoorChoice : MonoBehaviour
{
    [SerializeField]
    GameObject m_WalkOutEndingScreen;
    [SerializeField]
    MenuCharacter m_MenuCharacter;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;
    [SerializeField]
    GameObject m_HUD;

    void OnTriggerEnter(Collider other)
    {
        if (!EndingNPC.Instance || !EndingNPC.Instance.IsChoiceActive) return;
        if (!other.GetComponentInParent<FirstPersonPlayerCharacter>()) return;
        EndingNPC.Instance.NotifyEndingChosen();
        m_MenuCharacter.OnMenuOpen(m_FirstPersonPlayerCharacter, m_HUD, m_WalkOutEndingScreen);
    }
}
