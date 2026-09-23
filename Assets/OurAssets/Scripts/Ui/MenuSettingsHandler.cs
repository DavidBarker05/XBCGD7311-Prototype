using System.Collections.Generic;
using UnityEngine;

public class MenuSettingsHandler : MonoBehaviour
{
    [SerializeField]
    GameObject m_MainScreen;
    [SerializeField]
    SettingsScreen m_SettingsScreen;

    readonly HashSet<GameObject> m_AdditionalScreens = new HashSet<GameObject>();
    GameObject m_CurrentScreen;

    public void OpenSettings()
    {
        if (m_CurrentScreen == m_SettingsScreen.gameObject) return;
        m_CurrentScreen = m_SettingsScreen.gameObject;
        m_MainScreen.SetActive(false);
        m_SettingsScreen.Open();
    }

    public void CloseSettings()
    {
        if (m_CurrentScreen != m_SettingsScreen.gameObject) return;
        m_CurrentScreen = m_MainScreen;
        m_MainScreen.SetActive(true);
        m_SettingsScreen.Close();
    }

    public void OpenScreen(GameObject screen)
    {
        if (!screen || m_CurrentScreen != m_MainScreen) return;
        m_AdditionalScreens.Add(screen);
        m_CurrentScreen = screen;
        m_MainScreen.SetActive(false);
        screen.SetActive(true);
    }

    public void CloseScreen(GameObject screen)
    {
        if (!screen || m_CurrentScreen != screen) return;
        m_CurrentScreen = m_MainScreen;
        screen.SetActive(false);
        m_MainScreen.SetActive(true);
    }

    public void Close()
    {
        if (m_CurrentScreen == m_SettingsScreen.gameObject) CloseSettings();
        else if (m_CurrentScreen != m_MainScreen) CloseScreen(m_CurrentScreen);
    }
}
