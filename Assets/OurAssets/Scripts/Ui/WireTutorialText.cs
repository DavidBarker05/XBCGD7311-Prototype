using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class WireTutorialText : MonoBehaviour
{
    [SerializeField]
    WireBoard m_WireBoard;
    [SerializeField, TextArea]
    string m_DisplayText;

    TMP_Text m_Text;

    void Awake() => m_Text = GetComponent<TMP_Text>();

    void OnEnable()
    {
        m_Text ??= GetComponent<TMP_Text>();
        m_Text.text = string.Format(m_DisplayText, m_WireBoard.FailsToLoseColour);
    }
}
