using UnityEngine;

public class Pipe : MonoBehaviour
{
    [SerializeField]
    Transform m_PipeTransform;

    GameObject m_CurrentPipePrefab;
    PipeSO m_CurrentPipeSO;
    public PipeSO CurrentPipeSO
    {
        get => m_CurrentPipeSO;
        set
        {
            if (m_CurrentPipePrefab) Destroy(m_CurrentPipePrefab);
            m_CurrentPipePrefab = null;
            if (value && value.Model)
            {
                GameObject go = Instantiate(value.Model, m_PipeTransform);
                m_CurrentPipePrefab = go;
            }
            m_CurrentPipeSO = value;
            RefreshOrientation();
        }
    }

    public PipeOrientation CurrentOrientation { get; private set; }

    public bool CanConnectLeft => CurrentOrientation.HasLeftHole;
    public bool CanConnectTop => CurrentOrientation.HasTopHole;
    public bool CanConnectRight => CurrentOrientation.HasRightHole;
    public bool CanConnectBottom => CurrentOrientation.HasBottomHole;

    PipeRotationAngle m_CurrentAngle;
    public PipeRotationAngle CurrentPipeAngle
    {
        get => m_CurrentAngle;
        set
        {
            m_PipeTransform.localEulerAngles = new Vector3(0f, (int)value, 0f);
            m_CurrentAngle = value;
            RefreshOrientation();
        }
    }

    void RefreshOrientation() => CurrentOrientation = m_CurrentPipeSO ? m_CurrentPipeSO.GetOrientationFromAngle(m_CurrentAngle) : default;

    public void RotateRight() => CurrentPipeAngle = PipeRotationAngleUtil.NextAngleRight(m_CurrentAngle);

    public void RotateLeft() => CurrentPipeAngle = PipeRotationAngleUtil.NextAngleLeft(m_CurrentAngle);
}
