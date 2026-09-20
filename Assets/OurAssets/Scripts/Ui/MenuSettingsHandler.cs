using UnityEngine;

public class MenuSettingsHandler : MonoBehaviour
{
    [SerializeField]
    GameObject m_MainScreen;
    [SerializeField]
    SettingsScreen m_SettingsScreen;

    bool m_bIsInSettings = false;

    public void OpenSettings()
    {
        if (m_bIsInSettings) return;
        m_bIsInSettings = true;
        m_MainScreen.SetActive(false);
        m_SettingsScreen.Open();
    }

    public void CloseSettings()
    {
        if (!m_bIsInSettings) return;
        m_bIsInSettings = false;
        m_MainScreen.SetActive(true);
        m_SettingsScreen.Close();
    }
}
