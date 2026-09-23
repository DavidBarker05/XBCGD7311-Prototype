using UnityEngine;

public class InteractPromptManager : MonoBehaviour
{
    public static InteractPromptManager Instance { get; private set; }

    [SerializeField]
    InteractPrompt m_Prompt;
    [SerializeField]
    Camera m_Camera;

    Transform m_CurrentTarget;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void Show(Transform target, string interactText)
    {
        if (!target || m_CurrentTarget == target) return;
        m_CurrentTarget = target;
        m_Prompt.Show(target, m_Camera ? m_Camera : Camera.main, interactText);
    }

    public void Hide()
    {
        if (!m_CurrentTarget) return;
        m_CurrentTarget = null;
        m_Prompt.Hide();
    }
}
