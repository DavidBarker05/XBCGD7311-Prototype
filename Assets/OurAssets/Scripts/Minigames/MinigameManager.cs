using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    public int MinigamesBeaten { get; private set; } = 0;

    [SerializeField, Min(1)]
    int m_NumMinigamesToBeat = 3;
    [SerializeField]
    MenuCharacter m_MenuCharacter;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;
    [SerializeField]
    GameObject m_HUD;
    [SerializeField]
    GameObject m_WinScreen;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void OnMinigameBeaten()
    {
        if (++MinigamesBeaten >= m_NumMinigamesToBeat) m_MenuCharacter.OnMenuOpen(m_FirstPersonPlayerCharacter, m_HUD, m_WinScreen); // Hopefully this works
    }
}
