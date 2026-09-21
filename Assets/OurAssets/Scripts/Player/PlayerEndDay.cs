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
    [Header("Waypoint")]
    [SerializeField]
    Sprite m_EndDayWaypointIcon;

    [Header("Tutorial")]
    [SerializeField]
    LoadingScreen m_TutorialLoadingScreen;
    [SerializeField, Min(0)]
    int m_TutorialMainLevelSceneIndex;

    bool m_bMarkerShown;
    readonly DisplayTask m_EndDayDisplayTask = new DisplayTask("Head to bed for the night", 1, 0, false);

    void Update()
    {
        bool bShouldShow = NPCHouseDailyManager.Instance && NPCHouseDailyManager.Instance.AllMinigamesBeatenForToday();
        if (bShouldShow == m_bMarkerShown) return;
        m_bMarkerShown = bShouldShow;
        if (bShouldShow)
        {
            WaypointManager.Instance?.AddWaypoint(transform, m_EndDayWaypointIcon);
            TaskList.Instance?.AddTask(m_EndDayDisplayTask);
        }
        else
        {
            WaypointManager.Instance?.RemoveWaypoint(transform);
            TaskList.Instance?.RemoveTask(m_EndDayDisplayTask);
        }
    }

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
        else if (TutorialMinigameManager.Instance) FinishTutorial();
        else GameManager.Instance.EndDay();
        return new InteractionStatus() { EndInteraction = true };
    }

    void FinishTutorial()
    {
        if (!TutorialMinigameManager.Instance.AllMinigamesBeaten)
        {
#if UNITY_EDITOR
            Debug.Log("Can't finish the tutorial until every minigame has been beaten");
#endif
            return;
        }
        ++PlayerSaveManager.CurrentSaveData.DayNumber;
        PlayerSaveManager.SaveGame();
        m_TutorialLoadingScreen.SceneIndexToLoad = m_TutorialMainLevelSceneIndex;
        m_TutorialLoadingScreen.gameObject.SetActive(true);
    }
}
