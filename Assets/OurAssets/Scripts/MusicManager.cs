using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.ArrayUtils;

[System.Serializable]
public struct MusicTrack
{
    public AudioClip MusicClip;
    public string ArtistName;
    public string SongName;
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    static readonly Dictionary<MusicTrack, int> s_TracksOnCooldown = new Dictionary<MusicTrack, int>();
    static readonly Dictionary<MusicTrack, int> s_ChaseTracksOnCooldown = new Dictionary<MusicTrack, int>();
    // ^ static so that when changing/reloading scenes they still won't hear the same track

    [SerializeField]
    AudioSource m_MusicSource;
    [SerializeField]
    MusicTrackDisplayer m_MusicTrackDisplayer;
    [SerializeField]
    MusicTrack[] m_BackgroundMusicTracks;
    [SerializeField]
    MusicTrack[] m_ChaseBackgroundMusicTracks;
    [SerializeField, Min(0f)]
    float m_FadeDuration = 1f;
    [SerializeField, Min(0)]
    int m_TrackCooldownTime = 2;
    [SerializeField, Min(0f)]
    float m_MinTimeBetweenTracks = 20f;
    [SerializeField, Min(0f)]
    float m_MaxTimeBetweenTracks = 60f;

    bool m_bChaseActive;
    Coroutine m_FadeCoroutine;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (Instance == this) StartCoroutine(BackgroundMusicLoop());
    }

    MusicTrack BackgroundMusicTrack
    {
        get
        {
            MusicTrack backgroundMusicTrack;
            do backgroundMusicTrack = m_BackgroundMusicTracks.GetRandomElement<MusicTrack>();
            while (s_TracksOnCooldown.ContainsKey(backgroundMusicTrack));
            return backgroundMusicTrack;
        }
    }

    MusicTrack ChaseBackgroundMusicTrack
    {
        get
        {
            MusicTrack chaseBackgroundMusicTrack;
            do chaseBackgroundMusicTrack = m_ChaseBackgroundMusicTracks.GetRandomElement<MusicTrack>();
            while (s_ChaseTracksOnCooldown.ContainsKey(chaseBackgroundMusicTrack));
            return chaseBackgroundMusicTrack;
        }
    }

    void PlayMusic(MusicTrack musicTrack, bool bLoop = false)
    {
        m_MusicSource.clip = musicTrack.MusicClip;
        m_MusicSource.loop = bLoop;
        m_MusicTrackDisplayer.CurrentMusicTrack = musicTrack;
        m_MusicTrackDisplayer.gameObject.SetActive(true);
        m_MusicSource.Play();
    }

    void AddToCooldown(Dictionary<MusicTrack, int> cooldowns, MusicTrack justPlayed)
    {
        List<MusicTrack> expired = null;
        foreach (MusicTrack track in new List<MusicTrack>(cooldowns.Keys))
        {
            if (--cooldowns[track] <= 0) (expired ??= new List<MusicTrack>()).Add(track);
        }
        if (expired != null) foreach (MusicTrack track in expired) cooldowns.Remove(track);
        if (m_TrackCooldownTime > 0) cooldowns[justPlayed] = m_TrackCooldownTime;
    }

    IEnumerator BackgroundMusicLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => !m_bChaseActive);
            yield return new WaitForSecondsRealtime(Random.Range(m_MinTimeBetweenTracks, m_MaxTimeBetweenTracks));
            if (m_bChaseActive) continue;
            MusicTrack track = BackgroundMusicTrack;
            AddToCooldown(s_TracksOnCooldown, track);
            m_MusicSource.volume = 0f;
            PlayMusic(track);
            yield return FadeVolume(0f, 1f, m_FadeDuration);
            yield return new WaitWhile(() => m_MusicSource.isPlaying && m_MusicSource.clip == track.MusicClip);
        }
    }

    public void StartChaseMusic()
    {
        m_bChaseActive = true;
        if (m_FadeCoroutine != null) StopCoroutine(m_FadeCoroutine);
        m_FadeCoroutine = StartCoroutine(FadeIntoChaseMusic());
    }

    public void EndChaseMusic()
    {
        if (m_FadeCoroutine != null) StopCoroutine(m_FadeCoroutine);
        m_FadeCoroutine = StartCoroutine(FadeOutChaseMusic());
    }

    IEnumerator FadeIntoChaseMusic()
    {
        if (m_MusicSource.isPlaying) yield return FadeVolume(m_MusicSource.volume, 0f, m_FadeDuration);
        MusicTrack chaseTrack = ChaseBackgroundMusicTrack;
        AddToCooldown(s_ChaseTracksOnCooldown, chaseTrack);
        m_MusicSource.volume = 0f;
        PlayMusic(chaseTrack, bLoop: true);
        yield return FadeVolume(0f, 1f, m_FadeDuration);
    }

    IEnumerator FadeOutChaseMusic()
    {
        yield return FadeVolume(m_MusicSource.volume, 0f, m_FadeDuration);
        m_MusicSource.Stop();
        m_bChaseActive = false;
    }

    IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            m_MusicSource.volume = to;
            yield break;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            m_MusicSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        m_MusicSource.volume = to;
    }
}
