using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ContinueButton : MonoBehaviour
{
    [SerializeField]
    LoadingScreen m_LoadingScreen;
    [SerializeField, Min(0)]
    int m_TutorialSceneIndex = 1;
    [SerializeField, Min(0)]
    int m_MainGameSceneIndex = 2;

    void Awake()
    {
        Button button = GetComponent<Button>();
        if (PlayerSaveManager.CurrentSaveData != null)
        {
            button.onClick.AddListener(() =>
            {
                m_LoadingScreen.SceneIndexToLoad = PlayerSaveManager.CurrentSaveData.DayNumber == 0 ? m_TutorialSceneIndex : m_MainGameSceneIndex;
                m_LoadingScreen.gameObject.SetActive(true);
            });
        }
        else button.interactable = false;
    }
}
