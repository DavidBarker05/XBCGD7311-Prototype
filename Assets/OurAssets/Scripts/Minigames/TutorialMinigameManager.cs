using UnityEngine;

public class TutorialMinigameManager : MonoBehaviour
{
    public static TutorialMinigameManager Instance { get; private set; }

    public event System.Action<MinigameType> OnMinigameCompleted;

    bool[] m_MinigamesBeaten;

    public bool AllMinigamesBeaten
    {
        get
        {
            foreach (bool beaten in m_MinigamesBeaten) if (!beaten) return false;
            return true;
        }
    }

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            m_MinigamesBeaten = new bool[System.Enum.GetValues(typeof(MinigameType)).Length];
        }
    }

    public void ReportMinigameCompleted(MinigameType type)
    {
        m_MinigamesBeaten[(int)type] = true;
        OnMinigameCompleted?.Invoke(type);
    }

    public bool IsMinigameBeaten(MinigameType type) => m_MinigamesBeaten[(int)type];
}
