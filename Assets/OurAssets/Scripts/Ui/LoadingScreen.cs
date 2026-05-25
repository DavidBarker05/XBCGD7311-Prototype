using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    Sprite[] m_LoadingScreenImages;
    [field: SerializeField, Min(0)]
    public int SceneIndexToLoad { get; set; } = 0;
    [SerializeField]
    Slider m_LoadingBar;

    void OnEnable()
    {
        Sprite sprite = null;
        if (m_LoadingScreenImages.Length > 0) sprite = m_LoadingScreenImages[Random.Range(0, m_LoadingScreenImages.Length)];
        GetComponent<Image>().sprite = sprite;
        StartCoroutine(LoadAsync());
    }

    IEnumerator LoadAsync()
    {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(SceneIndexToLoad);
        while (!loadScene.isDone)
        {
            float loadProgress = Mathf.Clamp01(loadScene.progress);
            m_LoadingBar.value = loadProgress;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}