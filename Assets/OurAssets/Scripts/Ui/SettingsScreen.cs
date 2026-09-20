using UnityEngine;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField]
    GameObject m_SoundSettings;
    [SerializeField]
    GameObject m_GraphicsSettings;
    [SerializeField]
    GameObject m_ControlsSettings;

    public void OpenSoundSettings()
    {
        if (m_SoundSettings.activeSelf) return;
        m_SoundSettings.SetActive(true);
        if (m_GraphicsSettings.activeSelf) m_GraphicsSettings.SetActive(false);
        if (m_ControlsSettings.activeSelf) m_ControlsSettings.SetActive(false);
        GameUserSettingsManager.Instance.ClearTempSettings();
    }

    public void OpenGraphicsSettings()
    {
        if (m_GraphicsSettings.activeSelf) return;
        if (m_SoundSettings.activeSelf) m_SoundSettings.SetActive(false);
        m_GraphicsSettings.SetActive(true);
        if (m_ControlsSettings.activeSelf) m_ControlsSettings.SetActive(false);
        GameUserSettingsManager.Instance.ClearTempSettings();
    }

    public void OpenControlsSettings()
    {
        if (m_ControlsSettings.activeSelf) return;
        if (m_SoundSettings.activeSelf) m_SoundSettings.SetActive(false);
        if (m_GraphicsSettings.activeSelf) m_GraphicsSettings.SetActive(false);
        m_ControlsSettings.SetActive(true);
        GameUserSettingsManager.Instance.ClearTempSettings();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        OpenSoundSettings();
    }

    public void Close()
    {
        GameUserSettingsManager.Instance.ClearTempSettings();
        gameObject.SetActive(false);
    }
}
