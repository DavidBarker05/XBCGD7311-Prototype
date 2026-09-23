using UnityEngine;

public class MinigameTutorialsScreen : MonoBehaviour
{
    [SerializeField]
    GameObject m_WiresTutorial;
    [SerializeField]
    GameObject m_WallKnockTutorial;
    [SerializeField]
    GameObject m_PipeTutorial;
    [SerializeField]
    GameObject m_ChaseTutorial;

    void OnEnable() => OpenWiresTutorial();

    public void OpenWiresTutorial()
    {
        if (m_WiresTutorial.activeSelf) return;
        m_WiresTutorial.SetActive(true);
        if (m_WallKnockTutorial.activeSelf) m_WallKnockTutorial.SetActive(false);
        if (m_PipeTutorial.activeSelf) m_PipeTutorial.SetActive(false);
        if (m_ChaseTutorial.activeSelf) m_ChaseTutorial.SetActive(false);
    }

    public void OpenWallKnockTutorial()
    {
        if (m_WallKnockTutorial.activeSelf) return;
        if (m_WiresTutorial.activeSelf) m_WiresTutorial.SetActive(false);
        m_WallKnockTutorial.SetActive(true);
        if (m_PipeTutorial.activeSelf) m_PipeTutorial.SetActive(false);
        if (m_ChaseTutorial.activeSelf) m_ChaseTutorial.SetActive(false);
    }

    public void OpenPipeTutorial()
    {
        if (m_PipeTutorial.activeSelf) return;
        if (m_WiresTutorial.activeSelf) m_WiresTutorial.SetActive(false);
        if (m_WallKnockTutorial.activeSelf) m_WallKnockTutorial.SetActive(false);
        m_PipeTutorial.SetActive(true);
        if (m_ChaseTutorial.activeSelf) m_ChaseTutorial.SetActive(false);
    }

    public void OpenChaseTutorial()
    {
        if (m_ChaseTutorial.activeSelf) return;
        if (m_WiresTutorial.activeSelf) m_WiresTutorial.SetActive(false);
        if (m_WallKnockTutorial.activeSelf) m_WallKnockTutorial.SetActive(false);
        if (m_PipeTutorial.activeSelf) m_PipeTutorial.SetActive(false);
        m_ChaseTutorial.SetActive(true);
    }
}
