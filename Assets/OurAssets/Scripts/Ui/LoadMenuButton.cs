using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadMenuButton : MonoBehaviour
{
    [SerializeField]
    int m_MainMenuSceneIndex = 0;
    [SerializeField]
    LoadingScreen m_LoadingScreen;

    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
    {
        Time.timeScale = 1f;
        // Don't let a person just load a tutorial then go back to main menu to get a set seed
        if (PlayerSaveManager.CurrentSaveData.DayNumber == 0)
            Random.InitState(PlayerSaveManager.CurrentSaveData.DaySeed);
        m_LoadingScreen.SceneIndexToLoad = m_MainMenuSceneIndex;
        m_LoadingScreen.gameObject.SetActive(true);
    });
}
