using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class CurrencyDisplayText : MonoBehaviour
{
    [SerializeField]
    string m_DisplayText = "Money: {0}";

    TMP_Text m_Text;

    void Awake() => m_Text = GetComponent<TMP_Text>();

    void UpdateText() => m_Text.text = string.Format(m_DisplayText, CurrencyManager.Instance.GetMoney);

    void Start()
    {
        CurrencyManager.Instance.OnCurrencyChanged.AddListener(UpdateText);
        UpdateText();
    }
}
