using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueDisplayer : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_SpeakerName;
    [SerializeField]
    TMP_Text m_DialogueText;
    [SerializeField]
    Button nextButton;
    [SerializeField]
    Button m_SkipButton;
    [SerializeField]
    Button m_SkipAllButton;
    [SerializeField]
    Player m_Player;
    [SerializeField]
    DialoguePlayerCharacter m_DialogueCharacter;

    PlayerCharacter m_LastCharacter;

    DialogueItem m_CurrentItem;

    bool m_bFinishedWithCurrentItem;

    float m_SecondsPerCharacter;
    float m_CurrentReadTime;

    System.Action endCallbackFunction;

    void Awake()
    {
        nextButton.onClick.AddListener(NextDialogueItem);
        m_SkipButton.onClick.AddListener(SkipCurrentItem);
        m_SkipAllButton.onClick.AddListener(StopDisplayingDialogue);
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        nextButton.onClick.RemoveListener(NextDialogueItem);
        m_SkipButton.onClick.RemoveListener(SkipCurrentItem);
        m_SkipAllButton.onClick.RemoveListener(StopDisplayingDialogue);
    }

    void Update()
    {
        if (m_CurrentItem == null)
        {
            StopDisplayingDialogue();
            return;
        }
        if (m_bFinishedWithCurrentItem)
        {
            if (m_DialogueText.maxVisibleCharacters != m_DialogueText.text.Length) m_DialogueText.maxVisibleCharacters = m_DialogueText.text.Length;
            if (!nextButton.gameObject.activeSelf) nextButton.gameObject.SetActive(true);
            return;
        }
        if (m_CurrentItem.CharactersPerSecond < 0) m_DialogueText.maxVisibleCharacters = m_DialogueText.text.Length;
        else
        {
            m_CurrentReadTime += Time.unscaledDeltaTime;
            if (m_CurrentReadTime >= m_SecondsPerCharacter)
            {
                int numNewCharacters = (int)(m_CurrentReadTime / m_SecondsPerCharacter);
                m_DialogueText.maxVisibleCharacters = Mathf.Clamp(m_DialogueText.maxVisibleCharacters + numNewCharacters, 0, m_DialogueText.text.Length);
                m_CurrentReadTime = 0f;
            }
        }
        m_bFinishedWithCurrentItem = m_DialogueText.maxVisibleCharacters == m_DialogueText.text.Length;
    }

    void NextDialogueItem()
    {
        DialogueManager.Instance.LoadNextItem();
        RetrieveCurrentDialogue();
    }

    void SkipCurrentItem() => m_bFinishedWithCurrentItem = true;

    void RetrieveCurrentDialogue()
    {
        m_CurrentItem = DialogueManager.Instance.CurrentDialogueItem;
        if (m_CurrentItem == null)
        {
            StopDisplayingDialogue();
            return;
        }
        nextButton.gameObject.SetActive(false);
        m_SpeakerName.text = m_CurrentItem.Name;
        m_DialogueText.text = m_CurrentItem.Text;
        m_DialogueText.fontSize = m_CurrentItem.FontSize;
        m_bFinishedWithCurrentItem = false;
        m_DialogueText.maxVisibleCharacters = 0;
        m_SecondsPerCharacter = 1f / m_CurrentItem.CharactersPerSecond;
        m_CurrentReadTime = 0f;
    }

    public void StartDisplayingDialogue(System.Action callbackFunction = null)
    {
        m_LastCharacter = m_Player.CurrentPlayerCharacter;
        m_Player.ChangeCharacter(m_DialogueCharacter);
        gameObject.SetActive(true);
        RetrieveCurrentDialogue();
        endCallbackFunction = callbackFunction;
    }

    void StopDisplayingDialogue()
    {
        m_Player.ChangeCharacter(m_LastCharacter);
        m_LastCharacter = null;
        endCallbackFunction?.Invoke();
        endCallbackFunction = null;
        DialogueManager.Instance.Clear();
        gameObject.SetActive(false);
    }
}