using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class WaypointIndicator : MonoBehaviour
{
    [SerializeField]
    Vector3 m_WorldOffset = new Vector3(0f, 2f, 0f); // Points a little above the target by default rather than through the floor

    RectTransform m_RectTransform;
    Image m_Image;
    Transform m_Target;
    Camera m_Camera;

    void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_Image = GetComponent<Image>();
    }

    public void Init(Transform target, Camera trackingCamera, Sprite icon, Vector3 worldOffset)
    {
        m_Target = target;
        m_Camera = trackingCamera;
        if (icon) m_Image.sprite = icon;
        m_WorldOffset = worldOffset;
    }

    public bool IsTracking(Transform target) => m_Target == target;

    void LateUpdate()
    {
        if (!m_Target || !m_Camera)
        {
            m_Image.enabled = false;
            return;
        }

        Vector3 screenPosition = m_Camera.WorldToScreenPoint(m_Target.position + m_WorldOffset);
        bool bInFrontOfCamera = screenPosition.z > 0f;
        m_Image.enabled = bInFrontOfCamera;
        if (bInFrontOfCamera) m_RectTransform.position = screenPosition;
    }
}
