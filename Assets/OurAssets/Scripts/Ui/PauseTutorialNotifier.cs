using UnityEngine;

public class PauseTutorialNotifier : MonoBehaviour
{
    public static PauseTutorialNotifier Instance { get; private set; }

    [SerializeField]
    MenuCharacter m_MenuCharacter;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;
    [SerializeField]
    GameObject m_PauseTutorialNotifyScreen;
    [SerializeField]
    GameObject m_HUD;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (Instance != this) return;
        if (PlayerSaveManager.CurrentSaveData == null || PlayerSaveManager.CurrentSaveData.DayNumber != 1) return;
        m_MenuCharacter.OnMenuOpen(m_FirstPersonPlayerCharacter, m_HUD, m_PauseTutorialNotifyScreen);
    }
}
