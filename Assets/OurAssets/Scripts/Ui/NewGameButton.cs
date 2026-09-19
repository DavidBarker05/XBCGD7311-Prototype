using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NewGameButton : MonoBehaviour
{
    [SerializeField]
    LoadingScreen m_LoadingScreen;
    [SerializeField, Min(0)]
    int m_TutorialSceneIndex = 1;
    [SerializeField]
    GameObject m_ConfirmationScreen;

    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
    {
        if (PlayerSaveManager.CurrentSaveData != null) m_ConfirmationScreen.SetActive(true);
        else
        {
            m_LoadingScreen.SceneIndexToLoad = m_TutorialSceneIndex;
            m_LoadingScreen.gameObject.SetActive(true);
        }
    });
}
