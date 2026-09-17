using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class KeybindButton : MonoBehaviour
{
    [SerializeField]
    string m_ActionMapName = "Player";
    [SerializeField]
    string m_ActionName;
    [SerializeField]
    int m_BindingIndex = 0;
    [SerializeField]
    TextMeshProUGUI m_Label;
    [SerializeField]
    string m_WaitingForInputText = "Press a key...";

    Button m_Button;

    void Awake()
    {
        m_Button = GetComponent<Button>();
        if (!m_Label) m_Label = GetComponentInChildren<TextMeshProUGUI>();
        m_Button.onClick.AddListener(BeginRebind);
    }

    void OnEnable() => RefreshLabel();

    public void Configure(string actionMapName, string actionName, int bindingIndex)
    {
        m_ActionMapName = actionMapName;
        m_ActionName = actionName;
        m_BindingIndex = bindingIndex;
        RefreshLabel();
    }

    public void RefreshLabel()
    {
        if (!GameUserSettingsManager.Instance || !m_Label || string.IsNullOrEmpty(m_ActionName)) return;
        m_Label.text = GameUserSettingsManager.Instance.GetBindingDisplayString(m_ActionMapName, m_ActionName, m_BindingIndex);
    }

    void BeginRebind()
    {
        if (!GameUserSettingsManager.Instance || string.IsNullOrEmpty(m_ActionName)) return;
        m_Button.interactable = false;
        if (m_Label) m_Label.text = m_WaitingForInputText;
        GameUserSettingsManager.Instance.StartRebind(m_ActionMapName, m_ActionName, m_BindingIndex,
            onComplete: () => { m_Button.interactable = true; RefreshLabel(); },
            onCancel: () => { m_Button.interactable = true; RefreshLabel(); });
    }

    public void ResetToDefault()
    {
        if (!GameUserSettingsManager.Instance || string.IsNullOrEmpty(m_ActionName)) return;
        GameUserSettingsManager.Instance.ResetKeybind(m_ActionMapName, m_ActionName, m_BindingIndex);
        RefreshLabel();
    }
}
