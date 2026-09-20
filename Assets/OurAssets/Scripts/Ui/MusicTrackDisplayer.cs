using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class MusicTrackDisplayer : MonoBehaviour
{
    [SerializeField, TextArea]
    string m_DisplayFormat = "Current Song:\n<i>{0}</i>\nBy {1}";
    [SerializeField]
    float m_DisplayDuration = 10f;

    public MusicTrack CurrentMusicTrack { get; set; }

    TMP_Text m_Text;

    void Awake() => m_Text = GetComponent<TMP_Text>();

    void OnEnable()
    {
        m_Text ??= GetComponent<TMP_Text>();
        m_Text.text = string.Format(m_DisplayFormat, CurrentMusicTrack.SongName, CurrentMusicTrack.ArtistName);
        StartCoroutine(Disappear());
    }

    IEnumerator Disappear()
    {
        yield return new WaitForSecondsRealtime(m_DisplayDuration);
        gameObject.SetActive(false);
    }
}
