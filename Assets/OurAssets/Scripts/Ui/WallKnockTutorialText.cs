using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class WallKnockTutorialText : MonoBehaviour
{
    [SerializeField]
    Wall m_Wall;
    [SerializeField, TextArea]
    string m_DisplayText;

    TMP_Text m_Text;

    void Awake() => m_Text = GetComponent<TMP_Text>();

    void OnEnable()
    {
        m_Text ??= GetComponent<TMP_Text>();
        m_Text.text = string.Format(m_DisplayText, m_Wall.MaxTries);
    }
}
