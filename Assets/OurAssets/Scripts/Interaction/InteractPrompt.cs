using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class InteractPrompt : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_Text;

    RectTransform m_RectTransform;
    CanvasGroup m_CanvasGroup;
    Transform m_Target;
    Camera m_Camera;

    void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_CanvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    public void Show(Transform target, Camera trackingCamera, string interactText)
    {
        m_Target = target;
        m_Camera = trackingCamera;
        m_Text.text = interactText;
    }

    public void Hide()
    {
        m_Target = null;
        if (m_CanvasGroup) m_CanvasGroup.alpha = 0f;
    }

    void LateUpdate()
    {
        if (!m_Target || !m_Camera)
        {
            if (m_CanvasGroup) m_CanvasGroup.alpha = 0f;
            return;
        }
        Vector3 screenPosition = m_Camera.WorldToScreenPoint(m_Target.position);
        bool bInFrontOfCamera = screenPosition.z > 0f;
        if (m_CanvasGroup) m_CanvasGroup.alpha = bInFrontOfCamera ? 1f : 0f;
        if (bInFrontOfCamera) m_RectTransform.position = screenPosition;
    }
}
