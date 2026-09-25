using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class MusicTrackDisplayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    TMP_Text m_Text;
    [SerializeField, TextArea]
    string m_DisplayFormat = "Current Song:\n<i>{0}</i>\nBy {1}";
    [SerializeField]
    float m_DisplayDuration = 10f;

    RectTransform m_RectTransform;
    CanvasGroup m_CanvasGroup;

    public MusicTrack CurrentMusicTrack { get; set; }

    void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_CanvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        m_CanvasGroup.alpha = 1f;
        m_Text.text = string.Format(m_DisplayFormat, CurrentMusicTrack.SongName, CurrentMusicTrack.ArtistName);
        m_Text.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_RectTransform);
        StartCoroutine(Disappear());
    }

    IEnumerator Disappear()
    {
        yield return new WaitForSecondsRealtime(m_DisplayDuration);
        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) => m_CanvasGroup.alpha = 0f;

    public void OnPointerExit(PointerEventData eventData) => m_CanvasGroup.alpha = 1f;
}
