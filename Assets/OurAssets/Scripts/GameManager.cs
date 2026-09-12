using UnityEngine;

// The point of this script is to ensure that the correct generation order happens when
// loading into a level
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    Player m_Player;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;
    [SerializeField]
    PlayerCamera m_PlayerCamera;
    [SerializeField]
    MenuCharacter m_MenuCharacter;
    [SerializeField]
    GameObject m_HUD;
    [SerializeField]
    GameObject m_EndOfDayScreen;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Update()
    {
        if (m_Player.CurrentPlayerCharacter != m_FirstPersonPlayerCharacter) return;
        HouseProgressTracker.UpdateActiveHousePlayerTransform(m_FirstPersonPlayerCharacter.transform.position, m_FirstPersonPlayerCharacter.transform.rotation, m_PlayerCamera.transform.rotation);
    }

    void Start() => LoadDay(); // Load the day when loading into the level

    void LoadDay()
    {
        PlayerSaveManager.UseSeedForCurrentDay();
        NPCHouse houseWithPlayer = NPCHouseDailyManager.Instance.GenerateHousesForDay();
        if (houseWithPlayer) RestorePlayerInsideHouse(houseWithPlayer);
    }

    // Puts the player back where they were, inside the house they were still in when the scene reloaded
    void RestorePlayerInsideHouse(NPCHouse house)
    {
        m_Player.ChangeCharacter(m_FirstPersonPlayerCharacter);
        CharacterController cc = m_FirstPersonPlayerCharacter.GetComponent<CharacterController>();
        cc.enabled = false;
        m_FirstPersonPlayerCharacter.transform.SetPositionAndRotation(house.Progress.PlayerPosition, house.Progress.PlayerRotation);
        cc.enabled = true;
        m_PlayerCamera.SetRotation(house.Progress.CameraRotation);
    }

    // Load day and exit end of day menu
    public void StartDay()
    {
        LoadDay();
        m_MenuCharacter.OnMenuExit();
    }

    public void EndDay()
    {
        ++PlayerSaveManager.CurrentSaveData.DayNumber;
        if (PlayerSaveManager.CurrentSaveData.DayNumber > 1) PlayerSaveManager.GenerateRandomSeed();
        PlayerSaveManager.SaveGame();
        NPCHouseDailyManager.Instance.ClearHouses();
        m_MenuCharacter.OnMenuOpen(m_FirstPersonPlayerCharacter, m_HUD, m_EndOfDayScreen);
    }
}
