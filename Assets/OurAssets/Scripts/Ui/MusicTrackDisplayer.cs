using System.Collections;
using TMPro;
using UnityEngine;

public class MusicTrackDisplayer : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_Text;
    [SerializeField, TextArea]
    string m_DisplayFormat = "Current Song:\n<i>{0}</i>\nBy {1}";
    [SerializeField]
    float m_DisplayDuration = 10f;

    public MusicTrack CurrentMusicTrack { get; set; }

    void OnEnable()
    {
        m_Text.text = string.Format(m_DisplayFormat, CurrentMusicTrack.SongName, CurrentMusicTrack.ArtistName);
        StartCoroutine(Disappear());
    }

    IEnumerator Disappear()
    {
        yield return new WaitForSecondsRealtime(m_DisplayDuration);
        gameObject.SetActive(false);
    }
}
