using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class MinigamesBeatenDisplayer : MonoBehaviour
{
    [SerializeField, TextArea]
    string m_DisplayText = "Minigames Beaten: {0:D}";

    TMP_Text m_Text;

    void Awake() => m_Text = GetComponent<TMP_Text>();

    void Update() => m_Text.text = string.Format(m_DisplayText, MinigameManager.Instance?.MinigamesBeaten ?? 0);
}
