using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    public int MinigamesBeaten { get; private set; } = 0;

    [SerializeField, Min(1)]
    int m_NumMinigamesToBeat = 3;
    [SerializeField]
    GameObject WinScreen;

    public void OnMinigameBeaten()
    {
        if (++MinigamesBeaten >= m_NumMinigamesToBeat) WinScreen.SetActive(true);
    }
}
