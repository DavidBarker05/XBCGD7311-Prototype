using UnityEngine;

public class PlayerFinishTutorial : Interactable
{
    [SerializeField]
    LoadingScreen m_LoadingScreen;
    [SerializeField, Min(0)]
    int m_MainLevelSceneIndex;

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: PlayerFinishTutorial objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
        }
        else if (!TutorialMinigameManager.Instance.AllMinigamesBeaten)
        {
#if UNITY_EDITOR
            Debug.Log("Can't finish the tutorial until every minigame has been beaten");
#endif
        }
        else
        {
            ++PlayerSaveManager.CurrentSaveData.DayNumber;
            PlayerSaveManager.SaveGame();
            m_LoadingScreen.SceneIndexToLoad = m_MainLevelSceneIndex;
            m_LoadingScreen.gameObject.SetActive(true);
        }
        return new InteractionStatus() { EndInteraction = true };
    }
}
